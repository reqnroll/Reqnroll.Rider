using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using CucumberExpressions;


namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions
{

    class CucumberParameterType<T> : IParameterType
    {
        public string[] RegexStrings { get; }
        public string Name { get; }
        public int Weight { get; }
        public bool UseForSnippets { get; }

        public Type ParameterType => typeof(T);

        public CucumberParameterType(string name, params string[] regexps) : this(name, regexps, true)
        {

        }

        public CucumberParameterType(string name, string[] regexps, bool useForSnippets = true, int weight = 0)
        {
            Name = name;
            RegexStrings = regexps;
            UseForSnippets = useForSnippets;
            Weight = weight;
        }

    }

    class ParameterTypeRegistry : IParameterTypeRegistry
    {
        private readonly List<IParameterType> _parameterTypes = new()
        {
            new CucumberParameterType<int>(ParameterTypeConstants.IntParameterName, ParameterTypeConstants.IntParameterRegexps, weight: 1000),
            new CucumberParameterType<string>(ParameterTypeConstants.StringParameterName, ParameterTypeConstants.StringParameterRegexps),
            new CucumberParameterType<string>(ParameterTypeConstants.WordParameterName, ParameterTypeConstants.WordParameterRegexps, false),
            new CucumberParameterType<float>(ParameterTypeConstants.FloatParameterName, ParameterTypeConstants.FloatParameterRegexpsEn, false),
            new CucumberParameterType<double>(ParameterTypeConstants.DoubleParameterName, ParameterTypeConstants.FloatParameterRegexpsEn)
        };


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


    [PsiSharedComponent(Instantiation.DemandAnyThreadSafe)]
    public class ReqnrollStepInfoFactory(IStepPatternUtil stepPatternUtil)
        : IReqnrollStepInfoFactory
    {

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
            var parameterTypeRegistry = new ParameterTypeRegistry();
            var expression = new CucumberExpression(pattern, parameterTypeRegistry);
            if (expression.ParameterTypes.Length > 0)
            {
                expression.ParameterTypes.ToList().ForEach(pt => Console.WriteLine($"DEBUG(ReqnrollStepInfoFactory.Create):\t{pt.Name}"));
                pattern = expression.Regex.ToString();
            }


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
            return new ReqnrollStepInfo(classFullName, methodName, methodParameterTypes, methodParameterNames, stepKind, pattern, regex, regexesPerCapture, scopes);
        }
    }
}