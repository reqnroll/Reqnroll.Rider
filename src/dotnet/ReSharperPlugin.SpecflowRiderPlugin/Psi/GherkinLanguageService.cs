using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.Rd.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    [Language(typeof (GherkinLanguage))]
    public class GherkinLanguageService : LanguageService
    {
        [NotNull] private readonly GherkinKeywordProvider _keywordProvider;

        public GherkinLanguageService([NotNull] GherkinLanguage language,
                                      [NotNull] IConstantValueService constantValueService,
                                      [NotNull] GherkinKeywordProvider keywordProvider) : base(language, constantValueService)
        {
            _keywordProvider = keywordProvider;
            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"GherkinLanguageService");
        }

        public override ILexerFactory GetPrimaryLexerFactory()
        {
            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"GetPrimaryLexerFactory");
            return new GherkinLexerFactory(_keywordProvider);
        }

        public override ILexer CreateFilteringLexer(ILexer lexer)
        {
            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"CreateFilteringLexer");
            throw new System.NotImplementedException();
        }

        public override IParser CreateParser(ILexer lexer, IPsiModule module, IPsiSourceFile sourceFile)
        {
            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"CreateParser: {sourceFile.Name}");
            return new GherkinParser(lexer, sourceFile);
        }

        public override IEnumerable<ITypeDeclaration> FindTypeDeclarations(IFile file)
        {
            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"FindTypeDeclarations");
            throw new System.NotImplementedException();
        }

        public override ILanguageCacheProvider CacheProvider { get; }
        public override bool IsCaseSensitive => true;
        public override bool SupportTypeMemberCache => false;
        public override ITypePresenter TypePresenter => DefaultTypePresenter.Instance;
    }
}