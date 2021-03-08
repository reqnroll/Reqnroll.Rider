using JetBrains.Application.Settings;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.EditorConfig;
using JetBrains.ReSharper.Psi.Format;

namespace ReSharperPlugin.SpecflowRiderPlugin.Formatting
{
    [SettingsKey(typeof(CodeFormattingSettingsKey), "Code formatting for Gherkin (SpecFlow)")]
    [EditorConfigKey("gherkin-dotnet")]
    public class GherkinFormatSettingsKey : FormatSettingsKeyBase
    {
    }
}