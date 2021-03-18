using System.Collections.Generic;
using System.Linq;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinKeywordsCompletionProvider : ItemsProviderOfSpecificContext<GherkinSpecificCodeCompletionContext>
    {
        private static readonly List<string> ValidKeywordsInFeature = new List<string>
        {
            "Background",
            "Feature",
            "Rule",
            "Scenario",
            "Scenario Outline",
        };

        private static readonly List<string> ValidKeywordsInScenario = new List<string>
        {
            "And",
            "Background",
            "But",
            "Examples",
            "Feature",
            "Given",
            "Rule",
            "Scenario",
            "Scenario Outline",
            "Then",
            "When",
        };

        protected override bool IsAvailable(GherkinSpecificCodeCompletionContext context)
        {
            var codeCompletionType = context.BasicContext.CodeCompletionType;
            return codeCompletionType == CodeCompletionType.BasicCompletion || codeCompletionType == CodeCompletionType.SmartCompletion;
        }

        protected override bool AddLookupItems(GherkinSpecificCodeCompletionContext context, IItemsCollector collector)
        {
            var settings = context.BasicContext.PsiServices.GetComponent<SpecflowSettingsProvider>();
            var keywordProvider = context.BasicContext.PsiServices.LanguageManager.GetService<GherkinKeywordProvider>(GherkinLanguage.Instance.NotNull());
            var keywordList = keywordProvider.GetKeywordsList(context.GherkinFile.Lang ?? settings.GetSettings(context.BasicContext.File.GetProject()).Language.Feature);

            if (context.NodeUnderCursor is IGherkinScenario)
                return AddKeywordsLookupItemsForScenario(keywordList, context, collector);
            if (context.NodeUnderCursor is GherkinFeature)
                return AddKeywordsLookupItemsForFeature(keywordList, context, collector);

            return false;
        }

        private bool AddKeywordsLookupItemsForFeature(GherkinKeywordList keywordList, GherkinSpecificCodeCompletionContext context, IItemsCollector collector)
        {
            foreach (var keyword in ValidKeywordsInFeature.SelectMany(keywordList.GetTranslations).Distinct())
            {
                var lookupItem = new TextLookupItem(keyword);
                lookupItem.InitializeRanges(context.Ranges, context.BasicContext);
                collector.Add(lookupItem);
            }
            return true;
        }

        private bool AddKeywordsLookupItemsForScenario(GherkinKeywordList keywordList, GherkinSpecificCodeCompletionContext context, IItemsCollector collector)
        {
            foreach (var keyword in ValidKeywordsInScenario.SelectMany(keywordList.GetTranslations).Distinct())
            {
                var lookupItem = new TextLookupItem(keyword);
                lookupItem.InitializeRanges(context.Ranges, context.BasicContext);
                collector.Add(lookupItem);
            }
            return true;
        }

    }
}