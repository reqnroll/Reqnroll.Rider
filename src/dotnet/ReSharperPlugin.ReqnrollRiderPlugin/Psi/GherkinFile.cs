using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinFile(GherkinFile.FileMetadata metadata) : FileElementBase
{
    public class FileMetadata(string filename, string lang)
    {

        public string Filename { get; set; } = filename;
        public string Lang { get; set; } = lang;
    }

    public override NodeType NodeType => GherkinNodeTypes.FILE;
    public override PsiLanguageType Language => GherkinLanguage.Instance.NotNull();

    public string FileName => metadata.Filename;
    public string Lang => metadata.Lang;

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