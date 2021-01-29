namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon
{
    public abstract class SpecflowHighlightingBase
    {
        // ErrorsGen makes IsValid override if we specify a base class
        public abstract bool IsValid();
    }
}