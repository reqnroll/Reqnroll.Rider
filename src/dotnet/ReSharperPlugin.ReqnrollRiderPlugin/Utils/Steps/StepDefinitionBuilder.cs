using System.Collections.Generic;
using System.Globalization;
using System.Text;
using JetBrains.Application;
using JetBrains.Application.Parts;
using Reqnroll.BindingSkeletons;
using Reqnroll.Tracing;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Utils.Steps
{
    public interface IStepDefinitionBuilder
    {
        string GetStepDefinitionMethodNameFromPattern(GherkinStepKind stepKind, string pattern, string[] parameterNames);
        string GetStepDefinitionMethodNameFromStepText(GherkinStepKind stepKind, string stepText, CultureInfo cultureInfo);
        IEnumerable<(string  parameterName, string parameterType)> GetStepDefinitionParameters(string stepText, CultureInfo cultureInfo);
        string GetPattern(string stepText, CultureInfo cultureInfo);
    }

    [ShellComponent(Instantiation.DemandAnyThreadUnsafe)]
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

            var parameterList = new List<(string parameterName, string parameterType)>();
            foreach (var param in result.Parameters)
                parameterList.Add((param.Name, GetCSharpTypeName(param.Type)));
            return parameterList;
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

            // First, let's check if this is a step with quoted strings
            var hasQuotedStrings = stepText.Contains("\"") || stepText.Contains("'");
            if (hasQuotedStrings)
            {
                // For steps with quoted strings, use the original text but replace the quoted parts with {string}
                var pattern = stepText;
                foreach (var param in result.Parameters)
                    if (param.Type == "String")
                    {
                        // Replace the quoted string with {string}
                        pattern = pattern.Replace($"\"{param.OriginalValue}\"", "{string}");
                        pattern = pattern.Replace($"'{param.OriginalValue}'", "{string}");
                    }
                return pattern;
            }

            // For non-quoted parameters, use the standard approach
            var sb = new StringBuilder();
            sb.Append(result.TextParts[0]);
            
            for (int i = 1; i < result.TextParts.Count; i++)
            {
                var param = result.Parameters[i - 1];
                switch (param.Type)
                {
                    case "String":
                        sb.Append("{string}");
                        break;
                    case "Int32":
                        sb.Append("{int}");
                        break;
                    case "Decimal":
                        sb.Append("{decimal}");
                        break;
                    case "DateTime":
                        sb.Append("{datetime}");
                        break;
                    default:
                        sb.AppendFormat("(.*)");
                        break;
                }
                sb.Append(result.TextParts[i]);
            }

            return sb.ToString();
        }

        private static string EscapeRegex(string text)
        {
            return text.Replace("\\ ", " ");
        }
    }

}