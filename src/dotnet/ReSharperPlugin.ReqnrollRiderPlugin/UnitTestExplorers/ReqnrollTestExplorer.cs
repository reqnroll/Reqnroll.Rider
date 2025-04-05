using System;
using System.Linq;
using JetBrains.Application.Parts;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestFramework.Exploration;
using JetBrains.ReSharper.UnitTestFramework.Exploration.Daemon;
using JetBrains.ReSharper.UnitTestFramework.Persistence;
using JetBrains.ReSharper.UnitTestProvider.nUnit.v30;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using JetBrains.Util;
using Reqnroll.Tracing;

namespace ReSharperPlugin.ReqnrollRiderPlugin.UnitTestExplorers;

[SolutionComponent(Instantiation.DemandAnyThreadUnsafe )]
internal class ReqnrollTestExplorer(
    ReqnrollUnitTestProvider unitTestProvider,
    IUnitTestElementRepository unitTestElementRepository,
    ILogger logger
) : IUnitTestExplorerFromFile
{
    public IUnitTestProvider Provider { get; } = unitTestProvider;

    public void ProcessFile(
        IFile psiFile,
        IUnitTestElementObserverOnFile observer,
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

        var featureTests = unitTestElementRepository.GetRelatedFeatureTests(gherkinFile, logger);
        if (featureTests == null)
            return;

        //A gherkin file can only contain one feature definition
        var feature = gherkinFile.GetFeatures().FirstOrDefault();
        var featureTest = featureTests.FirstOrDefault();
        if (featureTest == null || feature == null)
            return;

        observer.OnUnitTestElementDisposition(
            featureTest,
            feature.GetDocumentRange().TextRange,
            feature.GetDocumentRange().TextRange
        );

        foreach (var scenario in feature.GetScenarios())
        {
            var scenarioText = scenario.GetScenarioText();
            if (string.IsNullOrWhiteSpace(scenarioText))
                continue;

            var matchingTest = featureTest.Children.FirstOrDefault(t => GetDescriptionFromAttributes(t) == scenarioText
                                                                        || CompareDescriptionWithShortName(scenarioText, t)
            );
            if (matchingTest == null)
                continue;

            observer.OnUnitTestElementDisposition(
                matchingTest,
                scenario.GetDocumentRange().TextRange,
                scenario.GetDocumentRange().TextRange
            );
        }
    }

    private bool CompareDescriptionWithShortName(string scenarioText, IUnitTestElement relatedTest)
    {
        switch (relatedTest.Provider.ID)
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
        switch (relatedTest.Provider.ID)
        {
            case NUnitTestProvider.PROVIDER_ID:
            {
                if (relatedTest.GetDeclaredElement() is IAttributesOwner c)
                    return c.GetAttributeInstances(NUnitDescriptionAttribute, false)
                        .FirstOrDefault()
                        ?.PositionParameter(0).ConstantValue.StringValue;
                break;
            }
            case "MSTest":
            {
                if (relatedTest.GetDeclaredElement() is IAttributesOwner c)
                    return c.GetAttributeInstances(MsTestDescriptionAttribute, false)
                        .FirstOrDefault()
                        ?.PositionParameter(0).ConstantValue.StringValue;
                break;
            }
            case "xUnit":
            {
                if (relatedTest.GetDeclaredElement() is IAttributesOwner c)
                    return c.GetAttributeInstances(XUnitTraitAttribute, false)
                        .FirstOrDefault(x => x.PositionParameter(0).ConstantValue.StringValue == "Description")
                        ?.PositionParameter(1).ConstantValue.StringValue;
                break;
            }
        }
        return null;
    }
}