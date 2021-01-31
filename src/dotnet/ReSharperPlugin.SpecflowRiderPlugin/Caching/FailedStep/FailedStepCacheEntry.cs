using System.Collections.Generic;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.TestOutput;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.FailedStep
{
    public class FailedStepCacheEntry
    {
        public string FeatureText { get; set; }
        public string ScenarioText { get; set; }
        public IList<StepTestOutput> StepsOutputs { get; set; }

        protected bool Equals(FailedStepCacheEntry other)
        {
            return FeatureText == other.FeatureText && ScenarioText == other.ScenarioText;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((FailedStepCacheEntry) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FeatureText != null ? FeatureText.GetHashCode() : 0) * 397) ^ (ScenarioText != null ? ScenarioText.GetHashCode() : 0);
            }
        }
    }
}