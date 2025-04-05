using System.Collections.Generic;
using System.Text.RegularExpressions;
using CucumberExpressions;
using JetBrains.DocumentModel;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.References;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public static class GherkinPsiUtil
{
    private static readonly ParameterTypeRegistry DefaultParameterTypeRegistry = new();

    public static List<TextRange> BuildParameterRanges(GherkinStep step, ReqnrollStepDeclarationReference reference, DocumentRange documentRange)
    {
        var stepText = step.GetStepText();
        var stepTextWithKeyword = step.GetStepText(true);
        var stepKeywordLengthWithWhiteSpace = stepTextWithKeyword.Length - stepText.Length;

        List<TextRange> parameterRanges = new List<TextRange>();
        reference.ResolveWithoutCache();
            
        var regex = reference.RegexPattern;
        if (regex == null) return parameterRanges;

        // Try matching with Cucumber expression first
        try
        {
            var expression = new CucumberExpression(regex.ToString(), DefaultParameterTypeRegistry);
            var regexMatch = expression.Regex.Match(stepText);
            if (regexMatch.Success && regexMatch.Groups.Count > 1) // Groups[0] is the full match
            {
                for (var i = 1; i < regexMatch.Groups.Count; i++)
                {
                    var group = regexMatch.Groups[i];
                    var start = stepKeywordLengthWithWhiteSpace + group.Index;
                    var parameterStart = documentRange.StartOffset.Offset + start;
                    var parameterEnd = parameterStart + group.Length;
                    var range = new TextRange(parameterStart, parameterEnd);

                    if (!parameterRanges.Contains(range))
                        parameterRanges.Add(range);
                }
                return parameterRanges;
            }
        }
        catch
        {
            // Not a valid Cucumber expression, fall back to regex matching
        }

        // Existing regex matching logic...
        var regexMatchExisting = regex.Match(stepText);
        if (regexMatchExisting.Success)
        {
            foreach (Group matchGroup in regexMatchExisting.Groups)
            {
                if(matchGroup.Value == stepText)
                    continue;
                    
                var start = stepKeywordLengthWithWhiteSpace + matchGroup.Index;

                var parameterStart = documentRange.StartOffset.Offset+start;
                var parameterEnd = parameterStart + matchGroup.Length;
                var range = new TextRange(parameterStart, parameterEnd);
                    
                if (!parameterRanges.Contains(range))
                {
                    parameterRanges.Add(range);
                }
            }
        }

        return parameterRanges;
    }
}