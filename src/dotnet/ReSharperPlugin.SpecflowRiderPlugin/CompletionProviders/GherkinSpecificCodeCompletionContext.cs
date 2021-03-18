using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
    public class GherkinSpecificCodeCompletionContext : SpecificCodeCompletionContext
    {
        public GherkinSpecificCodeCompletionContext(
            [NotNull] CodeCompletionContext context,
            TextLookupRanges ranges,
            ITreeNode nodeUnderCursor,
            string relatedText)
            : base(context)
        {
            Ranges = ranges;
            NodeUnderCursor = nodeUnderCursor;
            RelatedText = relatedText;
        }

        public override string ContextId => "GherkinCodeCompletionContext";
        public TextLookupRanges Ranges { get; }
        public ITreeNode NodeUnderCursor { get; }
        public string RelatedText { get; }
        public GherkinFile GherkinFile => (GherkinFile) BasicContext.File;
    }
}