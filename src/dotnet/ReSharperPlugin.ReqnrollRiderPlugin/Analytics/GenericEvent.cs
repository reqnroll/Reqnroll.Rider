using System.Collections.Generic;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics
{
    public class GenericEvent : IAnalyticsEvent
    {
        public GenericEvent(string eventName, Dictionary<string, string> properties = null)
        {
            EventName = eventName;
            Properties = properties ?? new Dictionary<string, string>();
        }

        public string EventName { get; }
        public Dictionary<string, string> Properties { get; }
    }
}