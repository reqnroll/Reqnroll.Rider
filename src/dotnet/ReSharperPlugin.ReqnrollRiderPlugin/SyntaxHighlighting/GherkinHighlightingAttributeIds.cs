using JetBrains.ReSharper.Feature.Services.Daemon.Attributes.Idea;
using JetBrains.TextControl.DocumentMarkup;

namespace ReSharperPlugin.ReqnrollRiderPlugin.SyntaxHighlighting
{
    [
        RegisterHighlighterGroup(GroupID, "Reqnroll", HighlighterGroupPriority.LANGUAGE_SETTINGS,
            RiderNamesProviderType = typeof(ReqnrollHighlighterNamesProvider)),
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
        public const string GroupID = "ReSharper Reqnroll Highlighters";
        public const string KEYWORD = "ReSharper Reqnroll Keyword";
        public const string TEXT = "ReSharper Reqnroll Text";
        public const string LINE_COMMENT = "ReSharper Reqnroll Line Comment";
        public const string TAG = "ReSharper Reqnroll Tag";
        public const string PYSTRING = "ReSharper Reqnroll Multiline String";
        public const string TABLE_CELL = "ReSharper Reqnroll Table Cell";
        public const string TABLE_HEADER_CELL = "ReSharper Reqnroll Table Header Cell";
        public const string PIPE = "ReSharper Reqnroll Table Pipe";
        public const string REGEXP_PARAMETER = "ReSharper Reqnroll Step Parameter";
        public const string OUTLINE_PARAMETER_SUBSTITUTION = "ReSharper Reqnroll Scenario Outline Parameter";
        // ReSharper restore InconsistentNaming
    }

    public class ReqnrollHighlighterNamesProvider : PrefixBasedSettingsNamesProvider
    {
        public ReqnrollHighlighterNamesProvider()
            : base("ReSharper Reqnroll", "ReSharper.Reqnroll")
        {
        }
    }
}