using System.Threading.Tasks;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics
{
    public interface IAnalyticsTransmitterSink
    {
        Task TransmitEvent(IAnalyticsEvent analyticsEvent, string userId);
    }
}