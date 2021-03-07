using System.Collections.Generic;
using System.Text;
using JetBrains.Application;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.Utils.Steps
{
    public enum StepTokenType
    {
        Text,
        Parameter,
        OutlineParameter
    }

    public interface IStepTextTokenizer
    {
        IEnumerable<(StepTokenType, string)> TokenizeStepText(string text, bool isInScenarioOutline, bool ignoreSpecialCharacters = true);
        IEnumerable<(StepTokenType, string)> TokenizeStepPattern(string pattern);
    }

    [ShellComponent]
    public class StepTextTokenizer : IStepTextTokenizer
    {
        private const char ParameterDelimiter = '"';
        private const char OutlineParameterStart = '<';
        private const char OutlineParameterEnd = '>';

        public IEnumerable<(StepTokenType, string)> TokenizeStepText(string text, bool isInScenarioOutline, bool ignoreSpecialCharacters = true)
        {
            var sb = new StringBuilder();

            for (var index = 0; index < text.Length; index++)
            {
                var c = text[index];
                switch (c)
                {
                    case ' ':
                    {
                        if (sb.Length > 0) yield return (StepTokenType.Text, sb.ToString());
                        sb.Clear();
                        break;
                    }
                    case OutlineParameterStart:
                    {
                        if (isInScenarioOutline)
                        {
                            if (sb.Length > 0) yield return (StepTokenType.Text, sb.ToString());
                            index++;
                            yield return (StepTokenType.OutlineParameter, ReadUntilChar(text, ref index, OutlineParameterEnd));
                        }
                        break;
                    }
                    case ParameterDelimiter:
                    {
                        if (sb.Length > 0) yield return (StepTokenType.Text, sb.ToString());
                        index++;
                        yield return (StepTokenType.Parameter, ReadUntilChar(text, ref index, ParameterDelimiter));
                        break;
                    }
                    default:
                    {
                        if (ignoreSpecialCharacters)
                        {
                            if (c.IsLetterOrDigitFast())
                                sb.Append(c);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                    }
                }
            }

            if (sb.Length > 0)
                yield return (StepTokenType.Text, sb.ToString());
        }

        public IEnumerable<(StepTokenType, string)> TokenizeStepPattern(string pattern)
        {
            var sb = new StringBuilder();

            for (var index = 0; index < pattern.Length; index++)
            {
                var c = pattern[index];
                switch (c)
                {
                    case ' ':
                    {
                        if (sb.Length > 0) yield return (StepTokenType.Text, sb.ToString());
                        sb.Clear();
                        break;
                    }
                    case '(':
                    {
                        if (sb.Length > 0) yield return (StepTokenType.Text, sb.ToString());
                        index++;
                        ReadUntilChar(pattern, ref index, ')');
                        yield return (StepTokenType.Parameter, null);
                        break;
                    }
                    default:
                    {
                        if (c.IsLetterOrDigitFast())
                            sb.Append(c);
                        break;
                    }
                }
            }

            if (sb.Length > 0)
                yield return (StepTokenType.Text, sb.ToString());
        }

        private string ReadUntilChar(string text, ref int index, char endCharacter)
        {
            var parameter = new StringBuilder();
            while (index < text.Length)
            {
                var c = text[index++];
                if (c == endCharacter)
                    break;

                parameter.Append(c);
            }

            return parameter.ToString();
        }
    }
}