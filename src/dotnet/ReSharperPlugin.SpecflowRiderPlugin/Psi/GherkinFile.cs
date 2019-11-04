using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinFile : FileElementBase
    {
        public override NodeType NodeType => GherkinNodeTypes.FILE;
        public override PsiLanguageType Language => GherkinLanguage.Instance.NotNull();
        
        public string FileName { get; }

        public GherkinFile(string fileName)
        {
            FileName = fileName;
        }
        
        public override string ToString()
        {
            return $"GherkinFile: {FileName}";
        }
    }
}