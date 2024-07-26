using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions
{
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