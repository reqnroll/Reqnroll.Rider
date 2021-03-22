using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon.CodeFolding;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Folding
{
    public class SpecFlowFoldingProcessor : ICodeFoldingProcessor
    {

        public bool InteriorShouldBeProcessed(ITreeNode element, FoldingHighlightingConsumer context) => true;

        public bool IsProcessingFinished(FoldingHighlightingConsumer context) => false;

        public void ProcessBeforeInterior(ITreeNode node, FoldingHighlightingConsumer consumer)
        {
            switch (node)
            {
                case GherkinScenario scenario:
                    FoldNode(node, consumer, scenario.IsBackground() ? GherkinTokenTypes.BACKGROUND_KEYWORD : GherkinTokenTypes.SCENARIO_KEYWORD);
                    break;
                case GherkinScenarioOutline scenarioOutline:
                    FoldNode(node, consumer, scenarioOutline.IsBackground() ? GherkinTokenTypes.BACKGROUND_KEYWORD :GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD);
                    break;
                case GherkinExamplesBlock _:
                    FoldNode(node, consumer, GherkinTokenTypes.EXAMPLES_KEYWORD);
                    break;
                case GherkinRule _:
                    FoldNode(node, consumer, GherkinTokenTypes.RULE_KEYWORD);
                    break;
            }

        }

        private static void FoldNode(ITreeNode node, FoldingHighlightingConsumer consumer, GherkinTokenType keyWordTokenType)
        {
            var gherkinElement = node as GherkinElement;
            var keywordRange = gherkinElement.FindChild<GherkinToken>(o => o.NodeType == keyWordTokenType).GetDocumentRange();
            var textRange = gherkinElement.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT)?.GetDocumentRange();

            var range = node.GetDocumentRange();

            var length = CalculateLength(textRange, keywordRange, range);
            if (length > 0)
            {
                var newRange = range.TrimLeft(length);
                consumer.AddDefaultPriorityFolding(CodeFoldingAttributes.DEFAULT_FOLDING_ATTRIBUTE, newRange, "...");
            }
        }

        private static int CalculateLength(DocumentRange? textRange, DocumentRange keywordRange, DocumentRange range)
        {
            if (keywordRange == range)
                return 0;

            var startOffset = range.StartOffset;
            var endOffset = (textRange?.EndOffset ?? keywordRange.EndOffset+1);
            
            if (startOffset > endOffset)
                return 0;
            
            return endOffset - startOffset;
        }

        public void ProcessAfterInterior(ITreeNode element, FoldingHighlightingConsumer consumer)
        {
            
        }
        
    }
}