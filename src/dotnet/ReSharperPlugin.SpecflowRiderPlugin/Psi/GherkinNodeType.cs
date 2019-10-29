using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinNodeType : CompositeNodeType
    {
        public GherkinNodeType(string name, int index) : base(name, index)
        {
        }

        public override CompositeElement Create()
        {
            return new GherkinElement(this);
        }
    }
}