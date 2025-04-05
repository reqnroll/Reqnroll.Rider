using System.Linq;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.Tags;
using ReSharperPlugin.ReqnrollRiderPlugin.Icons;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders;

[Language(typeof(GherkinLanguage))]
public class TagCompletionProvider : ItemsProviderOfSpecificContext<GherkinSpecificCodeCompletionContext>
{
    protected override bool IsAvailable(GherkinSpecificCodeCompletionContext context)
    {
        var codeCompletionType = context.BasicContext.CodeCompletionType;
        return codeCompletionType == CodeCompletionType.BasicCompletion || codeCompletionType == CodeCompletionType.SmartCompletion;
    }

    protected override bool AddLookupItems(GherkinSpecificCodeCompletionContext context, IItemsCollector collector)
    {
        if (context.NodeUnderCursor is not GherkinTag)
            return false;
        if (context.RelatedText.Length == 0)
            return false;

        var textToComplete = context.RelatedText.Substring(1);

        var reqnrollTagsCache = context.BasicContext.PsiServices.GetComponent<ReqnrollTagsCache>();
        var matchingTags = reqnrollTagsCache.GetAllTags().Where(x => x.StartsWith(textToComplete)).OrderBy();

        foreach (var matchingTag in matchingTags)
        {
            var textLookupItem = new TextLookupItem('@' + matchingTag, ReqnrollIcons.ReqnrollTagIcon);
            textLookupItem.InitializeRanges(context.Ranges, context.BasicContext);
            collector.Add(textLookupItem);
        }

        return true;
    }
}