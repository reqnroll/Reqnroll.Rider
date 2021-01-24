using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Naming.Extentions;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.ReSharper.Psi.Naming.Settings;

namespace ReSharperPlugin.SpecflowRiderPlugin.Helpers
{
    public static class SpecflowStepHelper
    {
        public static (string name, string pattern, string[] parameterTypes) GetMethodNameAndParameterFromStepText(string stepText, IPsiServices psiServices, IPsiSourceFile psiSourceFile)
        {
            var options = new SuggestionOptions();
            var (cleanPattern, parameters) = CleanPattern(stepText.Trim());
            var patternNoSpace = TextHelper.ToPascalCase(cleanPattern.Replace("\"(.*)\"", "x"));
            var methodName = psiServices.Naming.Suggestion.GetDerivedName(patternNoSpace, NamedElementKinds.Method, ScopeKind.Common, CSharpLanguage.Instance, options, psiSourceFile);
            var parameterTypes = parameters.Select(parameter => int.TryParse(parameter, out _) ? "System.Int32" : "System.String").ToArray();

            return (methodName, cleanPattern, parameterTypes);
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