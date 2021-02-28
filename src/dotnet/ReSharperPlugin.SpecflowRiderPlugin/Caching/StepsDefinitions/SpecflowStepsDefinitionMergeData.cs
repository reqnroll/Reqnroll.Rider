using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using RE;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    public class SpecflowStepsDefinitionMergeData
    {
        public readonly OneToSetMap<IPsiSourceFile, SpecflowStepInfo> StepsDefinitionsPerFiles = new OneToSetMap<IPsiSourceFile, SpecflowStepInfo>();
        public readonly OneToSetMap<string, IPsiSourceFile> SpecflowBindingTypes = new OneToSetMap<string, IPsiSourceFile>();
        public readonly OneToSetMap<string, IPsiSourceFile> PotentialSpecflowBindingTypes = new OneToSetMap<string, IPsiSourceFile>();
    }

    public class SpecflowAssemblyStepsDefinitionMergeData
    {
        public readonly OneToSetMap<IPsiAssembly, SpecflowStepInfo> StepsDefinitionsPerFiles = new OneToSetMap<IPsiAssembly, SpecflowStepInfo>();
        public readonly Dictionary<string, IPsiAssembly> SpecflowBindingTypes = new Dictionary<string, IPsiAssembly>();
        public readonly Dictionary<string, IPsiAssembly> PotentialSpecflowBindingTypes = new Dictionary<string, IPsiAssembly>();
    }

    public class SpecflowStepInfo
    {
        public string ClassFullName { get; }
        public string MethodName { get; }
        public GherkinStepKind StepKind { get; }
        public string Pattern { get; }
        [CanBeNull]
        public Regex Regex { get; }
        [CanBeNull]
        public CharFA<string> RegexForPartialMatch { get; }

        public SpecflowStepInfo(string classFullName, string methodName, GherkinStepKind stepKind, string pattern)
        {
            ClassFullName = classFullName;
            MethodName = methodName;
            StepKind = stepKind;
            Pattern = pattern;

            try
            {
                RegexForPartialMatch = RegexExpression.Parse(Pattern).ToFA<string>();
            }
            catch (Exception)
            {
                RegexForPartialMatch = null;
            }

            try
            {
                if (!pattern.StartsWith("^"))
                    pattern = "^" + pattern;
                if (!pattern.EndsWith("$"))
                    pattern += "$";
                Regex = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(2));
            }
            catch (ArgumentException)
            {
                Regex = null;
            }
        }
    }
}