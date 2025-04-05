using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Paths;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Extensions;

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

    public static bool IsReqnrollProject(this IProject project)
    {
        var isReqnrollProject = false;
        var assemblies = project.GetAllReferencedAssemblies();
        var nugetPackages = project.GetAllPackagesReferences().ToArray();
        if (assemblies.Any(a => a.Name == "Reqnroll") ||
            nugetPackages.Any(p => p.Name.IndexOf("Reqnroll", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            isReqnrollProject = true;
        }
        return isReqnrollProject;
    }
}