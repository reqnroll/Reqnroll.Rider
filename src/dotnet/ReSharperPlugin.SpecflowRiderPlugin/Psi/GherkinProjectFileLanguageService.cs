using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.Rd.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.UI.Icons;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    [ProjectFileType(typeof (GherkinProjectFileType))]
    public class GherkinProjectFileLanguageService : ProjectFileLanguageService
    {
        public GherkinProjectFileLanguageService(GherkinProjectFileType projectFileType) : base(projectFileType)
        {
            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"GherkinProjectFileLanguageService");
        }

        public override ILexerFactory GetMixedLexerFactory(ISolution solution, IBuffer buffer, IPsiSourceFile sourceFile = null)
        {
            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"Requested lexer factory for {sourceFile?.Name}");
            return GherkinLanguage.Instance.LanguageService().GetPrimaryLexerFactory();
        }

//        public override PsiLanguageType GetPsiLanguageType(IProjectFile projectFile)
//        {
//            var result = base.GetPsiLanguageType(projectFile);
//            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"GetPsiLanguageType(IProjectFile): {projectFile.Name}: {result}");
//            return result;
//        }

//        public override PsiLanguageType GetPsiLanguageType(ProjectFileType languageType)
//        {
//            var result = base.GetPsiLanguageType(languageType);
//            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"GetPsiLanguageType(ProjectFileType): {languageType}: {result}");
//            return result;
//        }

//        public override PsiLanguageType GetPsiLanguageType(IPsiSourceFile sourceFile)
//        {
//            var result = base.GetPsiLanguageType(sourceFile);
//            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"GetPsiLanguageType(IPsiSourceFile): {sourceFile.Name}: {result}");
//            return result;
//        }

        protected override PsiLanguageType PsiLanguageType => GherkinLanguage.Instance;
        
        public override IconId Icon { get; }
    }
}