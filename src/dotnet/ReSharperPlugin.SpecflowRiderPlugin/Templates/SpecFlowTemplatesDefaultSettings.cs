using System.IO;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using JetBrains.UI.ThemedIcons;

namespace ReSharperPlugin.SpecflowRiderPlugin.Templates
{
    [ShellComponent]
    public class SpecFlowTemplatesDefaultSettings : IHaveDefaultSettingsStream
    {
        static SpecFlowTemplatesDefaultSettings() => TemplateImage.Register("SpecFlowFile", SpecFlowThemedIcons.Specflow.Id);
        
        
        public Stream GetDefaultSettingsStream(Lifetime lifetime)
        {
            var stream = typeof(SpecFlowTemplatesDefaultSettings).Assembly.GetManifestResourceStream("ReSharperPlugin.SpecflowRiderPlugin.Templates.FileTemplates.xml").NotNull();
            
            lifetime.OnTermination(stream);
            
            return stream;
        }

        public string Name => "SpecFlow default FileTemplates";
    }
}