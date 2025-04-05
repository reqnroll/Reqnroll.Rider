using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsUsages;

public class ReqnrollStepUsageCacheEntry(GherkinStepKind stepKind, string stepText)
{
    public GherkinStepKind StepKind { get; } = stepKind;
    public string StepText { get; } = stepText;
}