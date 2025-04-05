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
        public readonly string RegexPattern;

        public AnalyzedStepParameter(string type, string name, string regexPattern = null)
        {
            Type = type;
            Name = name;
            RegexPattern = regexPattern;
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

                const string singleQuoteRegexPattern = "[^']*";
                const string doubleQuoteRegexPattern = "[^\"\"]*";
                const string defaultRegexPattern = ".*";

                string regexPattern = defaultRegexPattern;
                string value = paramMatch.Capture.Value;
                int index = paramMatch.Capture.Index;
                switch (paramMatch.ParameterType)
                {
                    case ParameterType.Text:
                        regexPattern = "{string}";
                        break;
                    case ParameterType.Int:
                        regexPattern = "{int}";
                        break;
                    case ParameterType.Decimal:
                        regexPattern = "{decimal}";
                        break;
                    default:
                        regexPattern = "{string}";
                        break;
                }
                switch (value.Substring(0, 1))
                {
                    case "\"":
                        regexPattern = doubleQuoteRegexPattern;
                        value = value.Substring(1, value.Length - 2);
                        index++;
                        break;
                    case "'":
                        regexPattern = singleQuoteRegexPattern;
                        value = value.Substring(1, value.Length - 2);
                        index++;
                        break;
                }
                result.TextParts.Add(stepText.Substring(textIndex, index - textIndex));
                result.Parameters.Add(AnalyzeParameter(value, bindingCulture, result.Parameters.Count, regexPattern, paramMatch.ParameterType));
                textIndex = index + value.Length;
            }

            result.TextParts.Add(stepText.Substring(textIndex));
            return result;
        }

        private AnalyzedStepParameter AnalyzeParameter(string value, CultureInfo bindingCulture, int paramIndex, string regexPattern, ParameterType parameterType)
        {
            string paramName = StepParameterNameGenerator.GenerateParameterName(value, paramIndex, _usedParameterNames);

            if (parameterType == ParameterType.Int && int.TryParse(value, NumberStyles.Integer, bindingCulture, out _))
                return new AnalyzedStepParameter("Int32", paramName, regexPattern);

            if (parameterType == ParameterType.Decimal && decimal.TryParse(value, NumberStyles.Number, bindingCulture, out _))
                return new AnalyzedStepParameter("Decimal", paramName, regexPattern);

            if (parameterType == ParameterType.Date && DateTime.TryParse(value, bindingCulture, DateTimeStyles.AllowWhiteSpaces, out _))
                return new AnalyzedStepParameter("DateTime", paramName, regexPattern);

            return new AnalyzedStepParameter("String", paramName, regexPattern);
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