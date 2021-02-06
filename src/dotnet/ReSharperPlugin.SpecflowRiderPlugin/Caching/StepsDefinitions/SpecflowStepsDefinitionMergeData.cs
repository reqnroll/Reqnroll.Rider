using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    public class SpecflowStepsDefinitionMergeData
    {
        public readonly OneToSetMap<IPsiSourceFile, SpecflowStepInfo> StepsDefinitionsPerFiles = new OneToSetMap<IPsiSourceFile, SpecflowStepInfo>();
        public readonly OneToSetMap<string, IPsiSourceFile> SpecflowBindingTypes = new OneToSetMap<string, IPsiSourceFile>();
        public readonly OneToSetMap<string, IPsiSourceFile> PotentialSpecflowBindingTypes = new OneToSetMap<string, IPsiSourceFile>();
    }

    public class SpecflowStepInfo
    {
        public string ClassFullName { get; }
        public string MethodName { get; }
        public GherkinStepKind StepKind { get; }
        public string Pattern { get; }
        [CanBeNull]
        public Regex Regex { get; }

        public SpecflowStepInfo(string classFullName, string methodName, GherkinStepKind stepKind, string pattern)
        {
            ClassFullName = classFullName;
            MethodName = methodName;
            StepKind = stepKind;
            Pattern = pattern;

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