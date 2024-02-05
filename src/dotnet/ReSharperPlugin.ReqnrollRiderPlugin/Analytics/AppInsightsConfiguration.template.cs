using JetBrains.Application;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics
{
    [ShellComponent]
    public class AppInsightsConfiguration
    {
        public string InstrumentationKey { get; set; } = "<InstrumentationKeyGoesHere>";
    }
}