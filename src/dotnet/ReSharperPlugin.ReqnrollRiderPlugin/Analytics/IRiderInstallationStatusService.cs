namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

public interface IRiderInstallationStatusService
{
    RiderInstallationStatus GetRiderInstallationStatus();
    void SaveNewStatus(RiderInstallationStatus newStatus);
}