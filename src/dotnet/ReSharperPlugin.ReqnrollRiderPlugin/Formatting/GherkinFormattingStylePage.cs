using JetBrains.Annotations;
using JetBrains.Application.Components;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Components;
using JetBrains.Application.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.OptionPages;
using JetBrains.ReSharper.Feature.Services.OptionPages.CodeEditing;
using JetBrains.ReSharper.Feature.Services.OptionPages.CodeStyle;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.EditorConfig;
using JetBrains.ReSharper.Resources.Resources.Icons;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Formatting;

[OptionsPage(
    Pid,
    "Gherkin (Reqnroll) Formatting Style",
    typeof(PsiFeaturesUnsortedOptionsThemedIcons.Indent),
    ParentId = CodeEditingPage.PID,
    Sequence = 0,
    FilterTags = new[] {ConfigFileUtils.EditorConfigName})]
public class GherkinFormattingStylePage(
    Lifetime lifetime,
    [NotNull] OptionsSettingsSmartContext smartContext,
    [NotNull] IUIApplication environment,
    [NotNull] GherkinFormattingStylePageSchema schema,
    [NotNull] CodeStylePreview preview,
    IComponentContainer container)
    : CodeStylePage(lifetime, smartContext, environment, schema, preview, container)
{
    public const string Pid = "GherkinDotnetFormattingStylePage";

    public override bool ShowAutoDetectAndConfigureFormattingTip => true;

    public override string Id => Pid;

}

[FormattingSettingsPresentationComponent]
public class GherkinFormattingStylePageSchema(
    Lifetime lifetime,
    [NotNull] IContextBoundSettingsStoreLive smartContext,
    [NotNull] IValueEditorViewModelFactory itemViewModelFactory,
    IComponentContainer container,
    ISettingsToHide settingsToHide)
    : IndentStylePageSchema<GherkinFormatSettingsKey, GherkinCodeStylePreview>(lifetime, smartContext, itemViewModelFactory, container, settingsToHide)
{
    public override KnownLanguage Language => GherkinLanguage.Instance;
    public override string PageName => "Gherkin (Reqnroll) Formatting Style";

    protected override Pair<string, PreviewParseType> GetPreviewForIndents()
    {
        return Pair.Of(@"
Feature: Score Calculation (alternative forms)
  In order to know my performance
  As a player
  I want the system to calculate my total score


Scenario: One single spare
  Given a new bowling game
  When I roll the following series:	3,7,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1
  Then my total score should be 29

Scenario: All spares
  Given a new bowling game
  When I roll 10 times 1 and 9
  And I roll 1
  Then my total score should be 110
", PreviewParseType.File);
    }

    protected override void Describe(SchemaBuilder builder)
    {
        base.Describe(builder);

        var tagsExample = """
                          Feature: Calculator
                              @ignore @myTag1 @myTag2
                              Scenario: Add two numbers
                          """;
        var indentationExample = @"
Feature: Cucumber stock keeping
Scenario: eat 5 out of 20
  Given there are 20 cucumbers
  And something else
  When I eat 5 cucumbers
  Then I should have 15 cucumbers

Scenario: PyString
  Given the following text
  """"""
  Some text
  """"""

Scenario Outline: eating
  Given there are <start> cucumbers
  When I eat <eat> cucumbers
  Then I should have <left> cucumbers

  Examples:
    | start | eat | left |
    |    12 |   5 |    7 |
    |    20 |   5 |   15 |
";
        builder
            .Category("Indentation rules")
            .ItemFor(key => key.ExampleIndentSize, indentationExample)
            .ItemFor(key => key.PyStringIndentSize, indentationExample)
            .ItemFor(key => key.ScenarioIndentSize, indentationExample)
            .ItemFor(key => key.StepIndentSize, indentationExample)
            .ItemFor(key => key.AndStepIndentSize, indentationExample)
            .ItemFor(key => key.TableIndentSize, indentationExample)
            .ItemFor(key => key.SmallTableIndent, indentationExample)
            .EndCategory();
        builder
            .Category("Blank lines rules")
            .ItemFor(key => key.BlankLinesBeforeExamples, indentationExample)
            .ItemFor(key => key.BlankLinesBeforeScenario, indentationExample)
            .EndCategory();
        builder
            .Category("New Lines")
            .ItemFor(key => key.WrapTagsOnDifferentLines, tagsExample)
            .EndCategory();

    }
}