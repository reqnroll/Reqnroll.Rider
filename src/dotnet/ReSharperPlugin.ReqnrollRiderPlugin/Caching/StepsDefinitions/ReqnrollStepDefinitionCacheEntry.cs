using System.Collections.Generic;
using JetBrains.Annotations;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;

public class ReqnrollStepDefinitionCacheClassEntry(string className, bool hasReqnrollBindingAttribute, [CanBeNull] IReadOnlyList<ReqnrollStepScope> scopes = null)
{
    public string ClassName { get; } = className;
    public bool HasReqnrollBindingAttribute { get; } = hasReqnrollBindingAttribute;
    [CanBeNull] public IReadOnlyList<ReqnrollStepScope> Scopes { get; } = scopes;
    public IList<ReqnrollStepDefinitionCacheMethodEntry> Methods { get; } = new List<ReqnrollStepDefinitionCacheMethodEntry>();

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

public class ReqnrollStepDefinitionCacheMethodEntry(
    string methodName,
    string[] methodParameterTypes,
    string[] methodParameterNames,
    [CanBeNull] IReadOnlyList<ReqnrollStepScope> scopes)
{
    public string MethodName { get; } = methodName;
    public IList<ReqnrollStepDefinitionCacheStepEntry> Steps { get; } = new List<ReqnrollStepDefinitionCacheStepEntry>();
    [CanBeNull] public IReadOnlyList<ReqnrollStepScope> Scopes { get; } = scopes;
    public string[] MethodParameterTypes { get; } = methodParameterTypes;
    public string[] MethodParameterNames { get; } = methodParameterNames;

    public void AddStep(
        GherkinStepKind stepKind,
        string pattern
    )
    {
        var stepCacheEntry = new ReqnrollStepDefinitionCacheStepEntry(stepKind, pattern);
        Steps.Add(stepCacheEntry);
    }
}

public class ReqnrollStepDefinitionCacheStepEntry(GherkinStepKind stepKind, string pattern)
{
    public GherkinStepKind StepKind { get; } = stepKind;
    public string Pattern { get; } = pattern;

}