using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsUsages
{
    public class SpecflowStepUsageCacheEntry
    {
        public SpecflowStepUsageCacheEntry(GherkinStepKind stepKind, string stepText)
        {
            StepKind = stepKind;
            StepText = stepText;
        }
        public GherkinStepKind StepKind { get; }
        public string StepText { get; }
    }
}