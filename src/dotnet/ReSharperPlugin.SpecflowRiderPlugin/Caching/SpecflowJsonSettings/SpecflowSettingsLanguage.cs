namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings
{
    public class SpecflowSettingsLanguage
    {
        private string _neutralFeature;
        
        public string Feature { get; set; }
        public string Tool { get; set; }

        public string NeutralFeature => _neutralFeature ?? (_neutralFeature = Feature.Split('-')[0]);
    }
}