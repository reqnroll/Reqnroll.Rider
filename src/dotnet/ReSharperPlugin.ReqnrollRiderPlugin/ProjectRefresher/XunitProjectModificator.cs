using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ProjectModel.ProjectsHost;
using JetBrains.ProjectModel.ProjectsHost.MsBuild.Strategies;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ProjectRefresher;

[ShellComponent(Instantiation.DemandAnyThreadSafe)]
public class XunitProjectModificator : MsBuildDefaultLoadStrategy.IModificator
{
    public bool IsApplicable(IProjectMark projectMark)
    {
        return true;
    }

    public void ModifyProperties(IDictionary<string, string> properties)
    {
    }

    public void ModifyTargets(List<string> targets)
    {
        //Temporary workaround for the Xunit error issue https://youtrack.jetbrains.com/issue/RIDER-62536
        targets.Add("GenerateReqnrollAssemblyHooksFileTask");
    }

}