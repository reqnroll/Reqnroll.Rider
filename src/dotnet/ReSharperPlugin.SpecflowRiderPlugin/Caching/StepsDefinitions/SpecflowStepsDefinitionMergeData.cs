using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    public class SpecflowStepsDefinitionMergeData
    {
        public readonly OneToSetMap<IPsiSourceFile, SpecflowStepInfo> StepsDefinitionsPerFiles = new();
        public readonly OneToSetMap<string, IPsiSourceFile> SpecflowBindingTypes = new();
        public readonly OneToSetMap<string, IPsiSourceFile> PotentialSpecflowBindingTypes = new();
    }

    public class SpecflowAssemblyStepsDefinitionMergeData
    {
        public readonly OneToSetMap<IPsiAssembly, SpecflowStepInfo> StepsDefinitionsPerFiles = new();
        public readonly Dictionary<string, IPsiAssembly> SpecflowBindingTypes = new();
        public readonly Dictionary<string, IPsiAssembly> PotentialSpecflowBindingTypes = new();
    }

    public class SpecflowStepInfo
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
        public IReadOnlyList<SpecflowStepScope> Scopes { get; }

        public SpecflowStepInfo(string classFullName,
                                string methodName,
                                string[] methodParameterTypes,
                                string[] methodParameterNames,
                                GherkinStepKind stepKind,
                                string pattern,
                                [CanBeNull] Regex regex,
                                List<Regex> regexesPerCapture,
                                IReadOnlyList<SpecflowStepScope> scopes)
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