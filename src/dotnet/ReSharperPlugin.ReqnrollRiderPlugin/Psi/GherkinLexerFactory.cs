using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.ReqnrollJsonSettings;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinLexerFactory(GherkinKeywordProvider keywordProvider, ReqnrollSettingsProvider settingsProvider) : ILexerFactory
{

    public ILexer CreateLexer(IBuffer buffer)
    {
        return new GherkinLexer(buffer, keywordProvider, settingsProvider);
    }
}