using JetBrains.Application.Settings;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.EditorConfig;
using JetBrains.ReSharper.Psi.Format;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Formatting
{
    [SettingsKey(typeof(CodeFormattingSettingsKey), "Code formatting for Gherkin (Reqnroll)")]
    [EditorConfigKey("gherkin-dotnet")]
    public class GherkinFormatSettingsKey : FormatSettingsKeyBase
    {
        [SettingsEntry(1, "Indent size for example")]
        public int ExampleIndentSize;
        [SettingsEntry(0, "Indent size for feature")]
        public int FeatureIndentSize;
        [SettingsEntry(0, "Indent size for multi-line string arguments")]
        public int PyStringIndentSize;
        [SettingsEntry(1, "Indent size for scenario")]
        public int ScenarioIndentSize;
        [SettingsEntry(1, "Indent size for step")]
        public int StepIndentSize;
        [SettingsEntry(0, "Indent size for 'And' step")]
        public int AndStepIndentSize;
        [SettingsEntry(0, "Indent size for table")]
        public int TableIndentSize;
        [SettingsEntry(true, "Indent size for table")]
        public bool SmallTableIndent;
        [SettingsEntry(1, "Blank lines before examples")]
        public int BlankLinesBeforeExamples;
        [SettingsEntry(1, "Blank lines before scenario")]
        public int BlankLinesBeforeScenario;
        [SettingsEntry(WrapStyle.CHOP_ALWAYS, "New line between tags")]
        public bool WrapTagsOnDifferentLines;
    }
}