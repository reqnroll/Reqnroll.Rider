using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Threading;
using JetBrains.Collections;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Session;
using JetBrains.Util;
using JetBrains.Util.Collections;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.FailedStep;
using ReSharperPlugin.SpecflowRiderPlugin.Extensions;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.TestOutput;
using GroupingEvent = JetBrains.Threading.GroupingEvent;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.ExecutionFailedStep
{
    [SolutionComponent]
    public class ExecutionFailedStepGutterIconUpdater
    {
        private static readonly ClrTypeName NunitDescriptionAttribute = new ClrTypeName("NUnit.Framework.DescriptionAttribute");
        private readonly ILogger _myLogger;
        [NotNull] private readonly FailedStepCache _failedStepCache;
        [NotNull] private readonly IPsiFiles _psiFiles;
        private readonly IDictionary<IUnitTestElement, (IUnitTestSession session, UnitTestResult result)> _updatedUnitTests;
        private readonly GroupingEvent _myResultUpdated;
        private readonly IUnitTestResultManager _unitTestResultManager;

        public ExecutionFailedStepGutterIconUpdater(
            Lifetime lifetime,
            [NotNull] IShellLocks shellLocks,
            [NotNull] IUnitTestResultManager resultManager,
            [NotNull] ILogger logger,
            [NotNull] FailedStepCache failedStepCache,
            [NotNull] IPsiFiles psiFiles
        )
        {
            _myLogger = logger;
            _failedStepCache = failedStepCache;
            _psiFiles = psiFiles;
            _updatedUnitTests = new Dictionary<IUnitTestElement, (IUnitTestSession session, UnitTestResult result)>(UnitTestElement.EqualityComparer);
            _myResultUpdated = shellLocks.NotNull("shellLocks != null")
                .CreateGroupingEvent(lifetime, nameof(ExecutionFailedStepGutterIconUpdater) + "::ResultUpdated", 500.Milliseconds(), OnProcessUpdated);
            _unitTestResultManager = resultManager;
            resultManager.NotNull("resultManager != null").UnitTestResultUpdated.Advise(lifetime, OnUnitTestResultUpdated);
        }

        private void OnProcessUpdated()
        {
            IDictionary<IUnitTestElement, (IUnitTestSession session, UnitTestResult result)> set;
            lock (_updatedUnitTests)
            {
                set = _updatedUnitTests.ToDictionary(e => e.Key, e => e.Value);
                _updatedUnitTests.Clear();
            }
            using (_myLogger.UsingLogBracket(LoggingLevel.TRACE, "Updating gutter mark icons for {0} elements", set.Count))
            {
                HashSet<IPsiSourceFile> updatedFiles;
                using (ReadLockCookie.Create())
                    updatedFiles = UpdateIconsInActiveDocuments(set);

                using (WriteLockCookie.Create())
                    foreach (var psiSourceFile in updatedFiles)
                        _psiFiles.MarkAsDirty(psiSourceFile);
            }
        }

        private HashSet<IPsiSourceFile> UpdateIconsInActiveDocuments(IDictionary<IUnitTestElement, (IUnitTestSession session, UnitTestResult result)> updatedUnitTests)
        {
            var updatedFiles = new HashSet<IPsiSourceFile>();
            foreach (var (testElement, (session, result)) in updatedUnitTests)
            {
                var declaredElement = testElement.GetDeclaredElement();
                if (declaredElement == null)
                    continue;
                if (!(testElement.GetDeclaredElement() is IMethod methodTestDeclaration))
                    continue;
                var psiSourceFile = declaredElement.GetSourceFiles().SingleItem;
                if (psiSourceFile?.Name.EndsWith(".feature.cs") != true)
                    continue;

                var project = psiSourceFile.GetProject();
                var gherkinFile = project?.GetGherkinFile(psiSourceFile.Name.Substring(0, psiSourceFile.Name.Length - 3));

                var gherkinDocument = gherkinFile?.GetSourceFile()?.Document;
                if (gherkinDocument == null)
                    continue;

                string featureText;
                string scenarioText;

                using (CompilationContextCookie.GetOrCreate(project.GetResolveContext()))
                {
                    var scenarioAttributeDescription = methodTestDeclaration.GetAttributeInstances(NunitDescriptionAttribute, false).FirstOrDefault();
                    if (scenarioAttributeDescription == null || scenarioAttributeDescription.PositionParameterCount < 1)
                        continue;
                    var featureAttributeDescription = methodTestDeclaration.GetContainingType()?.GetAttributeInstances(NunitDescriptionAttribute, false).FirstOrDefault();
                    if (featureAttributeDescription == null || featureAttributeDescription.PositionParameterCount < 1)
                        continue;
                    featureText = featureAttributeDescription.PositionParameter(0).ConstantValue.Value as string;
                    scenarioText = scenarioAttributeDescription.PositionParameter(0).ConstantValue.Value as string;
                }

                if (!result.Status.GetResultStatus().Has(UnitTestStatus.Failed) && !result.Status.GetResultStatus().Has(UnitTestStatus.Inconclusive))
                {
                    if (_failedStepCache.RemoveFailedStep(gherkinFile.GetSourceFile(), featureText, scenarioText))
                        updatedFiles.Add(gherkinFile.GetSourceFile());
                    continue;
                }

                var feature = gherkinFile.GetFeature(featureText);
                var scenario = feature?.GetScenario(scenarioText);
                if (scenario == null)
                    continue;

                var testResult = _unitTestResultManager.GetResultData(testElement, session);
                var steps = scenario.GetSteps().ToList();
                if (steps.Count == 0)
                    continue;

                var gherkinKeywordProvider = scenario.GetPsiServices().LanguageManager.GetService<GherkinKeywordProvider>(GherkinLanguage.Instance.NotNull());
                var invalidSteps = GetFailedTestOutput(gherkinFile.Lang, testResult, gherkinKeywordProvider);
                if (invalidSteps == null)
                    continue;

                if (_failedStepCache.AddFailedStep(gherkinFile.GetSourceFile(), featureText, scenarioText, invalidSteps))
                    updatedFiles.Add(gherkinFile.GetSourceFile());
            }
            return updatedFiles;
        }

        private List<StepTestOutput> GetFailedTestOutput(string lang, UnitTestResultData testResult, GherkinKeywordProvider gherkinKeywordProvider)
        {
            var parser = new OutputTestParser(GetTestOutputByLine(testResult), gherkinKeywordProvider.GetKeywordsList(lang));

            var stepOutputs = parser.ParseOutput().ToList();
            if (stepOutputs.Any(x => x.Status != StepTestOutput.StepStatus.Done))
                return stepOutputs;

            return null;
        }

        public IEnumerable<string> GetTestOutputByLine(UnitTestResultData testResult)
        {
            for (var i = 0; i < testResult.OutputChunks; i++)
            {
                var chunk = testResult.GetOutputChunk(i);
                var lines = chunk.SplitByNewLine();

                foreach (var line in lines)
                    yield return line;
            }
        }

        private void OnUnitTestResultUpdated(UnitTestResultEventArgs e)
        {
            if (e.Result.Status.Has(UnitTestStatus.Running | UnitTestStatus.Pending))
                return;
            lock (_updatedUnitTests)
                _updatedUnitTests[e.Element] = (e.Session, e.Result);
            _myResultUpdated.FireIncomingDontProlongate();
        }
    }
}