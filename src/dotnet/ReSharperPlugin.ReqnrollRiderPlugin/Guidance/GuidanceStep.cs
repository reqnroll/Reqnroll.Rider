namespace ReSharperPlugin.ReqnrollRiderPlugin.Guidance
{
    public class GuidanceStep
    {

        public GuidanceStep(GuidanceNotification userLevel, int? usageDays, string url, string title, string content, string linkText)
        {
            UserLevel = userLevel;
            UsageDays = usageDays;
            Url = url;
            Title = title;
            Content = content;
            LinkText = linkText;

        }

        public GuidanceNotification UserLevel { get; }

        public int? UsageDays { get; }

        public string Url { get; }

        public string Title { get; }

        public string Content { get; }

        public string LinkText { get; }
    }
}