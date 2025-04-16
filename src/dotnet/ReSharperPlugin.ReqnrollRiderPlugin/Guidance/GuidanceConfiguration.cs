using System.Collections.Generic;
using JetBrains.Application;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Guidance;

[ShellComponent]
public class GuidanceConfiguration : IGuidanceConfiguration
{

    public IEnumerable<GuidanceStep> UsageSequence { get; } =
    [
        new GuidanceStep(GuidanceNotification.AfterInstall, 1, @"https://reqnroll.net/tools/reqnroll/onboarding-day-1/", "Ready to start using Reqnroll?", "Get your personalized learning material", "Start now"),
        new GuidanceStep(GuidanceNotification.TwoDayUsage, 2, "https://reqnroll.net/tools/reqnroll/onboarding-day-2/", "Curious how to write better Gherkin feature files?", "Take a look into our helpful list of tips and tricks", "Learn more"),
        new GuidanceStep(GuidanceNotification.FiveDayUsage, 5, "https://reqnroll.net/tools/reqnroll/onboarding-day-5/", "How was your first experience with Reqnroll?", "Help us to further improve your automation experience", "Leave feedback"),
        new GuidanceStep(GuidanceNotification.TenDayUsage, 10, "https://reqnroll.net/tools/reqnroll/onboarding-day-10/", "Ready to become a Reqnroll expert?", "Join one of our online courses or trainings", "Learn more"),
        new GuidanceStep(GuidanceNotification.TwentyDayUsage, 20, null, "","",""),
        new GuidanceStep(GuidanceNotification.HundredDayUsage, 100, "https://reqnroll.net/tools/reqnroll/onboarding-day-100/", "Congrats, you are now a Reqnroll expert!", "Help us to make it easier to get started as beginner", "Leave feedback"),
        new GuidanceStep(GuidanceNotification.TwoHundredDayUsage, 200, "https://reqnroll.net/tools/reqnroll/onboarding-day-200/", "Congrats, you are now a Reqnroll expert!", "Help us to make it easier to get started as beginner", "Leave feedback"),
    ];
}