using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Notifications;
using JetBrains.Application.StdApplicationUI;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Impl;
using JetBrains.ProjectModel.Tasks;
using ReSharperPlugin.ReqnrollRiderPlugin.Extensions;
using ReSharperPlugin.ReqnrollRiderPlugin.Guidance;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics
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
             solutionLoadTasksScheduler.EnqueueTask(new SolutionLoadTask("Reqnroll", SolutionLoadTaskKinds.AsLateAsPossible, () =>
                 {
                     var projects = ((SolutionElement)solution).GetAllProjects();
                     var isReqnrollSolution = false;
                     foreach (var project in projects)
                     {
                         var targetFrameworks = project.TargetFrameworkIds;
                         if(project.IsReqnrollProject())
                         {
                             isReqnrollSolution = true;
                             transmitter.TransmitRuntimeEvent(new GenericEvent("Rider Reqnroll loaded", new Dictionary<string, string>()
                             {
                                 {"ProjectTargetFramework", string.Join(";", targetFrameworks.Select(t => t.PresentableString))}
                             }));
                         }
                     }
                     if (isReqnrollSolution)
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

         private static void ShowNotification(OpensUri opensUri, GuidanceStep guidance)
         {
             if (!opensUri.IsInternetConnected())
                 return;

             opensUri.OpenUri(new Uri(guidance.Url));
         }
     }
}