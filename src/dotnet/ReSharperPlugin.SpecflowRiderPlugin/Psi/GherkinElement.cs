using System.Linq;
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

        protected virtual string GetPresentableText()
        {
            var textTokens = this.FindChildren<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT);
            return string.Join(" ", textTokens.Select(o => o.GetText()));
        }

        public override string ToString()
        {
            var presentableText = GetPresentableText();
            if (string.IsNullOrWhiteSpace(presentableText))
                return GetType().Name;
                
            return $"{GetType().Name}: {presentableText}";
        }
    }
}