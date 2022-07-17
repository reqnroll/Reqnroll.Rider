using System.Xml.Serialization;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings
{
    public class SpecflowSettingsLanguage
    {
        private string _neutralFeature;
        
        [XmlAttribute("feature")]
        public string Feature { get; set; }
        [XmlAttribute("tool")]
        public string Tool { get; set; }

        public string NeutralFeature => _neutralFeature ??= Feature.Split('-')[0];
    }
}