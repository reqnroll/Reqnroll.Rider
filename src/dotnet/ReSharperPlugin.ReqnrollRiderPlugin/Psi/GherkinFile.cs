using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi
{
    public class GherkinFile : FileElementBase
    {
        public class FileMetadata
        {
            public FileMetadata(string filename, string lang)
            {
                Filename = filename;
                Lang = lang;
            }

            public string Filename { get; set; }
            public string Lang { get; set; }
        }

        public override NodeType NodeType => GherkinNodeTypes.FILE;
        public override PsiLanguageType Language => GherkinLanguage.Instance.NotNull();

        private readonly FileMetadata _metadata;

        public string FileName => _metadata.Filename;
        public string Lang => _metadata.Lang;

        public GherkinFile(FileMetadata metadata)
        {
            _metadata = metadata;
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