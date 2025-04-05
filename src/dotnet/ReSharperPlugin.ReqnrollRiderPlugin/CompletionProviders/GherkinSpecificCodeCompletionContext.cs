using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders;

public class GherkinSpecificCodeCompletionContext(
    [NotNull] CodeCompletionContext context,
    TextLookupRanges ranges,
    ITreeNode nodeUnderCursor,
    string relatedText,
    bool isStartOfLine)
    : SpecificCodeCompletionContext(context)
{

    public override string ContextId => "GherkinCodeCompletionContext";
    public TextLookupRanges Ranges { get; } = ranges;
    public ITreeNode NodeUnderCursor { get; } = nodeUnderCursor;
    public string RelatedText { get; } = relatedText;
    public GherkinFile GherkinFile => (GherkinFile) BasicContext.File;
    public bool IsStartOfLine { get; } = isStartOfLine;
}