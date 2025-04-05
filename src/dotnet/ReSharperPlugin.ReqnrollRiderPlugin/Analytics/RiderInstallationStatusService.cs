using System;
using System.IO;
using System.Reflection;
using JetBrains.Application;
using Newtonsoft.Json;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

[ShellComponent]
public class RiderInstallationStatusService(IAnalyticsTransmitter analyticsTransmitter) : IRiderInstallationStatusService
{
    private RiderInstallationStatus _currentStatusData;
    private static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
    private static readonly string ReqnrollFolder = Path.Combine(AppDataFolder, "Reqnroll");
    private static readonly string ReqnrollRiderPluginFilePath = Path.Combine(ReqnrollFolder, "reqnrollriderplugin.json");

    public RiderInstallationStatus GetRiderInstallationStatus()
    {
        var today = DateTime.Today;
        var currentPluginVersion = Assembly.GetExecutingAssembly().GetName().Version;

        if (_currentStatusData == null)
        {
            _currentStatusData = new RiderInstallationStatus()
            {
                InstallDate = today,
                InstalledVersion = currentPluginVersion.ToString(),
                LastUsedDate = today,
                UsageDays = 1
            };
            if (!File.Exists(ReqnrollRiderPluginFilePath))
            {
                analyticsTransmitter.TransmitRuntimeEvent(new GenericEvent("Rider Extension installed"));
                SaveNewStatus(_currentStatusData);
            }
            else
            {
                try
                {
                    _currentStatusData = JsonConvert.DeserializeObject<RiderInstallationStatus>(
                        File.ReadAllText(ReqnrollRiderPluginFilePath));
                }
                catch
                {
                    //NOP
                }
            }
        }
        return _currentStatusData;
    }

    public void SaveNewStatus(RiderInstallationStatus newStatus)
    {
        try
        {
            _currentStatusData = newStatus;
            File.WriteAllText(ReqnrollRiderPluginFilePath, JsonConvert.SerializeObject(_currentStatusData));
        }
        catch
        {
            //NOP
        }
    }
}