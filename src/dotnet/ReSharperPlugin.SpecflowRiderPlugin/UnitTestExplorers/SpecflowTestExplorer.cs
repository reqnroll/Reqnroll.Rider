using System;
using System.Linq;
using JetBrains.Diagnostics;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Exploration;
using JetBrains.ReSharper.UnitTestProvider.nUnit.v30;
using JetBrains.ReSharper.UnitTestProvider.Xunit;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.UnitTestExplorers
{
    [SolutionComponent]
    internal class SpecflowTestExplorer : IUnitTestExplorerFromFile
    {
        private readonly IUnitTestElementRepository _unitTestElementRepository;
        public IUnitTestProvider Provider { get; }

        public SpecflowTestExplorer(
            SpecflowUnitTestProvider unitTestProvider,
            IUnitTestElementRepository unitTestElementRepository
        )
        {
            Provider = unitTestProvider;
            _unitTestElementRepository = unitTestElementRepository;
        }

        public void ProcessFile(
            IFile psiFile,
            IUnitTestElementsObserver observer,
            Func<bool> interrupted)
        {
            if (!(psiFile is GherkinFile gherkinFile))
                return;

            var project = psiFile.GetProject();
            if (project == null)
                return;

            var projectFile = gherkinFile.GetSourceFile().ToProjectFile();
            if (projectFile == null)
                return;

            var projectTests = _unitTestElementRepository.Query()
                .Where(t => t.Id.Project.Guid == project.Guid)
                .ToList();

            var featureCsFile = GetRelatedFeatureFile(psiFile);
            if (featureCsFile == null)
                return;

            var relatedTests = projectTests.Where(t => t.GetProjectFiles()?.Contains(featureCsFile) == true).ToList();
            if (relatedTests.Count == 0)
                return;

            var featureTests = relatedTests.Where(x => x.IsOfKind(UnitTestElementKind.TestContainer)).ToList();
            foreach (var feature in gherkinFile.GetFeatures())
            {
                var featureText = feature.GetFeatureText();
                var featureTest = featureTests.FirstOrDefault(t => GetDescription(t) == featureText) ?? featureTests.FirstOrDefault();
                if (featureTest == null)
                    continue;

                observer.OnUnitTestElementDisposition(new UnitTestElementDisposition(
                    featureTest,
                    projectFile,
                    feature.GetDocumentRange().TextRange,
                    feature.GetDocumentRange().TextRange
                ));

                foreach (var scenario in feature.GetScenarios())
                {
                    var scenarioText = scenario.GetScenarioText();
                    var matchingTest = featureTest.Children.FirstOrDefault(t => GetDescription(t) == scenarioText);
                    if (matchingTest == null)
                        continue;

                    observer.OnUnitTestElementDisposition(new UnitTestElementDisposition(
                        matchingTest,
                        projectFile,
                        scenario.GetDocumentRange().TextRange,
                        scenario.GetDocumentRange().TextRange
                    ));
                }
            }
        }

        public static readonly ClrTypeName NUnitDescriptionAttribute = new ClrTypeName("NUnit.Framework.DescriptionAttribute");
        public static readonly ClrTypeName XUnitTraitAttribute = new ClrTypeName("Xunit.TraitAttribute");
        private string GetDescription(IUnitTestElement relatedTest)
        {
            switch (relatedTest.Id.ProviderId)
            {
                case NUnitTestProvider.PROVIDER_ID:
                {
                    if (relatedTest.GetDeclaredElement() is IAttributesOwner c)
                        return c.GetAttributeInstances(NUnitDescriptionAttribute, false)
                            .FirstOrDefault()
                            ?.PositionParameter(0).ConstantValue.Value as string;
                    break;
                }
                case "xUnit":
                {
                    if (relatedTest.GetDeclaredElement() is IAttributesOwner c)
                        return c.GetAttributeInstances(XUnitTraitAttribute, false)
                            .FirstOrDefault(x => x.PositionParameter(0).ConstantValue.Value as string == "Description")
                            ?.PositionParameter(1).ConstantValue.Value as string;
                    break;
                }
            }
            return null;
        }

        private IProjectFile GetRelatedFeatureFile(IFile psiFile)
        {
            var fileLocation = psiFile.GetSourceFile().NotNull().ToProjectFile()?.Location;
            if (fileLocation == null)
                return null;
            var expectedFeatureCsFileName = fileLocation.Name.Replace(".feature", ".feature.cs");
            var file = psiFile.GetProject()
                .GetAllProjectFiles()
                .FirstOrDefault(f => f.Location.Parent == fileLocation.Parent && f.Name == expectedFeatureCsFileName);

            return file;
        }

        private string ToUnitTestName(string text)
        {
            return text.Replace(" ", "").Replace("-", "_").Replace("(", "").Replace(")", "");
        }
    }
}