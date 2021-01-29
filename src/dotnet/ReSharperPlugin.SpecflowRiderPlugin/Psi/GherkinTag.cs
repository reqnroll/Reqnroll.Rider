namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinTag : GherkinElement
    {
        public GherkinTag() : base(GherkinNodeTypes.TAG)
        {
        }

        protected override string GetPresentableText()
        {
            var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TAG);
            return textToken?.GetText();
        }
    }
}