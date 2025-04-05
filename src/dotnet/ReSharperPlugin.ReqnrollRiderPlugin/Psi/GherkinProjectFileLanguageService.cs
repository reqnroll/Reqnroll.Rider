using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.UI.Icons;
using ReSharperPlugin.ReqnrollRiderPlugin.Icons;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

[ProjectFileType(typeof(GherkinProjectFileType))]
public class GherkinProjectFileLanguageService(GherkinProjectFileType projectFileType) : ProjectFileLanguageService(projectFileType)
{

    public override ILexerFactory GetMixedLexerFactory(ISolution solution, IBuffer buffer, IPsiSourceFile sourceFile = null)
    {
        return GherkinLanguage.Instance.LanguageService().NotNull().GetPrimaryLexerFactory();
    }

    protected override PsiLanguageType PsiLanguageType => GherkinLanguage.Instance.NotNull();

    public override IconId Icon => ReqnrollIcons.ReqnrollIcon;
}