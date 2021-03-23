namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    public interface IRiderInstallationStatusService
    {
        RiderInstallationStatus GetRiderInstallationStatus();
        void SaveNewStatus(RiderInstallationStatus newStatus);
    }
}