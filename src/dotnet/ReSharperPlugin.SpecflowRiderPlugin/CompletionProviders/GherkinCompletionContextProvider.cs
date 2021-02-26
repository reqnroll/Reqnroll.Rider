using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Impl;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
    [IntellisensePart]
    public class GherkinCompletionContextProvider : CodeCompletionContextProviderBase
    {
        public override bool IsApplicable(CodeCompletionContext context)
        {
            return context.File is GherkinFile;
        }

        public override ISpecificCodeCompletionContext GetCompletionContext(CodeCompletionContext context)
        {
            var nodeUnderCursor = TextControlToPsi.GetElement<ITreeNode>(context.Solution, context.TextControl);
            var ranges = GetTextLookupRanges(context, nodeUnderCursor.GetDocumentRange());
            return new GherkinSpecificCodeCompletionContext(context, ranges, nodeUnderCursor);
        }
    }
}