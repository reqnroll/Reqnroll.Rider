using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinLexerFactory : ILexerFactory
    {
        private readonly GherkinKeywordProvider _provider;

        public GherkinLexerFactory(GherkinKeywordProvider provider)
        {
            _provider = provider;
        }
        
        public ILexer CreateLexer(IBuffer buffer)
        {
            return new GherkinLexer(buffer, _provider);
        }
    }
}