using JetBrains.Application;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    [ShellComponent]
    public class AppInsightsConfiguration
    {
        public string InstrumentationKey { get; set; } = "<InstrumentationKeyGoesHere>";
    }
}