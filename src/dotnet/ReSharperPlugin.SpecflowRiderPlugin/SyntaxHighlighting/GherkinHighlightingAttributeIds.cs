using JetBrains.ReSharper.Feature.Services.Daemon.Attributes.Idea;
using JetBrains.TextControl.DocumentMarkup;

namespace ReSharperPlugin.SpecflowRiderPlugin.SyntaxHighlighting
{
    [
        RegisterHighlighterGroup(GroupID, "SpecFlow", HighlighterGroupPriority.LANGUAGE_SETTINGS,
            RiderNamesProviderType = typeof(SpecFlowHighlighterNamesProvider)),
        RegisterHighlighter(KEYWORD,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = IdeaHighlightingAttributeIds.KEYWORD,
            Layer = HighlighterLayer.SYNTAX),
        RegisterHighlighter(TEXT,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = IdeaHighlightingAttributeIds.TEXT,
            Layer = HighlighterLayer.SYNTAX),
        RegisterHighlighter(LINE_COMMENT,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = IdeaHighlightingAttributeIds.DOC_COMMENT,
            Layer = HighlighterLayer.SYNTAX),
        RegisterHighlighter(TAG,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = IdeaHighlightingAttributeIds.METADATA,
            Layer = HighlighterLayer.SYNTAX),
        RegisterHighlighter(REGEXP_PARAMETER,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = IdeaHighlightingAttributeIds.STRING,
            Layer = HighlighterLayer.SYNTAX),
        RegisterHighlighter(TABLE_CELL,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = REGEXP_PARAMETER,
            Layer = HighlighterLayer.SYNTAX),
        RegisterHighlighter(OUTLINE_PARAMETER_SUBSTITUTION,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = IdeaHighlightingAttributeIds.INSTANCE_FIELD,
            Layer = HighlighterLayer.SYNTAX),
        RegisterHighlighter(TABLE_HEADER_CELL,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = OUTLINE_PARAMETER_SUBSTITUTION,
            Layer = HighlighterLayer.SYNTAX),
        RegisterHighlighter(PIPE,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = KEYWORD,
            Layer = HighlighterLayer.SYNTAX),
        RegisterHighlighter(PYSTRING,
            GroupId = GroupID,
            EffectType = EffectType.TEXT,
            FallbackAttributeId = IdeaHighlightingAttributeIds.STRING,
            Layer = HighlighterLayer.SYNTAX)
    ]
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