using System.Linq;
using JetBrains.Application.Components;
using JetBrains.DataFlow;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Features.SolutionBuilders;
using JetBrains.ProjectModel.ProjectsHost;
using JetBrains.ProjectModel.ProjectsHost.SolutionHost;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Extensions;

namespace ReSharperPlugin.SpecflowRiderPlugin.ProjectRefresher
{
    [SolutionComponent]
    public class ProjectRefresher
    {
        private readonly ISolution solution;

        public ProjectRefresher(ISolution solution, ISolutionBuilder solutionBuilder, Lifetime lifetime)
        {
            this.solution = solution;
            solutionBuilder.RunningRequest.ForEachValue_NotNull(lifetime, (_, request) => HandleNewRequest(request));
        }

        private void HandleNewRequest(SolutionBuilderRequest request)
        {
            var myProjectsHostContainer = solution.ProjectsHostContainer();
            var solutionHost = myProjectsHostContainer.GetComponent<ISolutionHost>();
            request.AfterBuildCompleted.Advise(request.Lifetime, () =>
            {
                using (ReadLockCookie.Create()) 
                {  
                    var specFlowProjects = request.Projects.Where(p => p.Project.IsSpecFlowProject()).Select(p => p.Project);
                    solutionHost.ReloadProjectsAsync(specFlowProjects.Select(x => x.GetProjectMark()).WhereNotNull().ToList());
                }
            });   
        }
    }
}