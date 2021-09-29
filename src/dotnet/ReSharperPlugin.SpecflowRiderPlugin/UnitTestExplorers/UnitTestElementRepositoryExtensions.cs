using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework.Criteria;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestFramework.Persistence;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using TechTalk.SpecFlow.Tracing;

namespace ReSharperPlugin.SpecflowRiderPlugin.UnitTestExplorers
{
    public static class UnitTestElementRepositoryExtensions
    {
        public static List<IUnitTestElement> GetRelatedFeatureTests(this IUnitTestElementRepository unitTestElementRepository, GherkinFile gherkinFile)
        {
            var projectTests = unitTestElementRepository.Query(new ProjectCriterion(gherkinFile.GetProject()))
                .ToList();

            var relatedTests = projectTests
                .Where(t => string.Compare(t.ShortName,
                    GetGeneratedClassName(gherkinFile), StringComparison.InvariantCultureIgnoreCase) == 0).ToList();
            if (relatedTests.Count == 0)
                return null;

            var featureTests = relatedTests.Where(x => x.IsOfKind(UnitTestElementKind.TestContainer)).ToList();
            return featureTests;
        }

        private static string GetGeneratedClassName(GherkinFile gherkinFile)
        {
            return $"{gherkinFile.GetFeatures().FirstOrDefault()?.GetFeatureText()?.ToIdentifier()}Feature";
        }
    }
}