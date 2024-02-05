using System.Collections.Generic;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Guidance
{
    public interface IGuidanceConfiguration
    {
        IEnumerable<GuidanceStep> UsageSequence { get; }
    }
}