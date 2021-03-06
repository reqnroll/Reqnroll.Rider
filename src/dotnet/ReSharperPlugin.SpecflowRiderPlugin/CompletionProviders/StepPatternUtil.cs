using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using RE;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
    public interface IStepPatternUtil
    {
        IEnumerable<string> ExpandMatchingStepPatternWithAllPossibleParameter(SpecflowStepInfo pattern, string partialStepText, string fullStepText);
        IEnumerable<(StepPatternUtil.StepPatternTokenType tokenType, string text, bool optional)> TokenizeStepPattern(string pattern);
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
            return results;
        }

        private List<List<string>> RetrieveParameterValues(SpecflowStepInfo stepDefinitionInfo, string partialStepText, string fullStepText, List<(StepPatternTokenType tokenType, string text, bool optional)> tokenizedStepPattern)
        {
            var captureValues = new List<List<string>>();
            var captureIndex = 0;

            foreach (var (tokenType, text, _) in tokenizedStepPattern)
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

        private void BuildAllPossibleSteps(string matchedText, StringBuilder stringBuilder, List<string> results, (StepPatternTokenType tokenType, string text, bool optional)[] tokenizedStepPattern, List<List<string>> captureValues, int elementIndex, int captureIndex)
        {
            var saveStringBuilderPosition = stringBuilder.Length;
            if (tokenizedStepPattern[elementIndex].tokenType == StepPatternTokenType.Text)
            {
                foreach (var variant in ExpandAllOptionalVariant(tokenizedStepPattern[elementIndex].text))
                {
                    stringBuilder.Length = saveStringBuilderPosition;
                    stringBuilder.Append(variant);

                    if (elementIndex + 1 == tokenizedStepPattern.Length)
                        results.Add(stringBuilder.ToString());
                    else
                        BuildAllPossibleSteps(matchedText, stringBuilder, results, tokenizedStepPattern, captureValues, elementIndex + 1, captureIndex);
                }
                stringBuilder.Length = saveStringBuilderPosition;
            }
            else if (tokenizedStepPattern[elementIndex].tokenType == StepPatternTokenType.Capture)
            {
                if (tokenizedStepPattern[elementIndex].optional)
                    BuildAllPossibleSteps(matchedText, stringBuilder, results, tokenizedStepPattern, captureValues, elementIndex + 1, captureIndex + 1);
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

        private List<string> ExpandAllOptionalVariant(string text, StringBuilder buffer = null, List<string> result = null, int startPos = 0)
        {
            result = result ?? new List<string>();
            buffer = buffer ?? new StringBuilder();

            var i = startPos;
            var nextChar = '\0';
            while (i < text.Length)
            {
                var c = text[i++];
                if (i < text.Length) nextChar = text[i];
                if (c == '\\' || nextChar != '?')
                    buffer.Append(c);
                else
                {
                    var position = buffer.Length;
                    ExpandAllOptionalVariant(text, buffer, result, i + 1);
                    buffer.Length = position;
                    buffer.Append(c);
                    ExpandAllOptionalVariant(text, buffer, result, i + 1);
                    buffer.Length = position;
                    return result;
                }
            }
            result.Add(buffer.ToString());
            return result;
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

        public IEnumerable<(StepPatternTokenType tokenType, string text, bool optional)> TokenizeStepPattern(string pattern)
        {
            var i = 0;
            var previousChar = '\0';
            var buffer = new StringBuilder();
            while (i < pattern.Length)
            {
                var c = pattern[i++];
                if (c == '(' && previousChar != '\\')
                {
                    if (buffer.Length > 0)
                    {
                        yield return (StepPatternTokenType.Text, buffer.ToString(), false);
                        buffer.Clear();
                    }

                    var captureContent = ReadUntilChar(pattern, ref i, ')');
                    if (captureContent.StartsWith("?:"))
                        captureContent = captureContent.Substring(2);
                    var isOptional = i < pattern.Length && pattern[i] == '?';
                    if (isOptional)
                        i++;
                    yield return (StepPatternTokenType.Capture, captureContent, isOptional);
                }
                else
                {
                    buffer.Append(c);
                }
                previousChar = c;
            }
            if (buffer.Length > 0)
                yield return (StepPatternTokenType.Text, buffer.ToString(), false);
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