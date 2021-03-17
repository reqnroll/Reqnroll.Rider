using System;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    public class RiderInstallationStatus
    {
        public string InstalledVersion { get; set; }
        
        public DateTime? InstallDate { get; set; }
        
        public DateTime? LastUsedDate { get; set; }
        
        public int UsageDays { get; set; }
        
        public int UserLevel { get; set; }
    }
}