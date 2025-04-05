using System.Text;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinTag() : GherkinElement(GherkinNodeTypes.TAG)
{

    protected override string GetPresentableText()
    {
        var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TAG);
        return textToken?.GetText();
    }

    public string GetTagText()
    {
        var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TAG);
        return textToken?.GetText().Substring(1);
    }

    public string GetStepTextBeforeCaret(DocumentOffset caretLocation)
    {
        var sb = new StringBuilder();
        for (var te = FirstChild; te != null; te = te.NextSibling)
        {
            if (te.GetDocumentStartOffset() > caretLocation)
                break;

            var truncateTextSize = 0;
            if (te.GetDocumentEndOffset() > caretLocation)
                truncateTextSize = te.GetDocumentEndOffset().Offset - caretLocation.Offset;

            sb.Append(te.GetText());

            if (truncateTextSize >= sb.Length)
                return string.Empty;

            sb.Length -= truncateTextSize;
        }
        return sb.ToString().Trim();
    }
}