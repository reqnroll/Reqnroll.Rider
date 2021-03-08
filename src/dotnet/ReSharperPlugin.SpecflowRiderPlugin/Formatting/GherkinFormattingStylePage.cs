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
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Formatting
{
    [OptionsPage(
        PID,
        "Gherkin (SpecFlow) Formatting Style",
        typeof(PsiFeaturesUnsortedOptionsThemedIcons.Indent),
        ParentId = CodeEditingPage.PID,
        Sequence = 0,
        FilterTags = new[] {ConfigFileUtils.EditorConfigName})]
    public class GherkinFormattingStylePage : CodeStylePage
    {
        public const string PID = "GherkinDotnetFormattingStylePage";

        public override bool ShowAutoDetectAndConfigureFormattingTip => true;

        public override string Id => PID;

        public GherkinFormattingStylePage(Lifetime lifetime,
                                            [NotNull] OptionsSettingsSmartContext smartContext,
                                            [NotNull] IUIApplication environment,
                                            [NotNull] GherkinFormattingStylePageSchema schema,
                                            [NotNull] CodeStylePreview preview, IComponentContainer container)
            : base(lifetime, smartContext, environment, schema, preview, container)
        {
        }
    }

    [FormattingSettingsPresentationComponent]
    public class GherkinFormattingStylePageSchema : IndentStylePageSchema<GherkinFormatSettingsKey, GherkinCodeStylePreview>
    {
        public GherkinFormattingStylePageSchema(Lifetime lifetime,
                                                  [NotNull] IContextBoundSettingsStoreLive smartContext,
                                                  [NotNull] IValueEditorViewModelFactory itemViewModelFactory,
                                                  IComponentContainer container, ISettingsToHide settingsToHide)
            : base(lifetime, smartContext, itemViewModelFactory, container, settingsToHide)
        {
        }

        protected override void Describe(SchemaBuilder builder)
        {
            base.Describe(builder);

        }

        public override KnownLanguage Language => GherkinLanguage.Instance;
        public override string PageName => "Gherkin (SpecFlow) Formatting Style";
    }
}