using System.Linq;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.Tags;
using ReSharperPlugin.SpecflowRiderPlugin.Icons;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
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

            var specflowTagsCache = context.BasicContext.PsiServices.GetComponent<SpecflowTagsCache>();
            var matchingTags = specflowTagsCache.GetAllTags().Where(x => x.StartsWith(context.RelatedText.Substring(1))).OrderBy();

           foreach (var matchingTag in matchingTags)
           {
               var textLookupItem = new TextLookupItem('@' + matchingTag, SpecFlowIcons.SpecFlowTagIcon);
               textLookupItem.InitializeRanges(context.Ranges, context.BasicContext);
               collector.Add(textLookupItem);
           }

            return true;
        }
    }
}