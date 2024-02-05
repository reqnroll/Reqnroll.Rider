using System.Collections.Generic;
using JetBrains.Annotations;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions
{
    public class ReqnrollStepDefinitionCacheClassEntry
    {
        public string ClassName { get; }
        public bool HasReqnrollBindingAttribute { get; }
        [CanBeNull] public IReadOnlyList<ReqnrollStepScope> Scopes { get; }
        public IList<ReqnrollStepDefinitionCacheMethodEntry> Methods { get; } = new List<ReqnrollStepDefinitionCacheMethodEntry>();

        public ReqnrollStepDefinitionCacheClassEntry(string className, bool hasReqnrollBindingAttribute, [CanBeNull] IReadOnlyList<ReqnrollStepScope> scopes = null)
        {
            ClassName = className;
            HasReqnrollBindingAttribute = hasReqnrollBindingAttribute;
            Scopes = scopes;
        }

        public ReqnrollStepDefinitionCacheMethodEntry AddMethod(
            string methodName,
            string[] methodParameterTypes,
            string[] methodParameterNames,
            [CanBeNull] IReadOnlyList<ReqnrollStepScope> methodScopes
        )
        {
            var methodCacheEntry = new ReqnrollStepDefinitionCacheMethodEntry(methodName, methodParameterTypes, methodParameterNames, methodScopes);
            Methods.Add(methodCacheEntry);
            return methodCacheEntry;
        }
    }

    public class ReqnrollStepDefinitionCacheMethodEntry
    {
        public string MethodName { get; }
        public IList<ReqnrollStepDefinitionCacheStepEntry> Steps { get; } = new List<ReqnrollStepDefinitionCacheStepEntry>();
        [CanBeNull] public IReadOnlyList<ReqnrollStepScope> Scopes { get; }
        public string[] MethodParameterTypes { get; }
        public string[] MethodParameterNames { get; }

        public ReqnrollStepDefinitionCacheMethodEntry(
            string methodName,
            string[] methodParameterTypes,
            string[] methodParameterNames,
            [CanBeNull] IReadOnlyList<ReqnrollStepScope> scopes
        )
        {
            MethodName = methodName;
            MethodParameterTypes = methodParameterTypes;
            MethodParameterNames = methodParameterNames;
            Scopes = scopes;
        }

        public void AddStep(
            GherkinStepKind stepKind,
            string pattern
        )
        {
            var stepCacheEntry = new ReqnrollStepDefinitionCacheStepEntry(stepKind, pattern);
            Steps.Add(stepCacheEntry);
        }
    }

    public class ReqnrollStepDefinitionCacheStepEntry
    {
        public GherkinStepKind StepKind { get; }
        public string Pattern { get; }

        public ReqnrollStepDefinitionCacheStepEntry(GherkinStepKind stepKind, string pattern)
        {
            Pattern = pattern;
            StepKind = stepKind;
        }
    }
}