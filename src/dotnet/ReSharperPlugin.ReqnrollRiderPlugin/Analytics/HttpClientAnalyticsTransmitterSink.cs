using System;
using System.Threading.Tasks;
using JetBrains.Application;
using JetBrains.Util;
using Newtonsoft.Json;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

[ShellComponent]
public class HttpClientAnalyticsTransmitterSink(
    IHttpClientProvider httpClientProvider,
    IEnvironmentReqnrollTelemetryChecker environmentReqnrollTelemetryChecker,
    AppInsightsConfiguration appInsightsConfiguration,
    ILogger logger)
    : IAnalyticsTransmitterSink
{
    private readonly Uri _appInsightsDataCollectionEndPoint = new Uri("https://dc.services.visualstudio.com/v2/track");

    public async Task TransmitEvent(IAnalyticsEvent analyticsEvent, string userId)
    {
        if (!environmentReqnrollTelemetryChecker.IsReqnrollTelemetryEnabled())
            return;
            
        try
        {
            await TransmitEventAsync(analyticsEvent, userId);
        }
        catch (Exception e)
        {
            logger.Verbose(e);
        }
    }

    private async Task TransmitEventAsync(IAnalyticsEvent analyticsEvent, string userId)
    {
        var eventTelemetry = new AppInsightsEventTelemetry(userId, appInsightsConfiguration.InstrumentationKey, analyticsEvent);
        var content = JsonConvert.SerializeObject(eventTelemetry);
        await httpClientProvider.PostStringAsync(_appInsightsDataCollectionEndPoint, content);            
    }
}