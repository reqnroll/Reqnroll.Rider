using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CucumberExpressions;
using JetBrains.Annotations;
using JetBrains.Application.Parts;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using Exception = System.Exception;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions;

class CucumberParameterType<T>(string name, string[] regexps, bool useForSnippets = true, int weight = 0)
    : IParameterType
{
    public string[] RegexStrings { get; } = regexps;
    public string Name { get; } = name;
    public int Weight { get; } = weight;
    public bool UseForSnippets { get; } = useForSnippets;

    public Type ParameterType => typeof(T);

    public CucumberParameterType(string name, params string[] regexps) : this(name, regexps, true)
    {

    }

}

class ParameterTypeRegistry : IParameterTypeRegistry
{
    private readonly List<IParameterType> _parameterTypes =
    [
        new CucumberParameterType<int>(ParameterTypeConstants.IntParameterName, ParameterTypeConstants.IntParameterRegexps, weight: 1000),
        new CucumberParameterType<string>(ParameterTypeConstants.StringParameterName, ParameterTypeConstants.StringParameterRegexps),
        new CucumberParameterType<string>(ParameterTypeConstants.WordParameterName, ParameterTypeConstants.WordParameterRegexps, false),
        new CucumberParameterType<float>(ParameterTypeConstants.FloatParameterName, ParameterTypeConstants.FloatParameterRegexpsEn, false),
        new CucumberParameterType<double>(ParameterTypeConstants.DoubleParameterName, ParameterTypeConstants.FloatParameterRegexpsEn),
    ];


    public IParameterType LookupByTypeName(string name)
    {
        if (name == "unknown")
            return null;

        var paramType = _parameterTypes.FirstOrDefault(pt => pt.Name == name);
        if (paramType != null)
            return paramType;

        return new CucumberParameterType<string>("???", ".*");
    }

    public IEnumerable<IParameterType> GetParameterTypes()
    {
        return _parameterTypes;

    }
}

public interface IReqnrollStepInfoFactory
{
    ReqnrollStepInfo Create(string classFullName,
                            string methodName,
                            string[] methodParameterTypes,
                            string[] methodParameterNames,
                            GherkinStepKind stepKind,
                            string pattern,
                            [CanBeNull] IReadOnlyList<ReqnrollStepScope> classEntryScopes,
                            [CanBeNull] IReadOnlyList<ReqnrollStepScope> methodScopes);
}


[PsiSharedComponent(Instantiation.DemandAnyThreadUnsafe)]
public class ReqnrollStepInfoFactory(
    IStepPatternUtil stepPatternUtil,
    JetBrains.Util.ILogger logger
) : IReqnrollStepInfoFactory
{
    // Add parameter type registry with common types
    private static readonly ParameterTypeRegistry DefaultParameterTypeRegistry = new();

    public ReqnrollStepInfo Create(string classFullName,
                                   string methodName,
                                   string[] methodParameterTypes,
                                   string[] methodParameterNames,
                                   GherkinStepKind stepKind,
                                   string pattern,
                                   IReadOnlyList<ReqnrollStepScope> classEntryScopes,
                                   IReadOnlyList<ReqnrollStepScope> methodScopes)
    {
        Regex regex;
        var finalPattern = pattern;

        Exception exCucumberExpression = null;
        // Try parsing as Cucumber expression first
        try
        {
            var expression = new CucumberExpression(pattern, DefaultParameterTypeRegistry);
            if (expression.ParameterTypes.Length > 0)
                // Convert Cucumber expression to regex pattern
                finalPattern = expression.Regex.ToString();
        }
        catch (Exception ex)
        {
            exCucumberExpression = ex;
        }

        // Not a valid Cucumber expression, treat as regex
        try
        {
            var fullMatchPattern = finalPattern;
            if (!fullMatchPattern.StartsWith("^"))
                fullMatchPattern = "^" + fullMatchPattern;
            if (!fullMatchPattern.EndsWith("$"))
                fullMatchPattern += "$";
            regex = new Regex(fullMatchPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(2));
        }
        catch (ArgumentException exRegex)
        {
            regex = null;
            if (exCucumberExpression is not null)
                logger.Warn(exCucumberExpression, $"Failed to parse step with pattern {pattern}. Not a valid a cucumber expression");
            logger.Warn(exRegex, $"Failed to parse step with pattern {pattern}. Neither a valid regex or a cucumber expression");
        }

        IReadOnlyList<ReqnrollStepScope> scopes = null;
        if (classEntryScopes != null && methodScopes != null)
        {
            var scope = new List<ReqnrollStepScope>(classEntryScopes.Count + methodScopes.Count);
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
        foreach (var (type, text, _) in stepPatternUtil.TokenizeStepPattern(pattern))
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

        var cleanPattern = pattern.TrimStart('^').TrimEnd('$');
        return new ReqnrollStepInfo(classFullName, methodName, methodParameterTypes, methodParameterNames, stepKind, cleanPattern, regex, regexesPerCapture, scopes);
    }
}