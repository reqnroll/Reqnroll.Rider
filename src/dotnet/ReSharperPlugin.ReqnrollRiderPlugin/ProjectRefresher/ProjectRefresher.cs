using System.Linq;
using JetBrains.Application.Components;
using JetBrains.Application.Parts;
using JetBrains.DataFlow;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Features.SolutionBuilders;
using JetBrains.ProjectModel.ProjectsHost;
using JetBrains.ProjectModel.ProjectsHost.SolutionHost;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Extensions;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ProjectRefresher;

[SolutionComponent(Instantiation.DemandAnyThreadUnsafe)]
public class ProjectRefresher
{
    private readonly ISolution _solution;

    public ProjectRefresher(ISolution solution, ISolutionBuilder solutionBuilder, Lifetime lifetime)
    {
        _solution = solution;
        solutionBuilder.RunningRequest.ForEachValue_NotNull(lifetime, (_, request) => HandleNewRequest(request));
    }

    private void HandleNewRequest(SolutionBuilderRequest request)
    {
        var myProjectsHostContainer = _solution.ProjectsHostContainer();
        var solutionHost = myProjectsHostContainer.GetComponent<ISolutionHost>();
        request.AfterBuildCompleted.Advise(request.Lifetime, () =>
        {
            using (ReadLockCookie.Create()) 
            {  
                var reqnrollProjects = request.Projects.Where(p => p.Project.IsReqnrollProject()).Select(p => p.Project);
                solutionHost.ReloadProjectsAsync(reqnrollProjects.Select(x => x.GetProjectMark()).WhereNotNull().ToList());
            }
        });   
    }
}