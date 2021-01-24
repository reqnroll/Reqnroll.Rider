using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    public class SpecflowStepDefinitionCacheEntry
    {
        [CanBeNull]
        public Regex Regex { get; }
        public string Pattern { get; }
        public string MethodName { get; }
        public GherkinStepKind StepKind { get; }

        public SpecflowStepDefinitionCacheEntry(string pattern, GherkinStepKind stepKind, string methodName)
        {
            Pattern = pattern;
            try
            {
                Regex = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(2));
            }
            catch (ArgumentException)
            {
                Regex = null;
            }
            StepKind = stepKind;
            MethodName = methodName;
        }
    }

}