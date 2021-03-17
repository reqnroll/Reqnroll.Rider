using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    /// <summary>
    /// For property names, check: https://github.com/microsoft/ApplicationInsights-dotnet/tree/develop/BASE/Schema/PublicSchema
    /// For tags, check: https://github.com/microsoft/ApplicationInsights-dotnet/blob/develop/BASE/Schema/PublicSchema/ContextTagKeys.bond
    /// </summary>
    public class AppInsightsEventTelemetry
    {
        [JsonProperty("name")]
        public string DataTypeName { get; set; }

        [JsonProperty("time")]
        public string EventDateTime { get; set; }

        [JsonProperty("iKey")]
        public string InstrumentationKey { get; set; }

        [JsonProperty("data")]
        public TelemetryData TelemetryData { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, string> TelemetryTags { get; set; }

        private const string DefaultValue = "undefined";

        public AppInsightsEventTelemetry(string userId, string instrumentationKey, IAnalyticsEvent analyticsEvent)
        {
            InstrumentationKey = instrumentationKey;

            DataTypeName = $"Microsoft.ApplicationInsights.{InstrumentationKey}.Event";

            EventDateTime = DateTime.UtcNow.ToString("O");

            TelemetryTags = new Dictionary<string, string>()
            {
                { "ai.user.id", userId },
                { "ai.user.accountId", userId }
            };

            TelemetryData = new TelemetryData
            {
                ItemTypeName = "EventData",
                TelemetryDataItem = new TelemetryDataItem
                {
                    EventName = analyticsEvent.EventName,
                    Properties = new Dictionary<string, string>(analyticsEvent.Properties)
                    {
                        { "UserId", userId },
                        { "Platform", GetOSPlatform() ?? DefaultValue },
                        { "PlatformDescription", RuntimeInformation.OSDescription },
                    }
                }
            };
        }

        private string GetOSPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "Windows";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "Linux";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "OSX";
            }

            return null;
        }
    }

    public class TelemetryData
    {
        [JsonProperty("baseType")]
        public string ItemTypeName { get; set; }

        [JsonProperty("baseData")]
        public TelemetryDataItem TelemetryDataItem { get; set; }
    }

    public class TelemetryDataItem
    {
        [JsonProperty("ver")]
        public string EndPointSchemaVersion => "2";
        [JsonProperty("name")]
        public string EventName { get; set; }
        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }
    }
}