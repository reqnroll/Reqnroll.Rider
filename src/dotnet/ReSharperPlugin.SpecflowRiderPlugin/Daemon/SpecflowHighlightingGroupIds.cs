using JetBrains.ReSharper.Feature.Services.Daemon;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon
{
    [RegisterConfigurableHighlightingsGroup(Specflow, "Specflow")]
    public static class SpecflowHighlightingGroupIds
    {
        public const string Specflow = "SPECFLOW";
    }
}