using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinComment(string commentText) : GherkinElement(GherkinNodeTypes.COMMENT), ICommentNode
{
    public string CommentText { get; } = commentText;

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