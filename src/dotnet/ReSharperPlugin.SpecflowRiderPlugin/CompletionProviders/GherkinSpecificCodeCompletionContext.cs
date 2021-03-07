using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
    public class GherkinSpecificCodeCompletionContext : SpecificCodeCompletionContext
    {
        public GherkinSpecificCodeCompletionContext(
            [NotNull] CodeCompletionContext context,
            TextLookupRanges ranges,
            ITreeNode nodeUnderCursor)
            : base(context)
        {
            Ranges = ranges;
            NodeUnderCursor = nodeUnderCursor;
        }

        public override string ContextId => "GherkinCodeCompletionContext";
        public TextLookupRanges Ranges { get; }
        public ITreeNode NodeUnderCursor { get; }
    }
}