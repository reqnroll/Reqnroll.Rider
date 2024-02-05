using JetBrains.ReSharper.Daemon.Syntax;
using JetBrains.ReSharper.Daemon.SyntaxHighlighting;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.SyntaxHighlighting.Rider
{
    public class GherkinSyntaxHighlightingProcessor : SyntaxHighlightingProcessor
    {
        public override bool InteriorShouldBeProcessed(ITreeNode element, IHighlightingConsumer context)
        {
            if (element.NodeType == GherkinNodeTypes.TABLE_CELL &&
                element.Parent?.NodeType == GherkinNodeTypes.TABLE_HEADER_ROW)
            {
                var documentRange = element.GetDocumentRange();
                if (documentRange.TextRange.IsEmpty)
                    return false;

                context.AddHighlighting(new ReSharperSyntaxHighlighting(GherkinHighlightingAttributeIds.TABLE_HEADER_CELL, null, documentRange));
                return false;
            }

            return base.InteriorShouldBeProcessed(element, context);
        }

        public override string GetAttributeId(TokenNodeType tokenType)
        {
            if (tokenType == GherkinTokenTypes.TAG)
                return GherkinHighlightingAttributeIds.TAG;
            
            if (tokenType == GherkinTokenTypes.STEP_PARAMETER_TEXT)
                return GherkinHighlightingAttributeIds.OUTLINE_PARAMETER_SUBSTITUTION;

            if (tokenType == GherkinTokenTypes.TEXT)
                return GherkinHighlightingAttributeIds.TEXT;

            if (tokenType == GherkinTokenTypes.TABLE_CELL)
                return GherkinHighlightingAttributeIds.TABLE_CELL;

            if (tokenType == GherkinTokenTypes.STEP_PARAMETER_TEXT) //TODO Detect regex parameter like: "param"
                return GherkinHighlightingAttributeIds.REGEXP_PARAMETER;

            if (tokenType == GherkinTokenTypes.PIPE)
                return GherkinHighlightingAttributeIds.PIPE;

            return base.GetAttributeId(tokenType);
        }

        protected override bool IsLineComment(TokenNodeType tokenType)
        {
            return tokenType == GherkinTokenTypes.COMMENT;
        }

        protected override bool IsKeyword(TokenNodeType tokenType)
        {
            return GherkinTokenTypes.KEYWORDS[tokenType];
        }

        protected override bool IsString(TokenNodeType tokenType)
        {
            return tokenType == GherkinTokenTypes.PYSTRING;
        }

        protected override string KeywordAttributeId => GherkinHighlightingAttributeIds.KEYWORD;

        protected override string LineCommentAttributeId => GherkinHighlightingAttributeIds.LINE_COMMENT;
        
        protected override string StringAttributeId => GherkinHighlightingAttributeIds.PYSTRING;
    }
}