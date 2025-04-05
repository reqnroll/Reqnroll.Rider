using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Application;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

[ShellComponent]
public class AnalyticsTransmitter(
    IAnalyticsTransmitterSink analyticsTransmitterSink,
    IReqnrollUserIdStore reqnrollUserIdStore,
    IApplicationHost applicationHost)
    : IAnalyticsTransmitter
{

    public async Task TransmitRuntimeEvent(IAnalyticsEvent runtimeEvent)
    {
        var ideVersion = applicationHost.HostProductInfo.VersionMarketingString;
        var reqnrollId = reqnrollUserIdStore.GetUserId();
        var currentPluginVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var properties = runtimeEvent.Properties;
        properties.Add("Ide", "JetBrains Rider");
        properties.Add("IdeVersion", ideVersion);
        properties.Add("ExtensionVersion", currentPluginVersion.ToString());
        await analyticsTransmitterSink.TransmitEvent(runtimeEvent, reqnrollId);
    }
}