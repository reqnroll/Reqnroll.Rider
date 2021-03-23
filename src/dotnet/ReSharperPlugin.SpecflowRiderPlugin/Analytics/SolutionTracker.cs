using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Impl;
using JetBrains.ProjectModel.Tasks;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
 {
     [SolutionComponent]
     public class SolutionTracker
     {
         public SolutionTracker(ISolution solution, ISolutionLoadTasksScheduler solutionLoadTasksScheduler, IAnalyticsTransmitter transmitter, IRiderInstallationStatusService riderInstallationStatusService)
         {
             solutionLoadTasksScheduler.EnqueueTask(new SolutionLoadTask("SpecFlow", SolutionLoadTaskKinds.Done, () =>
                 {
                     var projects = ((SolutionElement)solution).GetAllProjects();
                     var isSpecFlowSolution = false;
                     foreach (var project in projects)
                     {
                         var targetFrameworks = project.TargetFrameworkIds;
                         var assemblies = project.GetModuleReferences(targetFrameworks.First());
                         if (assemblies.Any(a => a.Name == "TechTalk.SpecFlow"))
                         {
                             isSpecFlowSolution = true;
                             transmitter.TransmitRuntimeEvent(new GenericEvent("Rider SpecFlow loaded", new Dictionary<string, string>()
                             {
                                 {"ProjectTargetFramework", string.Join(";", targetFrameworks.Select(t => t.PresentableString))}
                             }));
                         }
                     }
                     if (isSpecFlowSolution)
                     {
                         var statusData = riderInstallationStatusService.GetRiderInstallationStatus();
                         var today = DateTime.Today;
                         if (statusData.LastUsedDate != today)
                         {
                             statusData.UsageDays++;
                             statusData.LastUsedDate = today;
                         }
                         riderInstallationStatusService.SaveNewStatus(statusData);
                     }
                 }
             ));
         }
     }
}