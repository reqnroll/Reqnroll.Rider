using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Application;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

[ShellComponent]
public class PluginTracker
{
    public PluginTracker(IAnalyticsTransmitter transmitter, IRiderInstallationStatusService installationStatusService)
    {
        var today = DateTime.Today;
        var currentPluginVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var statusData = installationStatusService.GetRiderInstallationStatus();

        if (new Version(statusData.InstalledVersion) < currentPluginVersion)
        {
            transmitter.TransmitRuntimeEvent(new GenericEvent("Rider Extension upgraded", new Dictionary<string, string>()
            {
                {"OldExtensionVersion", statusData.InstalledVersion}
            }));
            statusData.InstallDate = today;
            statusData.InstalledVersion = currentPluginVersion.ToString();
            installationStatusService.SaveNewStatus(statusData);
        }

        transmitter.TransmitRuntimeEvent(new GenericEvent("Rider Extension loaded"));
    }
}