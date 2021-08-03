using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Notifications;
using JetBrains.Application.StdApplicationUI;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Impl;
using JetBrains.ProjectModel.Tasks;
using ReSharperPlugin.SpecflowRiderPlugin.Extensions;
using ReSharperPlugin.SpecflowRiderPlugin.Guidance;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
 {
     [SolutionComponent]
     public class SolutionTracker
     {
         public SolutionTracker(ISolution solution, 
                                ISolutionLoadTasksScheduler solutionLoadTasksScheduler, 
                                IAnalyticsTransmitter transmitter, 
                                IRiderInstallationStatusService riderInstallationStatusService, 
                                IGuidanceConfiguration guidanceConfiguration, 
                                UserNotifications userNotifications,
                                Lifetime lifetime,
                                OpensUri opensUri)
         {
             solutionLoadTasksScheduler.EnqueueTask(new SolutionLoadTask("SpecFlow", SolutionLoadTaskKinds.AsLateAsPossible, () =>
                 {
                     var projects = ((SolutionElement)solution).GetAllProjects();
                     var isSpecFlowSolution = false;
                     foreach (var project in projects)
                     {
                         var targetFrameworks = project.TargetFrameworkIds;
                         if(project.IsSpecFlowProject())
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

                         var guidance = guidanceConfiguration.UsageSequence
                             .LastOrDefault(i => statusData.UsageDays >= i.UsageDays && statusData.UserLevel < (int)i.UserLevel);
                
                         if (guidance?.UsageDays != null)
                         {
                             if (guidance.Url != null)
                             {

                                 userNotifications.CreateNotification(lifetime,
                                     NotificationSeverity.INFO,
                                     guidance.Title,
                                     body: guidance.Content,

                                     executed: new UserNotificationCommand(guidance.LinkText, () => ShowNotification(opensUri, guidance)));
                             }
                             
                             transmitter.TransmitRuntimeEvent(new GenericEvent($"{guidance.UsageDays.Value} day usage"));

                             statusData.UserLevel = (int)guidance.UserLevel;
                         }
                         
                         riderInstallationStatusService.SaveNewStatus(statusData);

                     }
                 }
             ));
         }

         private static bool ShowNotification(OpensUri opensUri, GuidanceStep guidance)
         {
             if (!opensUri.IsInternetConnected())
                 return false;
                         
             opensUri.OpenUri(new Uri(guidance.Url));
             return true;
         }
     }
}