using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    
    public class GherkinElement : CompositeElement
    {
        public override NodeType NodeType { get; }
        
        public override PsiLanguageType Language => GherkinLanguage.Instance.NotNull();

        public GherkinElement(GherkinNodeType nodeType)
        {
            NodeType = nodeType;
        }
    }
}