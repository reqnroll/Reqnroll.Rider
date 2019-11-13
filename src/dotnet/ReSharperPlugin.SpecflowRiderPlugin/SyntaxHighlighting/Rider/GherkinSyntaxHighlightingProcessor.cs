using JetBrains.Diagnostics;
using JetBrains.Rd.Impl;
using JetBrains.ReSharper.Daemon.SyntaxHighlighting;
using JetBrains.ReSharper.Psi.Parsing;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.SyntaxHighlighting.Rider
{
    public class GherkinSyntaxHighlightingProcessor : SyntaxHighlightingProcessor
    {
        public override string GetAttributeId(TokenNodeType tokenType)
        {
            if (tokenType == GherkinTokenTypes.TAG)
            {
                Protocol.TraceLogger.Log(LoggingLevel.INFO, $"GherkinSyntaxHighlighting:GetAttributeId – {tokenType}");
                return GherkinHighlightingAttributeIds.TAG;
            }
            
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

        protected override string KeywordAttributeId => GherkinHighlightingAttributeIds.KEYWORD;

        protected override string LineCommentAttributeId => GherkinHighlightingAttributeIds.LINE_COMMENT;
    }
}