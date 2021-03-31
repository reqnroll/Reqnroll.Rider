namespace ReSharperPlugin.SpecflowRiderPlugin.Guidance
{
    public class GuidanceStep
    {

        public GuidanceStep(GuidanceNotification userLevel, int? usageDays, string url)
        {
            UserLevel = userLevel;
            UsageDays = usageDays;
            Url = url;
        }

        public GuidanceNotification UserLevel { get; }

        public int? UsageDays { get; }

        public string Url { get; }
    }
}