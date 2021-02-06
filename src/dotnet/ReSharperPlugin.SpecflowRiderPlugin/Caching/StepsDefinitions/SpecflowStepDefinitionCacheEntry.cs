using System.Collections.Generic;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    public class SpecflowStepDefinitionCacheClassEntry
    {
        public string ClassName { get; }
        public bool HasSpecflowBindingAttribute { get; }
        public IList<SpecflowStepDefinitionCacheMethodEntry> Methods { get; } = new List<SpecflowStepDefinitionCacheMethodEntry>();

        public SpecflowStepDefinitionCacheClassEntry(string className, bool hasSpecflowBindingAttribute)
        {
            ClassName = className;
            HasSpecflowBindingAttribute = hasSpecflowBindingAttribute;
        }

        public SpecflowStepDefinitionCacheMethodEntry AddMethod(string methodName)
        {
            var methodCacheEntry = new SpecflowStepDefinitionCacheMethodEntry(methodName);
            Methods.Add(methodCacheEntry);
            return methodCacheEntry;
        }
    }

    public class SpecflowStepDefinitionCacheMethodEntry
    {
        public string MethodName { get; }
        public IList<SpecflowStepDefinitionCacheStepEntry> Steps { get; } = new List<SpecflowStepDefinitionCacheStepEntry>();

        public SpecflowStepDefinitionCacheMethodEntry(string methodName)
        {
            MethodName = methodName;
        }

        public void AddStep(GherkinStepKind stepKind, string pattern)
        {
            var stepCacheEntry = new SpecflowStepDefinitionCacheStepEntry(stepKind, pattern);
            Steps.Add(stepCacheEntry);
        }
    }

    public class SpecflowStepDefinitionCacheStepEntry
    {
        public GherkinStepKind StepKind { get; }
        public string Pattern { get; }

        public SpecflowStepDefinitionCacheStepEntry(GherkinStepKind stepKind, string pattern)
        {
            Pattern = pattern;
            StepKind = stepKind;
        }
    }
}