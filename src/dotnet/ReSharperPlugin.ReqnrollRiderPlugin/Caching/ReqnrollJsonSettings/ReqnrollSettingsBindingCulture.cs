using System.Xml.Serialization;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.ReqnrollJsonSettings
{
    public class ReqnrollSettingsBindingCulture
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}