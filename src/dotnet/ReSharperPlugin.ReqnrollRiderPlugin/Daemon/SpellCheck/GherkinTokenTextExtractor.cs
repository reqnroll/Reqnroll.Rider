using JetBrains.Application.Parts;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Features.ReSpeller.Analyzers;
using JetBrains.ReSharper.Features.ReSpeller.Analyzers.HighlightingGenerators;
using JetBrains.ReSharper.Features.ReSpeller.Analyzers.Scopes;
using JetBrains.ReSharper.Features.ReSpeller.Daemon;
using JetBrains.ReSharper.Features.ReSpeller.Daemon.TextExtraction;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.SpellCheck;

[SolutionComponent(Instantiation.DemandAnyThreadUnsafe)]
public class GherkinTokenTextExtractor(
    ISpellingAndGrammarDataBuilder spellingAndGrammarDataBuilder,
    IRequiredSpellCheckingModesProvider requiredRequiredSpellCheckingModesProvider,
    GrammarAndSpellingMeasurements measurements
) : ElementTextExtractor<GherkinToken>(
    spellingAndGrammarDataBuilder,
    requiredRequiredSpellCheckingModesProvider,
    measurements
)
{

    public override bool Extract(GherkinToken node, ElementTextExtractorContext context)
    {
        context.Collector.EnrichWithReSpellerData(
            SpellingAndGrammarDataBuilder,
            context.File,
            node,
            SpellCheckingMode.Orthography,
            CheckingContext.Comment,
            SparseTextToCheck.FromNode(node)
        );
        return true;
    }
}