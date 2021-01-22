using System.Text.RegularExpressions;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    public class SpecflowStepDefinitionCacheEntry
    {
        public Regex Regex { get; }
        public string Pattern { get; }
        public string MethodName { get; }
        public GherkinStepKind StepKind { get; }

        public SpecflowStepDefinitionCacheEntry(string pattern, GherkinStepKind stepKind, string methodName)
        {
            Pattern = pattern;
            Regex = new Regex(pattern, RegexOptions.Compiled);
            StepKind = stepKind;
            MethodName = methodName;
        }
    }

}