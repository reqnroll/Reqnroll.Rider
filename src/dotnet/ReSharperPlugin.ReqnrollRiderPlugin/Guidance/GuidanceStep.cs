namespace ReSharperPlugin.ReqnrollRiderPlugin.Guidance;

public class GuidanceStep(GuidanceNotification userLevel, int? usageDays, string url, string title, string content, string linkText)
{

    public GuidanceNotification UserLevel { get; } = userLevel;

    public int? UsageDays { get; } = usageDays;

    public string Url { get; } = url;

    public string Title { get; } = title;

    public string Content { get; } = content;

    public string LinkText { get; } = linkText;
}