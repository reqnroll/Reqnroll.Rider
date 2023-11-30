using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinComment : GherkinElement, ICommentNode
    {
        public string CommentText { get; }

        public GherkinComment(string commentText) : base(GherkinNodeTypes.COMMENT)
        {
            CommentText = commentText;
        }

        public TokenNodeType GetTokenType()
        {
            return GherkinTokenTypes.COMMENT;
        }

        public TreeTextRange GetCommentRange()
        {
            var treeStartOffset = GetTreeStartOffset();
            return new TreeTextRange(treeStartOffset + 1, treeStartOffset + GetTextLength());
        }
    }
}