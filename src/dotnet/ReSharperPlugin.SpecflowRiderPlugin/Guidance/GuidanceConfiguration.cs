using System.Collections.Generic;
using JetBrains.Application;

namespace ReSharperPlugin.SpecflowRiderPlugin.Guidance
{
    [ShellComponent]
    public class GuidanceConfiguration : IGuidanceConfiguration
    {
        public GuidanceConfiguration()
        {
            UsageSequence = new[]
            {
                new GuidanceStep(GuidanceNotification.AfterInstall, 1, @"https://specflow.org/welcome-to-specflow/"),
                new GuidanceStep(GuidanceNotification.TwoDayUsage, 2, "https://specflow.org/ide-onboarding-two-days/"),
                new GuidanceStep(GuidanceNotification.FiveDayUsage, 5, "https://specflow.org/vs-onboarding-five-days/"),
                new GuidanceStep(GuidanceNotification.TenDayUsage, 10, "https://specflow.org/beyond-the-basics/"),
                new GuidanceStep(GuidanceNotification.TwentyDayUsage, 20, null),
                new GuidanceStep(GuidanceNotification.HundredDayUsage, 100, "https://specflow.org/experienced/"),
                new GuidanceStep(GuidanceNotification.TwoHundredDayUsage, 200, "https://specflow.org/veteran/")
            };
        }

        public IEnumerable<GuidanceStep> UsageSequence { get; }
    }
}