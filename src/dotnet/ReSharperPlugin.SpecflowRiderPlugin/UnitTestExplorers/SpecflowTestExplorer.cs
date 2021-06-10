using System;
using System.Linq;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Exploration;
using JetBrains.ReSharper.UnitTestProvider.nUnit.v30;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using TechTalk.SpecFlow.Tracing;

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

            var featureTests = _unitTestElementRepository.GetRelatedFeatureTests(gherkinFile);
            if (featureTests == null)
                return;

            //A gherkin file can only contain one feature definition
            var feature = gherkinFile.GetFeatures().FirstOrDefault();
            var featureTest = featureTests.FirstOrDefault();
            if (featureTest == null || feature == null)
                return;

            observer.OnUnitTestElementDisposition(new UnitTestElementDisposition(
                featureTest,
                projectFile,
                feature.GetDocumentRange().TextRange,
                feature.GetDocumentRange().TextRange
            ));

            foreach (var scenario in feature.GetScenarios())
            {
                var scenarioText = scenario.GetScenarioText();
                if (string.IsNullOrWhiteSpace(scenarioText))
                    continue;
                
                var matchingTest = featureTest.Children.FirstOrDefault(t => GetDescriptionFromAttributes(t) == scenarioText 
                                                                            || CompareDescriptionWithShortName(scenarioText, t));
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

        private bool CompareDescriptionWithShortName(string scenarioText, IUnitTestElement relatedTest)
        {
            switch (relatedTest.Id.ProviderId)
            {
                case NUnitTestProvider.PROVIDER_ID:
                case "MSTest":
                {
                    var scenarioTextWithoutSpace = scenarioText.ToIdentifier();
                    return string.Compare(scenarioTextWithoutSpace, relatedTest.ShortName, StringComparison.InvariantCultureIgnoreCase) == 0;

                }
                case "xUnit":
                {
                    return scenarioText == relatedTest.ShortName;
                }
            }
            return false;
        }

        public static readonly ClrTypeName NUnitDescriptionAttribute = new ClrTypeName("NUnit.Framework.DescriptionAttribute");
        public static readonly ClrTypeName XUnitTraitAttribute = new ClrTypeName("Xunit.TraitAttribute");
        public static readonly ClrTypeName MsTestDescriptionAttribute = new ClrTypeName("Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute");

        private string GetDescriptionFromAttributes(IUnitTestElement relatedTest)
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
                case "MSTest":
                {
                    if (relatedTest.GetDeclaredElement() is IAttributesOwner c)
                        return c.GetAttributeInstances(MsTestDescriptionAttribute, false)
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
    }
}