using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.AspectLookupItems.BaseInfrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.AspectLookupItems.Info;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
    public class GherkinLookupItemFactory : LookupItemFactoryBase
    {
        [CanBeNull]
        private static GherkinLookupItemFactory _instance;
        [NotNull]
        public static GherkinLookupItemFactory Instance => _instance ?? (_instance = new GherkinLookupItemFactory());

        public LookupItem<TextualInfo> CreateKeywordLookupItem(TextLookupRanges textLookupRanges, string keyword)
        {
            return CreateTextLookupItem(textLookupRanges, keyword, "something", true, true);
        }
    }
}