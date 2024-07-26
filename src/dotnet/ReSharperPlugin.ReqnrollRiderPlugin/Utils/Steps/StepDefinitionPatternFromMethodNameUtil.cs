using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains;
using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Utils.Steps
{
    public interface IUnderscoresMethodNameStepDefinitionUtil
    {
        string BuildRegexFromMethodName(IMethodDeclaration methodDeclaration);
    }

    [ShellComponent(Instantiation.DemandAnyThreadSafe)]
    public class StepDefinitionPatternFromMethodNameUtil : IUnderscoresMethodNameStepDefinitionUtil
    {
        public string BuildRegexFromMethodName(IMethodDeclaration methodDeclaration)
        {
            if (methodDeclaration.DeclaredName.Contains('_'))
                return BuildFromUnderscoreNotation(methodDeclaration);
            return BuildFromPascalNotation(methodDeclaration);
        }

        // FIXME: WIP, need bigger reworks to support this.
        // https://docs.reqnroll.net/projects/reqnroll/en/latest/Bindings/Step-Definitions.html#step-matching-styles-rules
        // - Upper case is not "working" so steps build from this method should be case insensitive
        // - When there is an _ it does not fall into this case so it's hard for the scenario where both are mixed: GivenIHaveEntered_P0_IntoTheCalculator
        private string BuildFromPascalNotation(IMethodDeclaration methodDeclaration)
        {
            var words = new List<string>();

            var pos = 0;
            var parameterIndex = 0;
            var parameterNames = methodDeclaration.Params?.ParameterDeclarations.Select(x => x.DeclaredName).ToList() ?? EmptyList<string>.InstanceList;
            var methodName = methodDeclaration.DeclaredName;

            while (pos < methodName.Length)
            {
                var (token, isParameter) = ReadNext(parameterNames, parameterIndex, methodName, pos);
                pos += token.Length;
                if (isParameter)
                {
                    if (IsIntNumber(methodDeclaration, parameterIndex))
                        words.Add("(\\d+)");
                    else
                        words.Add("(.+)");
                }
                else
                    words.Add(token.ToLowerInvariant());
            }

            return string.Join(" ", words.Skip(1));
        }

        private static bool IsIntNumber(IMethodDeclaration methodDeclaration, int parameterIndex)
        {
            var type = methodDeclaration.Params?.ParameterDeclarations[parameterIndex].Type;
            if (type is not IDeclaredType clrTypeName)
                return false;
            return clrTypeName.IsPredefinedIntegral();
        }

        private (string, bool) ReadNext(IList<string> parameterNames, int parameterIndex, string methodName, int pos)
        {
            if (parameterIndex < parameterNames.Count)
            {
                if (methodName.Substring(pos).StartsWith("P" + parameterIndex))
                {
                    return ("P" + parameterIndex, true);
                }
                var nextParameterName = parameterNames[parameterIndex];
                if (methodName.Substring(pos).StartsWith(nextParameterName.ToUpperInvariant()))
                {
                    return (nextParameterName.ToUpperInvariant(), true);
                }
            }

            var i = pos + 1;
            for (; i < methodName.Length; i++)
                if (methodName[i].IsUpperFast())
                    break;

            return (methodName.Substring(pos, i - pos), false);
        }

        private string BuildFromUnderscoreNotation(IMethodDeclaration methodDeclaration)
        {
            var sb = new StringBuilder();
            var parameterIndex = 0;
            foreach (var word in methodDeclaration.DeclaredName.Split('_').Skip(1))
            {
                if (IsParameter(word, methodDeclaration, parameterIndex))
                {
                    sb.Append("(.+)");
                    parameterIndex++;
                }
                else
                    sb.Append(word);
                sb.Append(' ');
            }

            // Remove last space
            if (sb.Length > 0)
                sb.Length--;

            return sb.ToString();
        }

        private bool IsParameter(string word, IMethodDeclaration methodDeclaration, int parameterIndex)
        {
            if (word == "P" + parameterIndex)
                return true;
            if (!word.All(c => c.IsUpperFast()))
                return false;
            if (methodDeclaration.Params.ParameterDeclarations.Count <= parameterIndex)
                return false;
            if (methodDeclaration.Params.ParameterDeclarations[parameterIndex].NameIdentifier?.Name.Equals(word, StringComparison.InvariantCultureIgnoreCase) == true)
                return true;
            return false;
        }
    }

}