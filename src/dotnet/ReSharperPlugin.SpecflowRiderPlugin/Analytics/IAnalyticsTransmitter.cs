using System.Threading.Tasks;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    public interface IAnalyticsTransmitter
    { 
        Task TransmitRuntimeEvent(IAnalyticsEvent runtimeEvent);
    }
}