using JetBrains.Application.Settings;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.EditorConfig;
using JetBrains.ReSharper.Psi.Format;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Formatting;

[SettingsKey(typeof(CodeFormattingSettingsKey), "Code formatting for Gherkin (Reqnroll)")]
[EditorConfigKey("gherkin")]
public class GherkinFormatSettingsKey : FormatSettingsKeyBase
{
    /// <summary>
    ///     Specifies whether child elements of Feature (Background, Rule, Scenario, Scenario Outline) should be indented.
    /// </summary>
    [EditorConfigEntryAlias("indent_feature_children", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(false, "Indent feature children")]
    public bool IndentFeatureChildren { get; set; }

    /// <summary>
    ///     Specifies whether child elements fo Rule (Background, Scenario, Scenario Outline) should be indented.
    /// </summary>
    [EditorConfigEntryAlias("indent_rule_children", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(false, "Indent rule children")]
    public bool IndentRuleChildren { get; set; }

    /// <summary>
    ///     Specifies whether steps of scenarios should be indented.
    /// </summary>
    [EditorConfigEntryAlias("indent_steps", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(true, "Indent steps")]
    public bool IndentSteps { get; set; } = true;

    /// <summary>
    ///     Specifies whether the 'And' and 'But' steps of the scenarios should have an additional indentation.
    /// </summary>
    [EditorConfigEntryAlias("indent_and_steps", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(false, "Indent 'And' steps")]
    public bool IndentAndSteps { get; set; }

    /// <summary>
    ///     Specifies whether DataTable arguments should be indented within the step.
    /// </summary>
    [EditorConfigEntryAlias("indent_datatable", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(true, "Indent data table")]
    public bool IndentDataTable { get; set; } = true;

    /// <summary>
    ///     Specifies whether DocString arguments should be indented within the step.
    /// </summary>
    [EditorConfigEntryAlias("indent_docstring", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(true, "Indent doc string")]
    public bool IndentDocString { get; set; } = true;

    /// <summary>
    ///     Specifies whether the Examples block should be indented within the Scenario Outline.
    /// </summary>
    [EditorConfigEntryAlias("indent_examples", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(false, "Indent examples")]
    public bool IndentExamples { get; set; }

    /// <summary>
    ///     Specifies whether the Examples table should be indented within the Examples block.
    /// </summary>
    [EditorConfigEntryAlias("indent_examples_table", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(true, "Indent examples table")]
    public bool IndentExamplesTable { get; set; } = true;

    /// <summary>
    ///     The number of space characters to be used on each sides as table cell padding.
    /// </summary>
    [EditorConfigEntryAlias("table_cell_padding_size", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(1, "Table cell padding size")]
    public int TableCellPaddingSize { get; set; } = 1;

    /// <summary>
    ///     The number of blank lines before the "Examples" clause
    /// </summary>
    [EditorConfigEntryAlias("blank_lines_before_examples", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(1, "Blank lines before examples")]
    public int BlankLinesBeforeExamples { get; set; } = 1;

    /// <summary>
    ///     The number of blank lines before the "Examples" clause
    /// </summary>
    [EditorConfigEntryAlias("blank_lines_before_scenario", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(1, "Blank lines before scenario")]
    public int BlankLinesBeforeScenario { get; set; } = 1;

    /// <summary>
    ///     Wrap tags on different lines
    /// </summary>
    [EditorConfigEntryAlias("wrap_tags_on_different_lines", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(true, "Wrap tags on different lines")]
    public bool WrapTagsOnDifferentLines { get; set; } = true;

    /// <summary>
    ///     Right-align numeric table cells
    /// </summary>
    [EditorConfigEntryAlias("table_cell_right_align_numeric_content", EditorConfigAliasType.LanguageSpecificStandard)]
    [SettingsEntry(true, "Table cell right align numeric content")]
    public bool TableCellRightAlignNumericContent { get; set; } = true;
}