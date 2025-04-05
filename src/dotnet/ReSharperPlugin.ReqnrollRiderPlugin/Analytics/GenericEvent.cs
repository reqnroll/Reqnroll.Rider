using System.Collections.Generic;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

public class GenericEvent(string eventName, Dictionary<string, string> properties = null) : IAnalyticsEvent
{

    public string EventName { get; } = eventName;
    public Dictionary<string, string> Properties { get; } = properties ?? new Dictionary<string, string>();
}