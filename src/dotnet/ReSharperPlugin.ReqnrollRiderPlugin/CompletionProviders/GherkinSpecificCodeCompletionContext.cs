using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders
{
    public class GherkinSpecificCodeCompletionContext : SpecificCodeCompletionContext
    {
        public GherkinSpecificCodeCompletionContext(
            [NotNull] CodeCompletionContext context,
            TextLookupRanges ranges,
            ITreeNode nodeUnderCursor,
            string relatedText,
            bool isStartOfLine
        )
            : base(context)
        {
            Ranges = ranges;
            NodeUnderCursor = nodeUnderCursor;
            RelatedText = relatedText;
            IsStartOfLine = isStartOfLine;
        }

        public override string ContextId => "GherkinCodeCompletionContext";
        public TextLookupRanges Ranges { get; }
        public ITreeNode NodeUnderCursor { get; }
        public string RelatedText { get; }
        public GherkinFile GherkinFile => (GherkinFile) BasicContext.File;
        public bool IsStartOfLine { get; }
    }
}