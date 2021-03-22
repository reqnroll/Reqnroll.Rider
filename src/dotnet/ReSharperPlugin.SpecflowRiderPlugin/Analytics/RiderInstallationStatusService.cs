using System;
using System.IO;
using System.Reflection;
using JetBrains.Application;
using Newtonsoft.Json;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    [ShellComponent]
    public class RiderInstallationStatusService : IRiderInstallationStatusService
    {
        private readonly IAnalyticsTransmitter _analyticsTransmitter;
        private RiderInstallationStatus currentStatusData;
        private static readonly string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string specFlowFolder = Path.Combine(appDataFolder, "SpecFlow");
        private static readonly string specflowRiderPluginFilePath = Path.Combine(specFlowFolder, "specflowriderplugin.json");

        public RiderInstallationStatusService(IAnalyticsTransmitter analyticsTransmitter)
        {
            _analyticsTransmitter = analyticsTransmitter;
        }

        public RiderInstallationStatus GetRiderInstallationStatus()
        {
            var today = DateTime.Today;
            var currentPluginVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (currentStatusData == null)
            {
                currentStatusData = new RiderInstallationStatus()
                {
                    InstallDate = today,
                    InstalledVersion = currentPluginVersion.ToString(),
                    LastUsedDate = today,
                    UsageDays = 1
                };
                if (!File.Exists(specflowRiderPluginFilePath))
                {
                    _analyticsTransmitter.TransmitRuntimeEvent(new GenericEvent("Rider Extension installed"));
                    SaveNewStatus(currentStatusData);
                }
                else
                {
                    try
                    {
                        currentStatusData = JsonConvert.DeserializeObject<RiderInstallationStatus>(
                            File.ReadAllText(specflowRiderPluginFilePath));
                    }
                    catch
                    {
                        //NOP
                    }
                }
            }
            return currentStatusData;
        }

        public void SaveNewStatus(RiderInstallationStatus newStatus)
        {
            try
            {
                currentStatusData = newStatus;
                File.WriteAllText(specflowRiderPluginFilePath, JsonConvert.SerializeObject(currentStatusData));
            }
            catch
            {
                //NOP
            }
        }
    }
}