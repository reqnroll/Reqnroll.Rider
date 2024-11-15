using System.IO;
using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.Application.Settings;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using ReSharperPlugin.ReqnrollRiderPlugin.Icons;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Templates
{
    [ShellComponent(Instantiation.DemandAnyThreadUnsafe)]
    public class ReqnrollTemplatesDefaultSettings : IDefaultSettingsRootKey<LiveTemplatesSettings>
    {
        public string Name => "Reqnroll default FileTemplates";

        static ReqnrollTemplatesDefaultSettings() => TemplateImage.Register("ReqnrollFile", ReqnrollIcons.ReqnrollIcon);

        public Stream GetDefaultSettingsStream(Lifetime lifetime)
        {
            var stream = typeof(ReqnrollTemplatesDefaultSettings).Assembly.GetManifestResourceStream("ReSharperPlugin.ReqnrollRiderPlugin.Templates.FileTemplates.xml").NotNull();
            lifetime.OnTermination(stream);
            return stream;
        }
    }
}