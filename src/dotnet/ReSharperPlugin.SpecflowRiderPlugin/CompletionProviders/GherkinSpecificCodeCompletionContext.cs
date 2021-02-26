using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

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