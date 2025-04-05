using System.Collections.Generic;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

public interface IAnalyticsEvent
{
    string EventName { get; }

    Dictionary<string, string> Properties { get; }
}