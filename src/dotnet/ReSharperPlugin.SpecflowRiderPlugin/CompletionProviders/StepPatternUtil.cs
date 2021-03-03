using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using RE;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
    public interface IStepPatternUtil
    {
        IEnumerable<string> ExpandMatchingStepPatternWithAllPossibleParameter(SpecflowStepInfo pattern, string partialStepText, string fullStepText);
        IEnumerable<(StepPatternUtil.StepPatternTokenType tokenType, string text)> TokenizeStepPattern(string pattern);
    }

    [PsiSharedComponent]
    public class StepPatternUtil : IStepPatternUtil
    {
        public enum StepPatternTokenType
        {
            Text,
            Capture
        }

        public IEnumerable<string> ExpandMatchingStepPatternWithAllPossibleParameter(SpecflowStepInfo stepDefinitionInfo, string partialStepText, string fullStepText)
        {
            var matchedText = string.Empty;
            if (partialStepText.Length > 0)
            {
                var result = stepDefinitionInfo.RegexForPartialMatch?.Match(ParseContext.Create(partialStepText), successOnAnyState: true);
                if (result == null || result.Position != 0)
                    return EmptyList<string>.Enumerable;
                matchedText = result.Value;
            }

            var tokenizedStepPattern = TokenizeStepPattern(stepDefinitionInfo.Pattern).ToList();
            var captureValues = RetrieveParameterValues(stepDefinitionInfo, partialStepText, fullStepText, tokenizedStepPattern);

            var stringBuilder = new StringBuilder();
            var results = new List<string>();
            BuildAllPossibleSteps(matchedText, stringBuilder, results, tokenizedStepPattern.ToArray(), captureValues, 0, 0);
            // Workaround: Until the FIXME above is fixed
            if (results.Count == 0)
            {
                results.Add(stepDefinitionInfo.Pattern);
            }
            // End Workaround
            return results;
        }

        private List<List<string>> RetrieveParameterValues(SpecflowStepInfo stepDefinitionInfo, string partialStepText, string fullStepText, List<(StepPatternTokenType tokenType, string text)> tokenizedStepPattern)
        {
            var captureValues = new List<List<string>>();
            var captureIndex = 0;

            foreach (var (tokenType, text) in tokenizedStepPattern)
            {
                if (tokenType == StepPatternTokenType.Capture)
                {
                    string capturedValue = null;
                    bool preferPossibleValues = false;
                    if (stepDefinitionInfo.RegexesPerCapture.Count > captureIndex)
                    {
                        var partialRegex = stepDefinitionInfo.RegexesPerCapture[captureIndex++];
                        var match = partialRegex.Match(fullStepText);
                        var matchGroup = match.Groups[captureIndex];
                        if (match.Success && matchGroup.Success)
                        {
                            capturedValue = matchGroup.Value;
                            if (matchGroup.Index > partialStepText.Length)
                                preferPossibleValues = true;
                        }
                    }
                    var possibleValues = ListPossibleValues(text).ToList();
                    if (capturedValue != null && (possibleValues.Count == 1 || !preferPossibleValues))
                        captureValues.Add(new List<string> {capturedValue});
                    else
                        captureValues.Add(possibleValues);
                }
            }

            return captureValues;
        }

        private void BuildAllPossibleSteps(string matchedText, StringBuilder stringBuilder, List<string> results, (StepPatternTokenType tokenType, string text)[] tokenizedStepPattern, List<List<string>> captureValues, int elementIndex, int captureIndex)
        {
            var saveStringBuilderPosition = stringBuilder.Length;
            if (tokenizedStepPattern[elementIndex].tokenType == StepPatternTokenType.Text)
            {
                stringBuilder.Append(tokenizedStepPattern[elementIndex].text);

                if (elementIndex + 1 == tokenizedStepPattern.Length)
                    results.Add(stringBuilder.ToString());
                else
                    BuildAllPossibleSteps(matchedText, stringBuilder, results, tokenizedStepPattern, captureValues, elementIndex + 1, captureIndex);
                stringBuilder.Length = saveStringBuilderPosition;
            }
            else if (tokenizedStepPattern[elementIndex].tokenType == StepPatternTokenType.Capture)
            {
                foreach (var captureValue in captureValues[captureIndex])
                {
                    stringBuilder.Length = saveStringBuilderPosition;
                    stringBuilder.Append(captureValue);

                    if (matchedText.Length <= stringBuilder.Length)
                    {
                        if (!stringBuilder.ToString().StartsWith(matchedText))
                            continue;
                    }
                    else
                    {
                        if (!matchedText.StartsWith(stringBuilder.ToString()))
                            continue;
                    }

                    if (elementIndex + 1 == tokenizedStepPattern.Length)
                        results.Add(stringBuilder.ToString());
                    else
                        BuildAllPossibleSteps(matchedText, stringBuilder, results, tokenizedStepPattern, captureValues, elementIndex + 1, captureIndex + 1);
                }
                stringBuilder.Length = saveStringBuilderPosition;
            }
        }

        private IEnumerable<string> ListPossibleValues(string captureText)
        {
            if (captureText.IndexOf('|') == -1)
                yield return '(' + captureText + ')';
            else
            {
                foreach (var substring in captureText.Split('|'))
                    yield return substring;
            }
        }

        public IEnumerable<(StepPatternTokenType tokenType, string text)> TokenizeStepPattern(string pattern)
        {
            var i = 0;
            var buffer = new StringBuilder();
            while (i < pattern.Length)
            {
                var c = pattern[i++];
                if (c == '(')
                {
                    if (buffer.Length > 0)
                    {
                        yield return (StepPatternTokenType.Text, buffer.ToString());
                        buffer.Clear();
                    }

                    var captureContent = ReadUntilChar(pattern, ref i, ')');
                    if (captureContent.StartsWith("?:"))
                        captureContent = captureContent.Substring(2);
                    yield return (StepPatternTokenType.Capture, captureContent);
                }
                else
                {
                    buffer.Append(c);
                }
            }
            if (buffer.Length > 0)
                yield return (StepPatternTokenType.Text, buffer.ToString());
        }

        private string ReadUntilChar(string text, ref int index, char endCharacter)
        {
            var parameter = new StringBuilder();
            while (index < text.Length)
            {
                var c = text[index++];
                if (c == endCharacter && text[index - 2] != '\\')
                    break;

                parameter.Append(c);
            }

            return parameter.ToString();
        }
    }
}