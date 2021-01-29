using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Naming.Extentions;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.ReSharper.Psi.Naming.Settings;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Helpers
{
    public static class SpecflowStepHelper
    {
        public static (string name, string pattern, string[] parameterTypes) GetMethodNameAndParameterFromStepText(GherkinStepKind stepKind, string stepText, IPsiServices psiServices, IPsiSourceFile psiSourceFile)
        {
            var options = new SuggestionOptions();
            var (cleanPattern, parameters) = CleanPattern(stepText.Trim());
            var patternNoSpace = stepKind + TextHelper.ToPascalCase(cleanPattern.Replace("\"(.*)\"", "X"));
            var methodName = psiServices.Naming.Suggestion.GetDerivedName(patternNoSpace, NamedElementKinds.Method, ScopeKind.Common, CSharpLanguage.Instance, options, psiSourceFile);
            var parameterTypes = parameters.Select(parameter => int.TryParse(parameter, out _) ? "System.Int32" : "System.String").ToArray();

            return (methodName, cleanPattern, parameterTypes);
        }

        public static string GetMethodNameAndParameterFromStepPattern(GherkinStepKind stepKind, string pattern, IPsiServices psiServices, IPsiSourceFile psiSourceFile)
        {
            var options = new SuggestionOptions();
            var cleanPattern = ReplacePatternCaptureWithX(pattern);
            var patternNoSpace = stepKind + TextHelper.ToPascalCase(cleanPattern);
            var methodName = psiServices.Naming.Suggestion.GetDerivedName(patternNoSpace, NamedElementKinds.Method, ScopeKind.Common, CSharpLanguage.Instance, options, psiSourceFile);

            return methodName;
        }

        private static string ReplacePatternCaptureWithX(string pattern)
        {
            var stringBuilder = new StringBuilder();

            var inCapture = false;
            var previousChar = '\0';
            foreach (var c in pattern)
            {
                if (c == '(' && previousChar != '\\')
                    inCapture = true;

                if (inCapture && c == ')' && previousChar != '\\')
                {
                    stringBuilder.Append('X');
                    inCapture = false;
                }

                if (!inCapture)
                    stringBuilder.Append(c);
                previousChar = c;
            }

            return stringBuilder.ToString();
        }

        private static (string patter, List<string> parameters) CleanPattern(string stepText)
        {
            var parameters = new List<string>();
            var pattern = new StringBuilder();
            var parameter = new StringBuilder();

            bool inQuote = false;
            for (var index = 0; index < stepText.Length; index++)
            {
                var c = stepText[index];
                if (c == '"')
                {
                    if (inQuote)
                    {
                        pattern.Append("\"(.*)\"");
                        parameters.Add(parameter.ToString());
                        parameter.Clear();
                    }
                    inQuote = !inQuote;
                }
                else
                {
                    if (inQuote)
                        parameter.Append(c);
                    else
                        pattern.Append(c);
                }
            }
            return (pattern.ToString(), parameters);
        }
    }
}