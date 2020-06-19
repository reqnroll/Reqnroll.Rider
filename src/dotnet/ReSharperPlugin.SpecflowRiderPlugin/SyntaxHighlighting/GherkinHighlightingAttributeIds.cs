using JetBrains.ReSharper.Feature.Services.Daemon.Attributes.Idea;
using JetBrains.TextControl.DocumentMarkup;
using ReSharperPlugin.SpecflowRiderPlugin.SyntaxHighlighting;

[assembly:
    RegisterHighlighterGroup(GherkinHighlightingAttributeIds.GroupID, "SpecFlow", HighlighterGroupPriority.LANGUAGE_SETTINGS,
        RiderNamesProviderType = typeof(SpecFlowHighlighterNamesProvider)),

    RegisterHighlighter(GherkinHighlightingAttributeIds.KEYWORD,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        EffectType = EffectType.TEXT,
        FallbackAttributeId = IdeaHighlightingAttributeIds.KEYWORD,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.TEXT,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        EffectType = EffectType.TEXT,
        FallbackAttributeId = IdeaHighlightingAttributeIds.TEXT,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.LINE_COMMENT,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        EffectType = EffectType.TEXT,
        FallbackAttributeId = IdeaHighlightingAttributeIds.DOC_COMMENT,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.TAG,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        FallbackAttributeId = IdeaHighlightingAttributeIds.METADATA,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.REGEXP_PARAMETER,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        FallbackAttributeId = IdeaHighlightingAttributeIds.PARAMETER,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.TABLE_CELL,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        FallbackAttributeId = GherkinHighlightingAttributeIds.REGEXP_PARAMETER,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.OUTLINE_PARAMETER_SUBSTITUTION,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        FallbackAttributeId = IdeaHighlightingAttributeIds.INSTANCE_FIELD,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.TABLE_HEADER_CELL,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        FallbackAttributeId = GherkinHighlightingAttributeIds.OUTLINE_PARAMETER_SUBSTITUTION,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.PIPE,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        FallbackAttributeId = GherkinHighlightingAttributeIds.KEYWORD,
        Layer = HighlighterLayer.SYNTAX),
    RegisterHighlighter(GherkinHighlightingAttributeIds.PYSTRING,
        GroupId = GherkinHighlightingAttributeIds.GroupID,
        FallbackAttributeId = IdeaHighlightingAttributeIds.STRING,
        Layer = HighlighterLayer.SYNTAX)
]

namespace ReSharperPlugin.SpecflowRiderPlugin.SyntaxHighlighting
{
    public class GherkinHighlightingAttributeIds
    {
        // ReSharper disable InconsistentNaming
        public const string GroupID = "ReSharper SpecFlow Highlighters";
        public const string KEYWORD = "ReSharper SpecFlow Keyword";
        public const string TEXT = "ReSharper SpecFlow Text";
        public const string LINE_COMMENT = "ReSharper SpecFlow Line Comment";
        public const string TAG = "ReSharper SpecFlow Tag";
        public const string PYSTRING = "ReSharper SpecFlow Multiline String";
        public const string TABLE_CELL = "ReSharper SpecFlow Table Cell";
        public const string TABLE_HEADER_CELL = "ReSharper SpecFlow Table Header Cell";
        public const string PIPE = "ReSharper SpecFlow Table Pipe";
        public const string REGEXP_PARAMETER = "ReSharper SpecFlow Step Parameter";
        public const string OUTLINE_PARAMETER_SUBSTITUTION = "ReSharper SpecFlow Scenario Outline Parameter";
        // ReSharper restore InconsistentNaming
    }

    public class SpecFlowHighlighterNamesProvider : PrefixBasedSettingsNamesProvider
    {
        public SpecFlowHighlighterNamesProvider()
            : base("ReSharper SpecFlow", "ReSharper.SpecFlow")
        {
        }
    }
}