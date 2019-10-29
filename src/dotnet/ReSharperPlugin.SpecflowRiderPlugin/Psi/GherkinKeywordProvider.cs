using System;
using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinKeywordProvider
    {
        public IReadOnlyCollection<string> getAllKeywords(string language)
        {
            return new[]
                   {
                       "Функционал",
                       "Предыстория",
                       "Сценарий",
                       "Структура сценария",
                       "Примеры",
                       "Допустим",
                       "Пусть",
                       "Дано",
                       "Когда",
                       "Тогда",
                       "И",
                       "Также"
                   };
        }

        public bool isSpaceRequiredAfterKeyword(string myCurLanguage, string keyword)
        {
            return true;
        }

        public TokenNodeType getTokenType(string myCurLanguage, string keyword)
        {
            switch (keyword)
            {
                case "Функционал":
                    return GherkinTokenTypes.FEATURE_KEYWORD;
                case "Предыстория":
                    return GherkinTokenTypes.BACKGROUND_KEYWORD;
                case "Сценарий":
                    return GherkinTokenTypes.SCENARIO_KEYWORD;
                case "Структура сценария":
                    return GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD;
                case "Примеры":
                    return GherkinTokenTypes.EXAMPLES_KEYWORD;
                case "Допустим":
                case "Пусть":
                case "Дано":
                case "Когда":
                case "Тогда":
                case "И":
                case "Также":
                    return GherkinTokenTypes.STEP_KEYWORD;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyword), keyword);
            }
        }
    }
}