using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinTag : CompositeElement
    {
        public override NodeType NodeType => GherkinNodeTypes.TAG;
        public override PsiLanguageType Language => GherkinLanguage.Instance.NotNull();
    }
}