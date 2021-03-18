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
            var relatedText = string.Empty;
            var nodeUnderCursor = TextControlToPsi.GetElement<ITreeNode>(context.Solution, context.TextControl);

            var interestingNode = GetInterestingNode(nodeUnderCursor);
            if (interestingNode == null)
                return null;

            var ranges = GetTextLookupRanges(context, nodeUnderCursor.GetDocumentRange());

            if (interestingNode is GherkinStep step)
            {
                var stepTextRange = step.GetStepTextRange();
                if (IsCursorBeforeNode(context, stepTextRange))
                    return null;

                relatedText = step.GetStepTextBeforeCaret(context.CaretDocumentOffset);
                if (IsCursorAfterNode(context, stepTextRange))
                {
                    stepTextRange = stepTextRange.ExtendRight(context.CaretDocumentOffset.Offset - stepTextRange.EndOffset.Offset);
                    relatedText += " ";
                }

                var replaceRange = stepTextRange;
                var insertRange = stepTextRange.SetEndTo(context.SelectedRange.EndOffset);

                ranges = new TextLookupRanges(insertRange, replaceRange);
            }
            return new GherkinSpecificCodeCompletionContext(context, ranges, interestingNode, relatedText);
        }

        // This occurs when cursor is at the end of line with a space before it. In this case the node ends up a bit sooner
        // example: Given some <caret>
        //          ^========^^
        //          |- Step   |- Whitespace token (outside the step)
        private static bool IsCursorAfterNode(CodeCompletionContext context, DocumentRange nodeRange)
        {
            return nodeRange.EndOffset < context.CaretDocumentOffset;
        }

        private static bool IsCursorBeforeNode(CodeCompletionContext context, DocumentRange nodeRange)
        {
            return context.CaretDocumentOffset < nodeRange.StartOffset;
        }

        private ITreeNode GetInterestingNode(ITreeNode node)
        {
            if (node.GetTokenType() == GherkinTokenTypes.WHITE_SPACE && node.PrevSibling != null)
                node = node.PrevSibling;

            while (node != null)
            {
                if (node is GherkinStep)
                    return node;
                if (node is IGherkinScenario)
                    return node;
                if (node is GherkinFeature)
                    return node;
                node = node.Parent;
            }

            return null;
        }
    }
}