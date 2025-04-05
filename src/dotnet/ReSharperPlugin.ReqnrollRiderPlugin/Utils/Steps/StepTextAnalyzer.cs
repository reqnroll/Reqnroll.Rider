using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Reqnroll.BindingSkeletons
{
    public class AnalyzedStepText
    {
        public readonly List<string> TextParts = new();
        public readonly List<AnalyzedStepParameter> Parameters = new();
    }

    public class AnalyzedStepParameter
    {
        public readonly string Type;
        public readonly string Name;
        public readonly string OriginalValue;

        public AnalyzedStepParameter(string type, string name, string originalValue)
        {
            Type = type;
            Name = name;
            OriginalValue = originalValue;
        }
    }

    public interface IStepTextAnalyzer
    {
        AnalyzedStepText Analyze(string stepText, CultureInfo bindingCulture);
    }

    /// <summary>
    /// https://github.com/reqnroll/Reqnroll/blob/cf2b7cb8b1781b793fb58c807fd529553ee4145e/Reqnroll/BindingSkeletons/StepTextAnalyzer.cs 
    /// </summary>
    public class StepTextAnalyzer : IStepTextAnalyzer
    {
        private readonly List<string> _usedParameterNames = new();

        public AnalyzedStepText Analyze(string stepText, CultureInfo bindingCulture)
        {
            var result = new AnalyzedStepText();

            var paramMatches = RecognizeQuotedTexts(stepText)
                .Concat(RecognizeDates(stepText))
                .Concat(RecognizeIntegers(stepText))
                .Concat(RecognizeDecimals(stepText, bindingCulture))
                .OrderBy(m => m.Capture.Index)
                .ThenByDescending(m => m.Capture.Length);

            int textIndex = 0;
            foreach (var paramMatch in paramMatches)
            {
                if (paramMatch.Capture.Index < textIndex)
                    continue;

                var value = paramMatch.Capture.Value;
                var index = paramMatch.Capture.Index;
                switch (value.Substring(0, 1))
                {
                    case "\"":
                        value = value.Substring(1, value.Length - 2);
                        index++;
                        break;
                    case "'":
                        value = value.Substring(1, value.Length - 2);
                        index++;
                        break;
                }
                result.TextParts.Add(stepText.Substring(textIndex, index - textIndex));
                result.Parameters.Add(AnalyzeParameter(value, bindingCulture, result.Parameters.Count, paramMatch.ParameterType));
                textIndex = index + value.Length;
            }

            result.TextParts.Add(stepText.Substring(textIndex));
            return result;
        }

        private AnalyzedStepParameter AnalyzeParameter(string value, CultureInfo bindingCulture, int paramIndex, ParameterType parameterType)
        {
            string paramName = StepParameterNameGenerator.GenerateParameterName(value, paramIndex, _usedParameterNames);

            if (parameterType == ParameterType.Int && int.TryParse(value, NumberStyles.Integer, bindingCulture, out _))
                return new AnalyzedStepParameter("Int32", paramName, value);

            if (parameterType == ParameterType.Decimal && decimal.TryParse(value, NumberStyles.Number, bindingCulture, out _))
                return new AnalyzedStepParameter("Decimal", paramName, value);

            if (parameterType == ParameterType.Date && DateTime.TryParse(value, bindingCulture, DateTimeStyles.AllowWhiteSpaces, out _))
                return new AnalyzedStepParameter("DateTime", paramName, value);

            return new AnalyzedStepParameter("String", paramName, value);
        }

        private static readonly Regex QuotesRe = new(@"""+(?<param>.*?)""+|'+(?<param>.*?)'+|(?<param>\<.*?\>)|\{\}");

        private IEnumerable<CaptureWithContext> RecognizeQuotedTexts(string stepText)
        {
            return QuotesRe.Matches(stepText)
                .Cast<Match>()
                .Select(m => m.Groups["param"].Success ? (Capture)m.Groups["param"] : m.Groups[0])
                .ToCaptureWithContext(ParameterType.Text);
        }

        private static readonly Regex IntRe = new Regex(@"-?\d+");

        private IEnumerable<CaptureWithContext> RecognizeIntegers(string stepText)
        {
            return IntRe.Matches(stepText).ToCaptureWithContext(ParameterType.Int);
        }

        private IEnumerable<CaptureWithContext> RecognizeDecimals(string stepText, CultureInfo bindingCulture)
        {
            var decimalRe = new Regex($@"-?\d+{bindingCulture.NumberFormat.NumberDecimalSeparator}\d+");
            return decimalRe.Matches(stepText).ToCaptureWithContext(ParameterType.Decimal);
        }

        private static readonly Regex DateRe = new Regex(string.Join("|", GetDateFormats()));

        /// <summary>
        /// note: space separator not supported to prevent clashes
        /// </summary>
        private static IEnumerable<string> GetDateFormats()
        {
            yield return GetDateFormat("/");
            yield return GetDateFormat("-");
            yield return GetDateFormat(".");
        }

        private static string GetDateFormat(string separator)
        {
            var separatorEscaped = Regex.Escape(separator);
            return @"\d{1,4}" + separatorEscaped + @"\d{1,4}" + separatorEscaped + @"\d{1,4}";
        }

        private IEnumerable<CaptureWithContext> RecognizeDates(string stepText)
        {
            return DateRe.Matches(stepText).ToCaptureWithContext(ParameterType.Date);
        }
    }

    internal static class MatchCollectionExtensions
    {
        public static IEnumerable<CaptureWithContext> ToCaptureWithContext(this MatchCollection collection, ParameterType parameterType)
        {
            return collection.Cast<Capture>().ToCaptureWithContext(parameterType);
        }

        public static IEnumerable<CaptureWithContext> ToCaptureWithContext(this IEnumerable<Capture> collection, ParameterType parameterType)
        {
            return collection.Select(c => new CaptureWithContext(c, parameterType));
        }
    }

    internal class CaptureWithContext
    {
        public Capture Capture { get; }

        public ParameterType ParameterType { get; }

        public CaptureWithContext(Capture capture, ParameterType parameterType)
        {
            Capture = capture;
            ParameterType = parameterType;
        }
    }

    internal enum ParameterType
    {
        Text,
        Int,
        Decimal,
        Date
    }
}