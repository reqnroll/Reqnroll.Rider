using System.Collections.Generic;

namespace ReSharperPlugin.SpecflowRiderPlugin.Guidance
{
    public interface IGuidanceConfiguration
    {
        IEnumerable<GuidanceStep> UsageSequence { get; }
    }
}