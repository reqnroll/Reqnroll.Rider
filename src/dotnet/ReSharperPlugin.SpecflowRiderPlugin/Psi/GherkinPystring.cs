namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinPystring : GherkinElement
    {
        public GherkinPystring() : base(GherkinNodeTypes.PYSTRING)
        {
        }

        protected override string GetPresentableText()
        {
            return string.Empty;
        }
    }
}