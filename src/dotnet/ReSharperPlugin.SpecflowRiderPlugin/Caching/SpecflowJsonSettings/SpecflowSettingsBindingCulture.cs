using System.Xml.Serialization;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings
{
    public class SpecflowSettingsBindingCulture
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}