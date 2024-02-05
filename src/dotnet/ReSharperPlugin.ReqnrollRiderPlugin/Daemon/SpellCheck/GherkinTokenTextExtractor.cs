using JetBrains.ProjectModel;
using JetBrains.ReSharper.Features.ReSpeller.Analyzers;
using JetBrains.ReSharper.Features.ReSpeller.Analyzers.HighlightingGenerators;
using JetBrains.ReSharper.Features.ReSpeller.Analyzers.Scopes;
using JetBrains.ReSharper.Features.ReSpeller.Daemon;
using JetBrains.ReSharper.Features.ReSpeller.Daemon.TextExtraction;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.SpellCheck;

[SolutionComponent]
public class GherkinTokenTextExtractor : ElementTextExtractor<GherkinToken>
{
    public GherkinTokenTextExtractor(
        ReSpellerDataBuilder reReSpellerDataBuilder,
        IRequiredSpellCheckingModesProvider requiredRequiredSpellCheckingModesProvider,
        GrammarAndSpellingMeasurements measurements
    ) : base(reReSpellerDataBuilder, requiredRequiredSpellCheckingModesProvider, measurements)
    {
    }

    public override bool Extract(GherkinToken node, ElementTextExtractorContext context)
    {
        context.Collector.EnrichWithReSpellerData(
            ReSpellerDataBuilder, context.File, SpellCheckingMode.Orthography, CheckingContext.Comment,
            SparseTextToCheck.FromNode(node)
        );
        return true;
    }
}