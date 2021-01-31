using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;

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

        [CanBeNull]
        public GherkinFeature GetFeature(string text)
        {
            return this.FindChild<GherkinFeature>(f => f.GetFeatureText() == text);
        }

        public IEnumerable<GherkinFeature> GetFeatures()
        {
            return this.Children<GherkinFeature>();
        }

        public override string ToString()
        {
            return $"GherkinFile: {FileName}";
        }
    }
}