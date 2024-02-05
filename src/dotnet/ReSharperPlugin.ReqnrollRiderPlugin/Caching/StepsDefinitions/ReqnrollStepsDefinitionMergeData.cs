using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions
{
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

    public class ReqnrollStepInfo
    {
        public string ClassFullName { get; }
        public string MethodName { get; }
        public string[] MethodParameterTypes { get; }
        public string[] MethodParameterNames { get; }
        public GherkinStepKind StepKind { get; }
        public string Pattern { get; }
        [CanBeNull]
        public Regex Regex { get; }
        public List<Regex> RegexesPerCapture { get; }
        public IReadOnlyList<ReqnrollStepScope> Scopes { get; }

        public ReqnrollStepInfo(string classFullName,
                                string methodName,
                                string[] methodParameterTypes,
                                string[] methodParameterNames,
                                GherkinStepKind stepKind,
                                string pattern,
                                [CanBeNull] Regex regex,
                                List<Regex> regexesPerCapture,
                                IReadOnlyList<ReqnrollStepScope> scopes)
        {
            ClassFullName = classFullName;
            MethodName = methodName;
            MethodParameterTypes = methodParameterTypes;
            MethodParameterNames = methodParameterNames;
            StepKind = stepKind;
            Pattern = pattern;
            Regex = regex;
            RegexesPerCapture = regexesPerCapture;
            Scopes = scopes;
        }
    }
}