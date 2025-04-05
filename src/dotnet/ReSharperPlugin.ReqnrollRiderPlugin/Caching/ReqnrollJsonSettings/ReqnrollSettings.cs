using System.Xml.Serialization;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.ReqnrollJsonSettings;

[XmlRoot("reqnroll")]
public class ReqnrollSettings
{
    [XmlElement("language")]
    public ReqnrollSettingsLanguage Language { get; set; } = new();
    [XmlElement("bindingCulture")]
    public ReqnrollSettingsBindingCulture BindingCulture { get; set; } = new();

    public ReqnrollSettings()
    {
        Language.Feature = "en";
        Language.Tool = "en";
        BindingCulture.Name = "en";
    }
}