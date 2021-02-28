using JetBrains.DocumentModel;
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
            var step = nodeUnderCursor?.GetContainingNode<GherkinStep>() ?? nodeUnderCursor?.PrevSibling as GherkinStep;
            if (step != null)
            {
                nodeUnderCursor = step;
                var stepRange = step.GetDocumentRange();
                if (stepRange.EndOffset < context.CaretDocumentOffset)
                    stepRange = stepRange.ExtendRight(context.CaretDocumentOffset.Offset - stepRange.EndOffset.Offset);
                var replaceRange = stepRange.TrimLeft(step.GetKeywordText().Length + 1);
                var insertRange = new DocumentRange(replaceRange.StartOffset, context.SelectedRange.EndOffset);
                ranges = new TextLookupRanges(insertRange, replaceRange);
            }
            return new GherkinSpecificCodeCompletionContext(context, ranges, nodeUnderCursor);
        }
    }
}