using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Text;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi
{
    public class GherkinToken : BoundToBufferLeafElement, ITokenNode
    {
        private readonly TokenNodeType _nodeType;

        public GherkinToken(TokenNodeType nodeType,
                            [NotNull] IBuffer buffer,
                            TreeOffset startOffset,
                            TreeOffset endOffset) : base(nodeType, buffer, startOffset, endOffset)
        {
            _nodeType = nodeType;
        }

        public override PsiLanguageType Language => GherkinLanguage.Instance.NotNull();
        
        public TokenNodeType GetTokenType()
        {
            return _nodeType;
        }

        public override string ToString()
        {
            return $"GherkinToken({GetTreeStartOffset()},{Length}): {NodeType}('{GetText()}')";
        }
    }
}