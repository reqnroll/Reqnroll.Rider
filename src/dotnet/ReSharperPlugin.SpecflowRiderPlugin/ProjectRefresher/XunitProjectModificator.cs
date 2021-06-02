using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.ProjectModel.ProjectsHost;
using JetBrains.ProjectModel.ProjectsHost.MsBuild.Strategies;

namespace ReSharperPlugin.SpecflowRiderPlugin.ProjectRefresher
{
    [ShellComponent]
    public class XunitProjectModificator : MsBuildDefaultLoadStrategy.IModificator
    {
        public bool IsApplicable(IProjectMark projectMark)
        {
            return true;
        }
        
        public void Modify(List<string> targets)
        {
            //Temporary workaround for the Xunit error issue https://youtrack.jetbrains.com/issue/RIDER-62536
            targets.Add("GenerateSpecFlowAssemblyHooksFileTask");
        }
    }
}