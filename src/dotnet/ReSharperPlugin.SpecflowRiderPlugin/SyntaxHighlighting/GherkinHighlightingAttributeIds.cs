using JetBrains.ReSharper.Feature.Services.Daemon.IdeaAttributes;
using JetBrains.TextControl.DocumentMarkup;
using ReSharperPlugin.SpecflowRiderPlugin.SyntaxHighlighting;

[assembly:
    RegisterHighlighterGroup(GherkinHighlightingAttributeIds.GroupID, "SpecFlow", HighlighterGroupPriority.LANGUAGE_SETTINGS,
        RiderNamesProviderType = typeof(SpecFlowHighlighterNamesProvider)),

//    RegisterHighlighter(GherkinHighlightingAttributeIds.NUMBER,
//        GroupId = GherkinHighlightingAttributeIds.GroupID,
//        EffectType = EffectType.TEXT,
//        FallbackAttributeId = IdeaHighlightingAttributeIds.NUMBER,
//        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.KEYWORD,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        EffectType = EffectType.TEXT,
        FallbackAttributeId = IdeaHighlightingAttributeIds.KEYWORD,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.STRING,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        EffectType = EffectType.TEXT,
        FallbackAttributeId = IdeaHighlightingAttributeIds.STRING,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.LINE_COMMENT,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        EffectType = EffectType.TEXT,
        FallbackAttributeId = IdeaHighlightingAttributeIds.LINE_COMMENT,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.BLOCK_COMMENT,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        EffectType = EffectType.TEXT,
        FallbackAttributeId = IdeaHighlightingAttributeIds.BLOCK_COMMENT,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.TAG,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        Layer = HighlighterLayer.SYNTAX)
]

namespace ReSharperPlugin.SpecflowRiderPlugin.SyntaxHighlighting
{
    public class GherkinHighlightingAttributeIds
    {
        public const string GroupID = "ReSharper SpecFlow Highlighters";

//        public const string NUMBER = "ReSharper SpecFlow Number";
        public const string KEYWORD = "ReSharper SpecFlow Keyword";
        public const string STRING = "ReSharper SpecFlow String";
        public const string LINE_COMMENT = "ReSharper SpecFlow Line Comment";
        public const string BLOCK_COMMENT = "ReSharper SpecFlow Block Comment";
        public const string TAG = "ReSharper SpecFlow Tag";
    }

    public class SpecFlowHighlighterNamesProvider : PrefixBasedSettingsNamesProvider
    {
        public SpecFlowHighlighterNamesProvider()
            : base("ReSharper SpecFlow", "ReSharper.SpecFlow")
        {
        }
    }
}