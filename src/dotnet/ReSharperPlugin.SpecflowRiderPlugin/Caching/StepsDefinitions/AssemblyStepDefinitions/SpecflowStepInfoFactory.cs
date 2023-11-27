using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions
{
    public interface ISpecflowStepInfoFactory
    {
        SpecflowStepInfo Create(
            string classFullName,
            string methodName,
            string[] methodParameterTypes,
            GherkinStepKind stepKind,
            string pattern,
            [CanBeNull] IReadOnlyList<SpecflowStepScope> classEntryScopes,
            [CanBeNull] IReadOnlyList<SpecflowStepScope> methodScopes
        );
    }

    [PsiSharedComponent]
    public class SpecflowStepInfoFactory : ISpecflowStepInfoFactory
    {
        private readonly IStepPatternUtil _stepPatternUtil;

        public SpecflowStepInfoFactory(IStepPatternUtil stepPatternUtil)
        {
            _stepPatternUtil = stepPatternUtil;
        }

        public SpecflowStepInfo Create(
            string classFullName,
            string methodName,
            string[] methodParameterTypes,
            GherkinStepKind stepKind,
            string pattern,
            IReadOnlyList<SpecflowStepScope> classEntryScopes,
            IReadOnlyList<SpecflowStepScope> methodScopes
        )
        {
            Regex regex;
            try
            {
                var fullMatchPattern = pattern;
                if (!fullMatchPattern.StartsWith("^"))
                    fullMatchPattern = "^" + fullMatchPattern;
                if (!fullMatchPattern.EndsWith("$"))
                    fullMatchPattern += "$";
                regex = new Regex(fullMatchPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(2));
            }
            catch (ArgumentException)
            {
                regex = null;
            }

            IReadOnlyList<SpecflowStepScope> scopes = null;
            if (classEntryScopes != null && methodScopes != null)
            {
                var scope = new List<SpecflowStepScope>(classEntryScopes.Count + methodScopes.Count);
                scope.AddRange(classEntryScopes);
                scope.AddRange(methodScopes);
                scopes = scope;
            }
            else if (classEntryScopes != null)
                scopes = classEntryScopes;
            else if (methodScopes != null)
                scopes = methodScopes;

            var regexesPerCapture = new List<Regex>();
            var partialPattern = new StringBuilder();
            var error = false;
            foreach (var (type, text, _) in _stepPatternUtil.TokenizeStepPattern(pattern))
            {
                switch (type)
                {
                    case StepPatternUtil.StepPatternTokenType.Text:
                        partialPattern.Append(text);
                        break;
                    case StepPatternUtil.StepPatternTokenType.Capture:
                        var captureText = text;
                        if (text == ".+")
                            captureText = ".+?";
                        else if (text == ".*")
                            captureText = ".*?";
                        partialPattern.Append('(').Append(captureText).Append(")");
                        try
                        {
                            regexesPerCapture.Add(new Regex("^" + partialPattern + "(?:(?:[ \"\\)])|$)", RegexOptions.Compiled));
                        }
                        catch (ArgumentException)
                        {
                            error = true;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (error)
                    break;
            }

            return new SpecflowStepInfo(classFullName, methodName, methodParameterTypes, stepKind, pattern, regex, regexesPerCapture, scopes);
        }
    }
}