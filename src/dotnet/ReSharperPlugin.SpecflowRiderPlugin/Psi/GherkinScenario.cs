namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinScenario : GherkinElement
    {
        public GherkinScenario() : base(GherkinNodeTypes.SCENARIO)
        {
        }

        public bool IsBackground()
        {
            return FirstChild?.NodeType == GherkinTokenTypes.BACKGROUND_KEYWORD;
        }

        public override string ToString()
        {
            if (IsBackground())
                return "GherkinScenario(Background):";

            var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT);
            return $"GherkinScenario: {textToken?.GetText()}";
        }
    }
}