using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Application;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Utils.Steps
{
    public interface IStepDefinitionBuilder
    {
        string GetStepDefinitionMethodNameFromPattern(GherkinStepKind stepKind, string pattern, string[] parameterNames);
        string GetStepDefinitionMethodNameFromStepText(GherkinStepKind stepKind, string stepText, bool isInsideScenarioOutline);
        IEnumerable<(string  parameterName, string parameterType)> GetStepDefinitionParameters(string stepText, bool isInsideScenarioOutline);
        string GetPattern(string stepText, bool isInsideScenarioOutline);
    }

    [ShellComponent]
    public class StepDefinitionBuilder : IStepDefinitionBuilder
    {
        private readonly IStepTextTokenizer _stepTextTokenizer;

        public StepDefinitionBuilder(IStepTextTokenizer stepTextTokenizer)
        {
            _stepTextTokenizer = stepTextTokenizer;
        }

        public string GetStepDefinitionMethodNameFromPattern(GherkinStepKind stepKind, string pattern, string[] parameterNames)
        {
            var methodNameSb = new StringBuilder(stepKind.ToString());
            var parameterIndex = 0;

            foreach (var (tokenType, tokenText) in _stepTextTokenizer.TokenizeStepPattern(pattern))
            {
                switch (tokenType)
                {
                    case StepTokenType.Text:
                        methodNameSb.Append(tokenText[0].ToUpperFast()).Append(tokenText.Substring(1));
                        break;
                    case StepTokenType.OutlineParameter:
                    case StepTokenType.Parameter:
                        if (parameterIndex < parameterNames.Length)
                        {
                            var parameterName = parameterNames[parameterIndex++];
                            methodNameSb.Append(parameterName[0].ToUpperFast()).Append(parameterName.Substring(1));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return methodNameSb.ToString();
        }

        public string GetStepDefinitionMethodNameFromStepText(GherkinStepKind stepKind, string stepText, bool isInsideScenarioOutline)
        {
            var methodNameSb = new StringBuilder(stepKind.ToString());

            foreach (var (tokenType, tokenText) in _stepTextTokenizer.TokenizeStepText(stepText, isInsideScenarioOutline))
            {
                switch (tokenType)
                {
                    case StepTokenType.Text:
                    case StepTokenType.OutlineParameter:
                        methodNameSb.Append(tokenText[0].ToUpperFast()).Append(tokenText.Substring(1));
                        break;
                    case StepTokenType.Parameter:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return methodNameSb.ToString();
        }

        public IEnumerable<(string parameterName, string parameterType)> GetStepDefinitionParameters(string stepText, bool isInsideScenarioOutline)
        {
            var parameterNumber = 1;

            foreach (var (tokenType, tokenText) in _stepTextTokenizer.TokenizeStepText(stepText, isInsideScenarioOutline))
            {
                switch (tokenType)
                {
                    case StepTokenType.OutlineParameter:
                        yield return (tokenText, "System.String");
                        break;
                    case StepTokenType.Parameter:
                        yield return ("p" + parameterNumber++, int.TryParse(tokenText, out _) ? "System.Int32" : "System.String");
                        break;
                }
            }
        }

        public string GetPattern(string stepText, bool isInsideScenarioOutline)
        {
            var patternSb = new StringBuilder();

            foreach (var (tokenType, tokenText) in _stepTextTokenizer.TokenizeStepText(stepText, isInsideScenarioOutline, false))
            {
                switch (tokenType)
                {
                    case StepTokenType.Text:
                        patternSb.Append(tokenText);
                        break;
                    case StepTokenType.OutlineParameter:
                    case StepTokenType.Parameter:
                        patternSb.Append("(.+)");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                patternSb.Append(' ');
            }

            if (patternSb.Length > 0)
                patternSb.Length--;

            return patternSb.ToString();
        }
    }
}