using System;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Exploration;
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

            foreach (var feature in gherkinFile.GetFeatures())
            {
                var folder = projectFile.ParentFolder;
                var testIdPrefix = project.Name + ".";
                while (folder != null && !ReferenceEquals(folder, project))
                {
                    if (folder.Path != null)
                        testIdPrefix += folder.Path.ShortName + ".";
                    folder = folder.ParentFolder;
                }
                testIdPrefix += ToUnitTestName(feature.GetFeatureText());

                var testIdPrefixWithSolution = project.GetSolution().Name + "." + testIdPrefix;

                var featureTests = projectTests.Where(x =>
                                                      {
                                                          if (x.Id.ProviderId == "xUnit")
                                                              return x.Id.Id.StartsWith(testIdPrefixWithSolution, StringComparison.InvariantCultureIgnoreCase);
                                                          return x.Id.Id.StartsWith(testIdPrefix, StringComparison.InvariantCultureIgnoreCase);
                                                      }).ToList();
                if (featureTests.Count == 0)
                    continue;

                IUnitTestElement parent = null;

                foreach (var scenario in feature.GetScenarios())
                {
                    var matchingTest = featureTests.FirstOrDefault(t => t.Id.Id.EndsWith("." + ToUnitTestName(scenario.GetScenarioText()), StringComparison.InvariantCultureIgnoreCase));
                    if (matchingTest == null)
                        continue;

                    parent = matchingTest.Parent;

                    observer.OnUnitTestElementDisposition(new UnitTestElementDisposition(
                        matchingTest,
                        projectFile,
                        scenario.GetDocumentRange().TextRange,
                        scenario.GetDocumentRange().TextRange
                    ));

                    Console.WriteLine(matchingTest);
                }

                if (parent != null)
                {
                    observer.OnUnitTestElementDisposition(new UnitTestElementDisposition(
                        parent,
                        projectFile,
                        feature.GetDocumentRange().TextRange,
                        feature.GetDocumentRange().TextRange
                    ));
                }
            }
        }

        private string ToUnitTestName(string text)
        {
            return text.Replace(" ", "").Replace("-", "_");
        }
    }
}