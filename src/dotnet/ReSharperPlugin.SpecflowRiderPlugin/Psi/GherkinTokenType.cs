using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinTokenType : TokenNodeType
    {
        public GherkinTokenType(string s, int index) : base(s, index)
        {
        }

        public override LeafElementBase Create(IBuffer buffer, TreeOffset startOffset, TreeOffset endOffset)
        {
            return new GherkinToken(this, buffer, startOffset, endOffset);
        }

        public override bool IsWhitespace { get; }
        public override bool IsComment { get; }
        public override bool IsStringLiteral { get; }
        public override bool IsConstantLiteral { get; }
        public override bool IsIdentifier { get; }
        public override bool IsKeyword { get; }
        public override string TokenRepresentation { get; }
    }
}