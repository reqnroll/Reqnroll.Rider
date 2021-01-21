namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinFeatureHeader : GherkinElement
    {
        public GherkinFeatureHeader() : base(GherkinNodeTypes.FEATURE_HEADER)
        {
        }

        protected override string GetPresentableText()
        {
            return string.Empty;
        }
    }
}