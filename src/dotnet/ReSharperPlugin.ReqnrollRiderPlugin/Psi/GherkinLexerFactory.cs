using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.ReqnrollJsonSettings;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi
{
    public class GherkinLexerFactory : ILexerFactory
    {
        private readonly GherkinKeywordProvider _keywordProvider;
        private readonly ReqnrollSettingsProvider _settingsProvider;

        public GherkinLexerFactory(GherkinKeywordProvider keywordProvider, ReqnrollSettingsProvider settingsProvider)
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