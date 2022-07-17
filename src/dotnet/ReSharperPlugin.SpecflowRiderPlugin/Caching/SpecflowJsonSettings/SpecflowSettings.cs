using System.Xml.Serialization;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings
{
    [XmlRoot("specFlow")]
    public class SpecflowSettings
    {
        [XmlElement("language")]
        public SpecflowSettingsLanguage Language { get; set; } = new();
        [XmlElement("bindingCulture")]
        public SpecflowSettingsBindingCulture BindingCulture { get; set; } = new();

        public SpecflowSettings()
        {
            Language.Feature = "en";
            Language.Tool = "en";
            BindingCulture.Name = "en";
        }
    }
}