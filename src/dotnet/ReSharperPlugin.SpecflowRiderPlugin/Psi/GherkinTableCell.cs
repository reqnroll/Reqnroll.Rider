namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinTableCell : GherkinElement
    {
        public GherkinTableCell() : base(GherkinNodeTypes.TABLE_CELL)
        {
        }

        protected override string GetPresentableText()
        {
            var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TABLE_CELL);
            return textToken?.GetText();
        }
    }
}