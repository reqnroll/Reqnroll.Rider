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
        }

        public override ILexerFactory GetMixedLexerFactory(ISolution solution, IBuffer buffer, IPsiSourceFile sourceFile = null)
        {
            return GherkinLanguage.Instance.LanguageService().GetPrimaryLexerFactory();
        }
        
        protected override PsiLanguageType PsiLanguageType => GherkinLanguage.Instance.NotNull();
        
        public override IconId Icon { get; }
    }
}