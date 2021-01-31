using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util.Dotnet.TargetFrameworkIds;

namespace ReSharperPlugin.SpecflowRiderPlugin.UnitTestExplorers
{
    [UnitTestProvider]
    public class SpecflowUnitTestProvider : IUnitTestProvider
    {
        public string ID => "SPECFLOW";
        public string Name  => "SpecFlow";

        public bool IsElementOfKind(IDeclaredElement declaredElement, UnitTestElementKind elementKind)
        {
            throw new System.NotImplementedException();
        }
        public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind)
        {
            throw new System.NotImplementedException();
        }
        public bool IsSupported(IHostProvider hostProvider, IProject project, TargetFrameworkId targetFrameworkId)
        {
            throw new System.NotImplementedException();
        }
        public bool IsSupported(IProject project, TargetFrameworkId targetFrameworkId)
        {
            return project.GetAssemblyReferences(targetFrameworkId).Any(x => x.Name == "TechTalk.SpecFlow");
        }
        public int CompareUnitTestElements(IUnitTestElement x, IUnitTestElement y)
        {
            throw new System.NotImplementedException();
        }
        public bool SupportsResultEventsForParentOf(IUnitTestElement element)
        {
            throw new System.NotImplementedException();
        }
    }
}