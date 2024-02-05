using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Application;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using Reqnroll.BindingSkeletons;
using Reqnroll.Tracing;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Utils.Steps
{
    public interface IStepDefinitionBuilder
    {
        string GetStepDefinitionMethodNameFromPattern(GherkinStepKind stepKind, string pattern, string[] parameterNames);
        string GetStepDefinitionMethodNameFromStepText(GherkinStepKind stepKind, string stepText, CultureInfo cultureInfo);
        IEnumerable<(string  parameterName, string parameterType)> GetStepDefinitionParameters(string stepText, CultureInfo cultureInfo);
        string GetPattern(string stepText, CultureInfo cultureInfo);
    }

    [ShellComponent]
    public class StepDefinitionBuilder : IStepDefinitionBuilder
    {
        public string GetStepDefinitionMethodNameFromPattern(GherkinStepKind stepKind, string pattern, string[] parameterNames)
        {
            var stepTextAnalyzer = new StepTextAnalyzer();
            var result = stepTextAnalyzer.Analyze(pattern, CultureInfo.CurrentCulture);
            
            return stepKind + string.Concat(result.TextParts.ToArray()).ToIdentifier();
        }

        public string GetStepDefinitionMethodNameFromStepText(GherkinStepKind stepKind, string stepText, CultureInfo cultureInfo)
        {
            var stepTextAnalyzer = new StepTextAnalyzer();
            var result = stepTextAnalyzer.Analyze(stepText, cultureInfo);
            
            return stepKind + string.Concat(result.TextParts.ToArray()).ToIdentifier();
        }

        public IEnumerable<(string parameterName, string parameterType)> GetStepDefinitionParameters(string stepText, CultureInfo cultureInfo)
        {
            var stepTextAnalyzer = new StepTextAnalyzer();
            var result = stepTextAnalyzer.Analyze(stepText, cultureInfo);

            return result.Parameters.Select(p => (p.Name, GetCSharpTypeName(p.Type)));
        }
        
        private string GetCSharpTypeName(string type)
        {
            switch (type)
            {
                case "DateTime":
                    return "System.DateTime";
                case "String":
                    return "string";
                case "Decimal":
                    return "decimal";
                case "Int32":
                    return "int";
                default:
                    return type;
            }
        }

        public string GetPattern(string stepText, CultureInfo cultureInfo)
        {
            var stepTextAnalyzer = new StepTextAnalyzer();
            var result = stepTextAnalyzer.Analyze(stepText, cultureInfo);
            
            var pattern = new StringBuilder();

            pattern.Append(EscapeRegex(result.TextParts[0]));
            for (int i = 1; i < result.TextParts.Count; i++)
            {
                pattern.AppendFormat("({0})", result.Parameters[i-1].RegexPattern);
                pattern.Append(EscapeRegex(result.TextParts[i]));
            }

            return pattern.ToString();
        }

        private static string EscapeRegex(string text)
        {
            return Regex.Escape(text).Replace("\"", "\"\"").Replace("\\ ", " ");
        }
    }

}