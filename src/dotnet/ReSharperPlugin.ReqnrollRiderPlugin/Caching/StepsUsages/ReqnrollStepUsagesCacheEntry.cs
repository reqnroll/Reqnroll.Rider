using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsUsages
{
    public class ReqnrollStepUsageCacheEntry
    {
        public ReqnrollStepUsageCacheEntry(GherkinStepKind stepKind, string stepText)
        {
            StepKind = stepKind;
            StepText = stepText;
        }
        public GherkinStepKind StepKind { get; }
        public string StepText { get; }
    }
}