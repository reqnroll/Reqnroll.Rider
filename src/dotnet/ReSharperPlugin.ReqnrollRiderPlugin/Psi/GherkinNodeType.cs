using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi
{
    public class GherkinNodeType : CompositeNodeWithArgumentType
    {
        protected GherkinNodeType(string name, int index) : base(name, index)
        {
        }

        public override CompositeElement Create(object userData)
        {
            return new GherkinElement(this);
        }

        public sealed override CompositeElement Create()
        {
            return Create(null);
        }
    }

    public class GherkinNodeType<T> : GherkinNodeType
        where T : CompositeElement, new()
    {
        protected GherkinNodeType(string name, int index) : base(name, index)
        {
        }

        public override CompositeElement Create(object userData)
        {
            return new T();
        }
    }
}