using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resources;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.ReqnrollJsonSettings;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders;

[Language(typeof(GherkinLanguage))]
public class GherkinKeywordsCompletionProvider : ItemsProviderOfSpecificContext<GherkinSpecificCodeCompletionContext>
{
    private static readonly List<(string keyword, bool addColon)> ValidKeywordsInFeature =
    [
        ("Background", true),
        ("Feature", true),
        ("Rule", true),
        ("Scenario", true),
        ("Scenario Outline", true),
    ];

    private static readonly List<(string keyword, bool addColon)> ValidKeywordsInScenario =
    [
        ("And", false),
        ("Background", true),
        ("But", false),
        ("Examples", true),
        ("Given", false),
        ("Rule", true),
        ("Scenario", true),
        ("Scenario Outline", true),
        ("Then", false),
        ("When", false),
    ];

    protected override bool IsAvailable(GherkinSpecificCodeCompletionContext context)
    {
        var codeCompletionType = context.BasicContext.CodeCompletionType;
        return (codeCompletionType == CodeCompletionType.BasicCompletion || codeCompletionType == CodeCompletionType.SmartCompletion) && context.IsStartOfLine;
    }

    protected override bool AddLookupItems(GherkinSpecificCodeCompletionContext context, IItemsCollector collector)
    {
        var settings = context.BasicContext.PsiServices.GetComponent<ReqnrollSettingsProvider>();
        var keywordProvider = context.BasicContext.PsiServices.LanguageManager.GetService<GherkinKeywordProvider>(GherkinLanguage.Instance.NotNull());
        var project = context.BasicContext.File.GetProject();
        var keywordList = keywordProvider.GetKeywordsList(context.GherkinFile.Lang ?? settings.GetSettings(project).Language.Feature);

        if (context.NodeUnderCursor is IGherkinScenario)
            return AddKeywordsLookupItemsForScenario(keywordList, context, collector);
        if (context.NodeUnderCursor is GherkinFeature)
            return AddKeywordsLookupItemsForFeature(keywordList, context, collector);

        return false;
    }

    private bool AddKeywordsLookupItemsForFeature(GherkinKeywordList keywordList, GherkinSpecificCodeCompletionContext context, IItemsCollector collector)
    {
        foreach (var (keyword, addColon) in ValidKeywordsInFeature
                     .SelectMany(k => keywordList.GetTranslations(k.keyword).Select(keyword => (keyword, k.addColon)))
                     .Where(x => x.keyword.StartsWith(context.RelatedText, StringComparison.InvariantCultureIgnoreCase))
                     .Distinct(x => x.keyword))
        {
            AddKeywordItem(context, collector, keyword, addColon);
        }
        return true;
    }

    private bool AddKeywordsLookupItemsForScenario(GherkinKeywordList keywordList, GherkinSpecificCodeCompletionContext context, IItemsCollector collector)
    {
        foreach (var (keyword, addColon) in ValidKeywordsInScenario
                     .SelectMany(k => keywordList.GetTranslations(k.keyword).Select(keyword => (keyword, k.addColon)))
                     .Where(x => x.keyword.StartsWith(context.RelatedText, StringComparison.InvariantCultureIgnoreCase))
                     .Distinct(x => x.keyword))
        {
            AddKeywordItem(context, collector, keyword, addColon);
        }
        return true;
    }

    private static void AddKeywordItem(GherkinSpecificCodeCompletionContext context, IItemsCollector collector, string keyword, bool addColon)
    {
        var completionText = keyword;
        if (addColon)
            completionText += ":";
        var lookupItem = new CompletionKeywordLookupItem(completionText, PsiSymbolsThemedIcons.Keyword.Id);
        lookupItem.InitializeRanges(context.Ranges, context.BasicContext);
        collector.Add(lookupItem);
    }
}