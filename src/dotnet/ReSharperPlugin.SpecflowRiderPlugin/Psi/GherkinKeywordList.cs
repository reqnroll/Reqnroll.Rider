using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Collections;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Util;
using Newtonsoft.Json.Linq;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinKeywordList
    {
        // i18n.json file contains list of keywords and some meta-information about the language. At the moment it's three attributes below. 
        private static readonly string[] GherkinLanguageMetaAttributes = {"name", "native", "encoding"};

        private static readonly Dictionary<string, GherkinTokenType> TokenTypes =
            new Dictionary<string, GherkinTokenType>
            {
                {"Feature", GherkinTokenTypes.FEATURE_KEYWORD},
                {"Background", GherkinTokenTypes.BACKGROUND_KEYWORD},
                {"Scenario", GherkinTokenTypes.SCENARIO_KEYWORD},
                {"Rule", GherkinTokenTypes.RULE_KEYWORD},
                {"Example", GherkinTokenTypes.EXAMPLE_KEYWORD},
                {"Scenario Outline", GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD},
                {"Examples", GherkinTokenTypes.EXAMPLES_KEYWORD},
                {"Scenarios", GherkinTokenTypes.EXAMPLES_KEYWORD},
                {"Given", GherkinTokenTypes.STEP_KEYWORD},
                {"When", GherkinTokenTypes.STEP_KEYWORD},
                {"Then", GherkinTokenTypes.STEP_KEYWORD},
                {"And", GherkinTokenTypes.STEP_KEYWORD},
                {"But", GherkinTokenTypes.STEP_KEYWORD},
                {"*", GherkinTokenTypes.STEP_KEYWORD}
            };

        private readonly HashSet<string> _spaceAfterKeywords = new HashSet<string>();
        private readonly Dictionary<string, string> _translatedKeywords = new  Dictionary<string, string>();
        private readonly Dictionary<string, GherkinTokenType> _translatedTokenTypes = new  Dictionary<string, GherkinTokenType>();

        // Need to use Descending comparer, because long keywords should be first.
        // For example: "Scenario" keyword is a part of "Scenario Outline" keyword.
        private readonly SortedSet<string> _allKeywords = new SortedSet<string>(new DescendingComparer<string>());

        public GherkinKeywordList(JObject keywords)
        {
            foreach (var (key, values) in keywords)
            {
                if (GherkinLanguageMetaAttributes.Contains(key))
                    continue;

                var keyword = CapitalizeAndFixSpace(key);
                var tokenType = TokenTypes[keyword];

                foreach (var jToken in values)
                {
                    var translatedKeyword = jToken.Value<string>();
                    if (translatedKeyword.EndsWith(" "))
                    {
                        translatedKeyword = translatedKeyword.Substring(0, translatedKeyword.Length - 1);
                        _spaceAfterKeywords.Add(translatedKeyword);
                    }
                    
                    _translatedKeywords[translatedKeyword] = keyword;
                    _translatedTokenTypes[translatedKeyword] = tokenType;
                    _allKeywords.Add(translatedKeyword);
                }
            }
        }

        private static string CapitalizeAndFixSpace(string keyword)
        {
            var result = new StringBuilder();
            for (var i = 0; i < keyword.Length; i++)
            {
                var c = keyword[i];
                if (i == 0)
                    c = char.ToUpper(c);

                if (char.IsUpper(c) && i > 0)
                    result.Append(' ');

                result.Append(c);
            }

            return result.ToString();
        }

        public IReadOnlyCollection<string> GetAllKeywords()
        {
            return _allKeywords;
        }
        
        public bool IsSpaceRequiredAfterKeyword(string keyword)
        {
            return _spaceAfterKeywords.Contains(keyword);
        }

        public TokenNodeType GetTokenType(string keyword)
        {
            return _translatedTokenTypes[keyword];
        }

        public string GetEnglishTokenKeyword(string keyword)
        {
            return _translatedKeywords[keyword];
        }

        private class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }
    }
}