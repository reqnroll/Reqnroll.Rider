using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Parts;
using JetBrains.Application.Threading;
using JetBrains.Collections;
using JetBrains.Diagnostics;
using JetBrains.DocumentManagers;
using JetBrains.Lifetimes;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestFramework.Execution;
using JetBrains.ReSharper.UnitTestFramework.Session;
using JetBrains.Util;
using JetBrains.Util.Collections;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.FailedStep;
using ReSharperPlugin.ReqnrollRiderPlugin.Extensions;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils.TestOutput;
using GroupingEvent = JetBrains.Threading.GroupingEvent;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.ExecutionFailedStep
{
    [SolutionComponent(Instantiation.DemandAnyThreadUnsafe)]
    public class ExecutionFailedStepGutterIconUpdater
    {
        private static readonly ClrTypeName MsTestPropertyAttribute = new ClrTypeName("Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute");
        private static readonly ClrTypeName MsTestDescriptionAttribute = new ClrTypeName("Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute");
        private static readonly ClrTypeName XunitTraitAttribute = new ClrTypeName("Xunit.TraitAttribute");
        private static readonly ClrTypeName NunitDescriptionAttribute = new ClrTypeName("NUnit.Framework.DescriptionAttribute");
        private readonly ILogger _myLogger;
        [NotNull] private readonly FailedStepCache _failedStepCache;
        [NotNull] private readonly IDaemon _daemon;
        private readonly IDictionary<IUnitTestElement, (IUnitTestSession session, UnitTestResult result)> _updatedUnitTests;
        private readonly GroupingEvent _myResultUpdated;
        private readonly IUnitTestResultManager _unitTestResultManager;

        public ExecutionFailedStepGutterIconUpdater(
            Lifetime lifetime,
            [NotNull] IShellLocks shellLocks,
            [NotNull] IUnitTestResultManager resultManager,
            [NotNull] ILogger logger,
            [NotNull] FailedStepCache failedStepCache,
            [NotNull] IDaemon daemon
        )
        {
            _myLogger = logger;
            _failedStepCache = failedStepCache;
            _daemon = daemon;
            _updatedUnitTests = new Dictionary<IUnitTestElement, (IUnitTestSession session, UnitTestResult result)>(UnitTestElement.Comparer.ById); // FIXME: or ByNaturalId ?
            _myResultUpdated = shellLocks.NotNull()
                .CreateGroupingEvent(lifetime, nameof(ExecutionFailedStepGutterIconUpdater) + "::ResultUpdated", 500.Milliseconds(), OnProcessUpdated);
            _unitTestResultManager = resultManager;
            UT.Events.Result.Updated.Subscribe(lifetime, OnUnitTestResultUpdated);
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
                {
                    updatedFiles = UpdateIconsInActiveDocuments(set);
                    foreach (var psiSourceFile in updatedFiles)
                        _daemon.ForceReHighlight(psiSourceFile.Document);
                }
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
                var featureCsPsiSourceFile = declaredElement.GetSourceFiles().SingleItem;
                if (featureCsPsiSourceFile?.Name.EndsWith(".feature.cs") != true)
                    continue;

                var project = featureCsPsiSourceFile.GetProject();
                if (project == null)
                    continue;
                var filePath = featureCsPsiSourceFile.Document.TryGetFilePath().MakeRelativeTo(project.Location);
                var gherkinFile = project.GetGherkinFile(filePath.FullPath.Substring(0, filePath.FullPath.Length - 3));

                var gherkinDocument = gherkinFile?.GetSourceFile()?.Document;
                if (gherkinDocument == null)
                    continue;

                if (!ReadFeatureAndScenarioTextFromAttributeInFeatureCs(project, methodTestDeclaration, out var featureText, out var scenarioText))
                    continue;

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

        private static bool ReadFeatureAndScenarioTextFromAttributeInFeatureCs(IProject project, IMethod methodTestDeclaration, out string featureText, out string scenarioText)
        {
            featureText = null;
            scenarioText = null;
            using (CompilationContextCookie.GetOrCreate(project.GetResolveContext()))
            {
                if (ReadFromNunit(methodTestDeclaration, ref featureText, ref scenarioText))
                    return true;
                if (ReadFromXUnit(methodTestDeclaration, ref featureText, ref scenarioText))
                    return true;
                if (ReadFromMsTest(methodTestDeclaration, ref featureText, ref scenarioText))
                    return true;
            }
            return true;
        }

        private static bool ReadFromNunit(IMethod methodTestDeclaration, ref string featureText, ref string scenarioText)
        {
            var scenarioAttributeDescription = methodTestDeclaration.GetAttributeInstances(NunitDescriptionAttribute, false).FirstOrDefault();
            if (scenarioAttributeDescription == null || scenarioAttributeDescription.PositionParameterCount < 1)
                return false;
            var featureAttributeDescription = methodTestDeclaration.GetContainingType()?.GetAttributeInstances(NunitDescriptionAttribute, false).FirstOrDefault();
            if (featureAttributeDescription == null || featureAttributeDescription.PositionParameterCount < 1)
                return false;

            featureText = featureAttributeDescription.PositionParameter(0).ConstantValue.StringValue;
            scenarioText = scenarioAttributeDescription.PositionParameter(0).ConstantValue.StringValue;
            return true;
        }

        private static bool ReadFromXUnit(IMethod methodTestDeclaration, ref string featureText, ref string scenarioText)
        {
            var xUnitTraitAttributes = methodTestDeclaration.GetAttributeInstances(XunitTraitAttribute, false);
            var scenarioAttributeDescription = xUnitTraitAttributes.FirstOrDefault(x => x.PositionParameter(0).ConstantValue.StringValue == "Description");
            if (scenarioAttributeDescription == null || scenarioAttributeDescription.PositionParameterCount < 2)
                return false;
            var featureAttributeDescription = xUnitTraitAttributes.FirstOrDefault(x => x.PositionParameter(0).ConstantValue.StringValue == "FeatureTitle");
            if (featureAttributeDescription == null || featureAttributeDescription.PositionParameterCount < 2)
                return false;

            featureText = featureAttributeDescription.PositionParameter(1).ConstantValue.StringValue;
            scenarioText = scenarioAttributeDescription.PositionParameter(1).ConstantValue.StringValue;
            return true;
        }

        private static bool ReadFromMsTest(IMethod methodTestDeclaration, ref string featureText, ref string scenarioText)
        {
            var msTestDescriptionAttribute = methodTestDeclaration.GetAttributeInstances(MsTestDescriptionAttribute, false).FirstOrDefault();
            if (msTestDescriptionAttribute == null || msTestDescriptionAttribute.PositionParameterCount < 1)
                return false;
            var msTestPropertyAttributes = methodTestDeclaration.GetAttributeInstances(MsTestPropertyAttribute, false);
            var featureAttributeDescription = msTestPropertyAttributes.FirstOrDefault(x => x.PositionParameter(0).ConstantValue.StringValue == "FeatureTitle");
            if (featureAttributeDescription == null || featureAttributeDescription.PositionParameterCount < 2)
                return false;

            featureText = featureAttributeDescription.PositionParameter(1).ConstantValue.StringValue;
            scenarioText = msTestDescriptionAttribute.PositionParameter(0).ConstantValue.StringValue;
            return true;
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
            for (var i = 0; i < testResult.OutputChunksCount; i++)
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