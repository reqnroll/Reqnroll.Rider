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

            if (IsAfterKeywordExpectingText(nodeUnderCursor))
                return null;

            ITreeNode interestingNode;
            if (IsAtEmptyLineBeforeEndOfFile(nodeUnderCursor))
            {
                var node = nodeUnderCursor;
                while (node != null && node.IsWhitespaceToken())
                    node = node.PrevSibling;
                interestingNode = node;
                while (interestingNode?.LastChild != null &&
                       (interestingNode.LastChild is GherkinFeature || interestingNode.LastChild is GherkinScenario || interestingNode.LastChild is GherkinScenarioOutline)
                )
                    interestingNode = interestingNode?.LastChild;
            }
            else
            {
                if (nodeUnderCursor?.GetTokenType() == GherkinTokenTypes.NEW_LINE && nodeUnderCursor?.NextSibling != null)
                    nodeUnderCursor = nodeUnderCursor.NextSibling;
                interestingNode = GetInterestingNode(nodeUnderCursor);
            }

            if (interestingNode == null)
                return null;

            var isStartOfLine = IsStartOfLine(nodeUnderCursor, out var startOfLineText);

            var ranges = GetTextLookupRanges(context, nodeUnderCursor.GetDocumentRange());
            if (nodeUnderCursor is GherkinToken token && token.IsWhitespaceToken() && token.PrevSibling?.GetTokenType() == GherkinTokenTypes.NEW_LINE)
                ranges = GetTextLookupRanges(context, nodeUnderCursor.GetDocumentRange().SetStartTo(context.CaretDocumentOffset));

            if (interestingNode is GherkinTag tag)
            {
                relatedText = startOfLineText = tag.GetStepTextBeforeCaret(context.CaretDocumentOffset);
                if (context.CaretDocumentOffset > tag.GetDocumentEndOffset())
                {
                    relatedText = "@";
                    ranges = new TextLookupRanges(
                        new DocumentRange(tag.GetDocumentEndOffset().Shift(1), context.CaretDocumentOffset),
                        new DocumentRange(tag.GetDocumentEndOffset().Shift(1), context.CaretDocumentOffset)
                    );
                }
                else
                {
                    ranges = new TextLookupRanges(
                        new DocumentRange(tag.GetDocumentStartOffset(), context.CaretDocumentOffset),
                        tag.GetDocumentRange()
                    );
                }

            }

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
                    if (string.IsNullOrWhiteSpace(relatedText))
                    {
                        relatedText = string.Empty;
                        stepTextRange = stepTextRange.ExtendLeft(-1);
                    }
                }

                var replaceRange = stepTextRange;
                var insertRange = stepTextRange.SetEndTo(context.SelectedRange.EndOffset);

                ranges = new TextLookupRanges(insertRange, replaceRange);
            }
            return new GherkinSpecificCodeCompletionContext(context, ranges, interestingNode, isStartOfLine ? startOfLineText : relatedText, isStartOfLine);
        }

        private bool IsAtEmptyLineBeforeEndOfFile(ITreeNode node)
        {
            var nodePrev = node;
            while (nodePrev?.IsWhitespaceToken() == true && nodePrev.GetTokenType() != GherkinTokenTypes.NEW_LINE)
                nodePrev = nodePrev.GetPreviousToken();
            if (nodePrev?.GetTokenType() != GherkinTokenTypes.NEW_LINE)
                return false;

            while (node != null)
            {
                if (!node.IsWhitespaceToken())
                    return false;
                node = node.GetNextToken();
            }
            return true;
        }

        private bool IsAfterKeywordExpectingText(ITreeNode nodeUnderCursor)
        {
            if (nodeUnderCursor.IsWhitespaceToken()
                || nodeUnderCursor is GherkinToken token && (token.NodeType == GherkinTokenTypes.TEXT || token.NodeType == GherkinTokenTypes.COLON))
            {
                for (var t = nodeUnderCursor.PrevSibling; t != null; t = t.PrevSibling)
                {
                    if (t.GetTokenType() == GherkinTokenTypes.NEW_LINE)
                        break;
                    if (t.GetTokenType() == GherkinTokenTypes.FEATURE_KEYWORD)
                        return true;
                    if (t.GetTokenType() == GherkinTokenTypes.SCENARIO_KEYWORD)
                        return true;
                    if (t.GetTokenType() == GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD)
                        return true;
                    if (t.GetTokenType() == GherkinTokenTypes.BACKGROUND_KEYWORD)
                        return true;
                    if (t.GetTokenType() == GherkinTokenTypes.PIPE || t.GetTokenType() == GherkinTokenTypes.TABLE_CELL)
                        return true;
                }
                return false;
            }
            return false;
        }

        private bool IsStartOfLine(ITreeNode nodeUnderCursor, out string text)
        {
            text = string.Empty;

            if (nodeUnderCursor.IsWhitespaceToken() && nodeUnderCursor.GetPreviousToken()?.NodeType == GherkinTokenTypes.NEW_LINE)
                return true;

            if ((nodeUnderCursor.GetPreviousToken()?.IsWhitespaceToken() == true && nodeUnderCursor.GetPreviousToken()?.GetPreviousToken()?.NodeType == GherkinTokenTypes.NEW_LINE)
                ||nodeUnderCursor.GetPreviousToken()?.NodeType == GherkinTokenTypes.NEW_LINE )
            {
                text = nodeUnderCursor.GetText();
                return true;
            }

            return false;
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
                node = GetDeepestLastChild(node.PrevSibling);

            if (node.GetTokenType() == GherkinTokenTypes.COMMENT)
                return null;

            while (node != null)
            {
                if (node is GherkinStep)
                    return node;
                if (node is IGherkinScenario)
                    return node;
                if (node is GherkinTag)
                    return node;
                if (node is GherkinFeature)
                    return node;
                node = node.Parent;
            }

            return null;
        }

        private static ITreeNode GetDeepestLastChild(ITreeNode node)
        {
            while (node.LastChild != null)
                node = node.LastChild;
            return node;
        }
    }
}