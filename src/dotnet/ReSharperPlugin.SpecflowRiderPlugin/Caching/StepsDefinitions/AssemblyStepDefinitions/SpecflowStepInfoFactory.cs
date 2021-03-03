using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.ReSharper.Psi;
using RE;
using ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions
{
    public interface ISpecflowStepInfoFactory
    {
        SpecflowStepInfo Create(string classFullName, string methodName, GherkinStepKind stepKind, string pattern);
    }

    [PsiSharedComponent]
    public class SpecflowStepInfoFactory : ISpecflowStepInfoFactory
    {
        private readonly IStepPatternUtil _stepPatternUtil;

        public SpecflowStepInfoFactory(IStepPatternUtil stepPatternUtil)
        {
            _stepPatternUtil = stepPatternUtil;
        }

        public SpecflowStepInfo Create(string classFullName, string methodName, GherkinStepKind stepKind, string pattern)
        {
            CharFA<string> regexForPartialMatch;
            try
            {
                regexForPartialMatch = RegexExpression.Parse(pattern).ToFA<string>();
            }
            catch (Exception)
            {
                regexForPartialMatch = null;
            }

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

            var regexesPerCapture = new List<Regex>();
            var partialPattern = new StringBuilder();
            var error = false;
            foreach (var (type, text) in _stepPatternUtil.TokenizeStepPattern(pattern))
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
                        else ;if (text == ".*")
                            captureText = ".*?";
                        partialPattern.Append('(').Append(captureText).Append(")");
                        try
                        {
                            regexesPerCapture.Add(new Regex("^" + partialPattern + "(?:(?:[ \">])|$)", RegexOptions.Compiled));
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

            return new SpecflowStepInfo(classFullName, methodName, stepKind, pattern, regex, regexForPartialMatch, regexesPerCapture);
        }
    }
}