namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinTableCell() : GherkinElement(GherkinNodeTypes.TABLE_CELL)
{

    protected override string GetPresentableText()
    {
        var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TABLE_CELL);
        return textToken?.GetText();
    }
}