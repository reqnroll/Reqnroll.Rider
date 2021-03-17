using System.Collections.Generic;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    public interface IAnalyticsEvent
    {
        string EventName { get; }

        Dictionary<string, string> Properties { get; }
    }

}