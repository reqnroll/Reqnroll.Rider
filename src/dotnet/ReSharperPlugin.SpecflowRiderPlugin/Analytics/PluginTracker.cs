using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Application;
using Newtonsoft.Json;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    [ShellComponent]
    public class PluginTracker
    {
        public PluginTracker(IAnalyticsTransmitter transmitter)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var specFlowFolder = Path.Combine(appDataFolder, "SpecFlow");
            var specflowRiderPluginFilePath = Path.Combine(specFlowFolder, "specflowriderplugin.json");
            var today = DateTime.Today;
            RiderInstallationStatus statusData;
            var currentPluginVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (!File.Exists(specflowRiderPluginFilePath))
            {
                transmitter.TransmitRuntimeEvent(new GenericEvent("Rider Extension installed"));
                statusData = new RiderInstallationStatus()
                {
                    InstallDate = today,
                    InstalledVersion = currentPluginVersion.ToString(),
                    LastUsedDate = today,
                    UsageDays = 1
                };
            }
            else
            {
                statusData = JsonConvert.DeserializeObject<RiderInstallationStatus>(
                    File.ReadAllText(specflowRiderPluginFilePath));
            }

            if (statusData.LastUsedDate != today)
            {
                statusData.UsageDays++;
                statusData.LastUsedDate = today;
            }

            if (new Version(statusData.InstalledVersion) < currentPluginVersion)
            {
                transmitter.TransmitRuntimeEvent(new GenericEvent("Rider Extension upgraded", new Dictionary<string, string>()
                {
                    {"OldExtensionVersion", statusData.InstalledVersion}
                }));
                statusData.InstallDate = today;
                statusData.InstalledVersion = currentPluginVersion.ToString();
            }

            File.WriteAllText(specflowRiderPluginFilePath, JsonConvert.SerializeObject(statusData));

            transmitter.TransmitRuntimeEvent(new GenericEvent("Rider Extension loaded"));
        }
    }

}