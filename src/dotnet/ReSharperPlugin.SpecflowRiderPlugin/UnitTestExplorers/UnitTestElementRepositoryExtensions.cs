using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.JavaScript.Util.Literals;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework.Criteria;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestFramework.Persistence;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using TechTalk.SpecFlow.Tracing;

namespace ReSharperPlugin.SpecflowRiderPlugin.UnitTestExplorers
{
    public static class UnitTestElementRepositoryExtensions
    {
        public static List<IUnitTestElement> GetRelatedFeatureTests(this IUnitTestElementRepository unitTestElementRepository, GherkinFile gherkinFile)
        {
            var projectTests = unitTestElementRepository.Query(new ProjectCriterion(gherkinFile.GetProject().NotNull()))
                .ToList();

            var generatedClassName = GetGeneratedClassName(gherkinFile);
            var generatedNamespace = GetGeneratedNamespace(gherkinFile);
            var relatedTests = projectTests
                .Where(t => string.Compare(t.ShortName, generatedClassName, StringComparison.InvariantCultureIgnoreCase) == 0)
                .Where(x => x.GetNamespace().QualifiedName == generatedNamespace)
                .ToList();

            if (relatedTests.Count == 0)
                return null;

            var featureTests = relatedTests.Where(x => x.IsOfKind(UnitTestElementKind.TestContainer)).ToList();
            return featureTests;
        }

        private static string GetGeneratedNamespace(GherkinFile gherkinFile)
        {
            var sb = new StringBuilder();
            foreach (var projectItem in gherkinFile.GetSourceFile().ToProjectFile().GetPathChain().Reverse())
            {
                if (projectItem is IProjectFolder projectFolder)
                {
                    if (projectFolder.IsSolutionFolder())
                        continue;
                    NamespaceHelper.ConvertFolderNameToToIdentifierPart(projectFolder.Name, sb);
                    sb.Append('.');
                }
            }
            if (sb.Length > 0)
                sb.Length--;
            return sb.ToString();
        }

        private static string GetGeneratedClassName(GherkinFile gherkinFile)
        {
            return $"{gherkinFile.GetFeatures().FirstOrDefault()?.GetFeatureText()?.ToIdentifier()}Feature";
        }
    }
}
