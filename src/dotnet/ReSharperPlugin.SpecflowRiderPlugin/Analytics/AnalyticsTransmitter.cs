using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Application;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{

    [ShellComponent]
    public class AnalyticsTransmitter : IAnalyticsTransmitter
    {
        private readonly IAnalyticsTransmitterSink _analyticsTransmitterSink;
        private readonly ISpecFlowUserIdStore _specFlowUserIdStore;
        private readonly IApplicationHost _applicationHost;

        public AnalyticsTransmitter(
            IAnalyticsTransmitterSink analyticsTransmitterSink,
            ISpecFlowUserIdStore specFlowUserIdStore,
            IApplicationHost applicationHost)
        {
            _analyticsTransmitterSink = analyticsTransmitterSink;
            _specFlowUserIdStore = specFlowUserIdStore;
            _applicationHost = applicationHost;
        } 

        public async Task TransmitRuntimeEvent(IAnalyticsEvent runtimeEvent)
        {
            var ideVersion = _applicationHost.HostProductInfo.VersionMarketingString;
            var specFlowId = _specFlowUserIdStore.GetUserId();
            var currentPluginVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var properties = runtimeEvent.Properties;
            properties.Add("Ide", "JetBrains Rider");
            properties.Add("IdeVersion", ideVersion);
            properties.Add("ExtensionVersion", currentPluginVersion.ToString());
            await _analyticsTransmitterSink.TransmitEvent(runtimeEvent, specFlowId);
        }
    }
}