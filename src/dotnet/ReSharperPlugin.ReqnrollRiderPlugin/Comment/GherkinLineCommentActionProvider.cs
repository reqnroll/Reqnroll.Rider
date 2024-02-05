using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Comment;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Comment
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinLineCommentActionProvider : SimpleLineCommentActionProvider
    {
        public override string StartLineCommentMarker => "#";
        protected override bool IsWhitespace(TokenNodeType tokenType) => tokenType == GherkinTokenTypes.WHITE_SPACE;
        protected override bool IsNewLine(TokenNodeType tokenType) => tokenType == GherkinTokenTypes.NEW_LINE;

        protected override bool IsEndOfLineComment(TokenNodeType tokenType, string tokenText)
        {
            return tokenType == GherkinTokenTypes.COMMENT;
        }

        public override bool ShouldInsertAtLineStart(IContextBoundSettingsStore settingsStore) => true;
    }
}