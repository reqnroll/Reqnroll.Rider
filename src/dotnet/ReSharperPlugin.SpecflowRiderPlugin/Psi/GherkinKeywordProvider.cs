using System.Collections.Generic;
using JetBrains.Collections;
using JetBrains.Extension;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using Newtonsoft.Json.Linq;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinKeywordProvider
    {
        private readonly Dictionary<string, GherkinKeywordList> _allKeywords = new Dictionary<string, GherkinKeywordList>();
        public GherkinKeywordProvider()
        {
            Initialize();
        }

        private void Initialize()
        {
            var keywordsStream = typeof(GherkinKeywordProvider).Assembly.GetManifestResourceStream("ReSharperPlugin.SpecflowRiderPlugin.Psi.i18n.json");
            var keywordsStr = keywordsStream.ReadTextFromFile();
            var jObject = JObject.Parse(keywordsStr.Text);

            foreach (var (language, value) in jObject)
                _allKeywords.Add(language, new GherkinKeywordList((JObject) value));
        }

        public IReadOnlyCollection<string> GetAllKeywords(string language)
        {
            var keywordsList = GetKeywordsList(language);
            return keywordsList.GetAllKeywords();
        }

        public bool IsSpaceRequiredAfterKeyword(string language, string keyword)
        {
            var keywordsList = GetKeywordsList(language);
            return keywordsList.IsSpaceRequiredAfterKeyword(keyword);
        }

        public TokenNodeType GetTokenType(string language, string keyword)
        {
            var keywordsList = GetKeywordsList(language);
            return keywordsList.GetTokenType(keyword);
        }

        public GherkinStepKind GetStepKind(string language, string keyword)
        {
            if (keyword == "*")
                return GherkinStepKind.And;

            var keywordsList = GetKeywordsList(language);
            var englishKeyword = keywordsList.GetEnglishTokenKeyword(keyword);
            switch (englishKeyword)
            {
                case "And":
                case "But":
                    return GherkinStepKind.And;
                case "Given":
                    return GherkinStepKind.Given;
                case "When":
                    return GherkinStepKind.When;
                case "Then":
                    return GherkinStepKind.Then;
            }

            return GherkinStepKind.Unknown;
        }


        public GherkinKeywordList GetKeywordsList(string language)
        {
            if (_allKeywords.TryGetValue(language, out var keywordsList))
                return keywordsList;

            if (_allKeywords.TryGetValue(language.Substring(0, 2), out var keywordsListLang))
                return keywordsListLang;

            return _allKeywords["en"];
        }
    }
}