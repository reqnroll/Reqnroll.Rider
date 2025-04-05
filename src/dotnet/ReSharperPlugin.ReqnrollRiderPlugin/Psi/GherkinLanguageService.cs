using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.ReqnrollJsonSettings;
using ReSharperPlugin.ReqnrollRiderPlugin.Formatting;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

[Language(typeof (GherkinLanguage))]
public class GherkinLanguageService(
    [NotNull] GherkinLanguage language,
    [NotNull] IConstantValueService constantValueService,
    [NotNull] GherkinKeywordProvider keywordProvider,
    [NotNull] ReqnrollSettingsProvider settingsProvider,
    [NotNull] GherkinCodeFormatter gherkinCodeFormatter)
    : LanguageService(language, constantValueService)
{

    public override ILexerFactory GetPrimaryLexerFactory()
    {
        return new GherkinLexerFactory(keywordProvider, settingsProvider);
    }

    public override ILexer CreateFilteringLexer(ILexer lexer)
    {
        throw new System.NotImplementedException();
    }

    public override IParser CreateParser(ILexer lexer, IPsiModule module, IPsiSourceFile sourceFile)
    {
        return new GherkinParser(lexer, sourceFile, settingsProvider, keywordProvider);
    }

    public override IEnumerable<ITypeDeclaration> FindTypeDeclarations(IFile file)
    {
        return Enumerable.Empty<ITypeDeclaration>();
    }

    public override ILanguageCacheProvider CacheProvider => null;
    public override bool IsCaseSensitive => true;
    public override bool SupportTypeMemberCache => false;
    public override ITypePresenter TypePresenter => DefaultTypePresenter.Instance;
    public override ICodeFormatter CodeFormatter => gherkinCodeFormatter;
}