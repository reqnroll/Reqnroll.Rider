using JetBrains.ReSharper.Feature.Services.Daemon;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon
{
    [RegisterConfigurableHighlightingsGroup(Reqnroll, "Reqnroll")]
    public static class ReqnrollHighlightingGroupIds
    {
        public const string Reqnroll = "REQNROLL";
    }
}