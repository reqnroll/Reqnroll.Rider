using System.Linq;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinElement(GherkinNodeType nodeType) : CompositeElement
{
    public override NodeType NodeType { get; } = nodeType;

    public override PsiLanguageType Language => GherkinLanguage.Instance.NotNull();

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