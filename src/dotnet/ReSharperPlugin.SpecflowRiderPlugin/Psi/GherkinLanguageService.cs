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
using ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings;
using ReSharperPlugin.SpecflowRiderPlugin.Formatting;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    [Language(typeof (GherkinLanguage))]
    public class GherkinLanguageService : LanguageService
    {
        [NotNull] private readonly GherkinKeywordProvider _keywordProvider;
        [NotNull] private readonly SpecflowSettingsProvider _settingsProvider;
        [NotNull] private readonly GherkinCodeFormatter _gherkinCodeFormatter;

        public GherkinLanguageService([NotNull] GherkinLanguage language,
                                      [NotNull] IConstantValueService constantValueService,
                                      [NotNull] GherkinKeywordProvider keywordProvider,
                                      [NotNull] SpecflowSettingsProvider settingsProvider,
                                      [NotNull] GherkinCodeFormatter gherkinCodeFormatter) : base(language, constantValueService)
        {
            _keywordProvider = keywordProvider;
            _settingsProvider = settingsProvider;
            _gherkinCodeFormatter = gherkinCodeFormatter;
        }

        public override ILexerFactory GetPrimaryLexerFactory()
        {
            return new GherkinLexerFactory(_keywordProvider, _settingsProvider);
        }

        public override ILexer CreateFilteringLexer(ILexer lexer)
        {
            throw new System.NotImplementedException();
        }

        public override IParser CreateParser(ILexer lexer, IPsiModule module, IPsiSourceFile sourceFile)
        {
            return new GherkinParser(lexer, sourceFile, _settingsProvider, _keywordProvider);
        }

        public override IEnumerable<ITypeDeclaration> FindTypeDeclarations(IFile file)
        {
            return Enumerable.Empty<ITypeDeclaration>();
        }

        public override ILanguageCacheProvider CacheProvider => null;
        public override bool IsCaseSensitive => true;
        public override bool SupportTypeMemberCache => false;
        public override ITypePresenter TypePresenter => DefaultTypePresenter.Instance;
        public override ICodeFormatter CodeFormatter => _gherkinCodeFormatter;
    }
}