using System.Threading.Tasks;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    public interface IAnalyticsTransmitterSink
    {
        Task TransmitEvent(IAnalyticsEvent analyticsEvent, string userId);
    }
}