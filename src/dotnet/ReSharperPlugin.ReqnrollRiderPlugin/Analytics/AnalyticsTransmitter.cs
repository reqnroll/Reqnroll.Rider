using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Application;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics
{

    [ShellComponent]
    public class AnalyticsTransmitter : IAnalyticsTransmitter
    {
        private readonly IAnalyticsTransmitterSink _analyticsTransmitterSink;
        private readonly IReqnrollUserIdStore _reqnrollUserIdStore;
        private readonly IApplicationHost _applicationHost;

        public AnalyticsTransmitter(
            IAnalyticsTransmitterSink analyticsTransmitterSink,
            IReqnrollUserIdStore reqnrollUserIdStore,
            IApplicationHost applicationHost)
        {
            _analyticsTransmitterSink = analyticsTransmitterSink;
            _reqnrollUserIdStore = reqnrollUserIdStore;
            _applicationHost = applicationHost;
        } 

        public async Task TransmitRuntimeEvent(IAnalyticsEvent runtimeEvent)
        {
            var ideVersion = _applicationHost.HostProductInfo.VersionMarketingString;
            var reqnrollId = _reqnrollUserIdStore.GetUserId();
            var currentPluginVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var properties = runtimeEvent.Properties;
            properties.Add("Ide", "JetBrains Rider");
            properties.Add("IdeVersion", ideVersion);
            properties.Add("ExtensionVersion", currentPluginVersion.ToString());
            await _analyticsTransmitterSink.TransmitEvent(runtimeEvent, reqnrollId);
        }
    }
}