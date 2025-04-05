using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;

public class ReqnrollStepsDefinitionMergeData
{
    public readonly OneToSetMap<IPsiSourceFile, ReqnrollStepInfo> StepsDefinitionsPerFiles = new();
    public readonly OneToSetMap<string, IPsiSourceFile> ReqnrollBindingTypes = new();
    public readonly OneToSetMap<string, IPsiSourceFile> PotentialReqnrollBindingTypes = new();
}

public class ReqnrollAssemblyStepsDefinitionMergeData
{
    public readonly OneToSetMap<IPsiAssembly, ReqnrollStepInfo> StepsDefinitionsPerFiles = new();
    public readonly Dictionary<string, IPsiAssembly> ReqnrollBindingTypes = new();
    public readonly Dictionary<string, IPsiAssembly> PotentialReqnrollBindingTypes = new();
}

public class ReqnrollStepInfo(
    string classFullName,
    string methodName,
    string[] methodParameterTypes,
    string[] methodParameterNames,
    GherkinStepKind stepKind,
    string pattern,
    [CanBeNull] Regex regex,
    List<Regex> regexesPerCapture,
    IReadOnlyList<ReqnrollStepScope> scopes)
{
    public string ClassFullName { get; } = classFullName;
    public string MethodName { get; } = methodName;
    public string[] MethodParameterTypes { get; } = methodParameterTypes;
    public string[] MethodParameterNames { get; } = methodParameterNames;
    public GherkinStepKind StepKind { get; } = stepKind;
    public string Pattern { get; } = pattern;
    [CanBeNull]
    public Regex Regex { get; } = regex;
    public List<Regex> RegexesPerCapture { get; } = regexesPerCapture;
    public IReadOnlyList<ReqnrollStepScope> Scopes { get; } = scopes;

}