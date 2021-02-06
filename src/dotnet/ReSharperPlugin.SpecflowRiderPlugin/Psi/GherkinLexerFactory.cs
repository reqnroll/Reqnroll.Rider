using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinLexerFactory : ILexerFactory
    {
        private readonly GherkinKeywordProvider _keywordProvider;
        private readonly SpecflowSettingsProvider _settingsProvider;

        public GherkinLexerFactory(GherkinKeywordProvider keywordProvider, SpecflowSettingsProvider settingsProvider)
        {
            _keywordProvider = keywordProvider;
            _settingsProvider = settingsProvider;
        }
        
        public ILexer CreateLexer(IBuffer buffer)
        {
            return new GherkinLexer(buffer, _keywordProvider, _settingsProvider);
        }
    }
}