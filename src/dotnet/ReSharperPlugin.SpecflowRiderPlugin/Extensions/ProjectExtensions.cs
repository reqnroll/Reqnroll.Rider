using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Paths;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Extensions
{
    public static class ProjectExtensions
    {
        [CanBeNull]
        public static ICSharpFile GetCSharpFile(this IProject project, string filename)
        {
            var sourceFileInProject = project.GetPsiSourceFileInProject(VirtualFileSystemPath.Parse(filename, InteractionContext.Local, FileSystemPathInternStrategy.INTERN));
            return sourceFileInProject?.GetPsiFiles<CSharpLanguage>().SafeOfType<ICSharpFile>().SingleOrDefault();
        }

        [CanBeNull]
        public static GherkinFile GetGherkinFile(this IProject project, string filename)
        {
            var sourceFileInProject = project.GetPsiSourceFileInProject(VirtualFileSystemPath.Parse(filename, InteractionContext.Local, FileSystemPathInternStrategy.INTERN));
            return sourceFileInProject?.GetPsiFiles<GherkinLanguage>().SafeOfType<GherkinFile>().SingleOrDefault();
        }

        public static bool IsSpecFlowProject(this IProject project)
        {
            var isSpecFlowProject = false;
            var assemblies = project.GetAllReferencedAssemblies();
            var nugetPackages = project.GetAllPackagesReferences().ToArray();
            if (assemblies.Any(a => a.Name == "TechTalk.SpecFlow") ||
                nugetPackages.Any(p => p.Name.IndexOf("SpecFlow", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                isSpecFlowProject = true;
            }
            return isSpecFlowProject;
        }
    }
}