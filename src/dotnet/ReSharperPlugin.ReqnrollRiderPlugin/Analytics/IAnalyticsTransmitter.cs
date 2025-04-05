using System.Threading.Tasks;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

public interface IAnalyticsTransmitter
{ 
    Task TransmitRuntimeEvent(IAnalyticsEvent runtimeEvent);
}