using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Calculated.Interface;
using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.Format;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Formatting
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinFormatterInfoProvider : FormatterInfoProviderWithFluentApi<CodeFormattingContext, GherkinFormatSettingsKey>
    {
        public GherkinFormatterInfoProvider(ISettingsSchema settingsSchema, ICalculatedSettingsSchema calculatedSettingsSchema, IThreading threading, Lifetime lifetime)
            : base(settingsSchema, calculatedSettingsSchema, threading, lifetime)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            Indenting();
            Aligning();
            Formatting();
        }

        public override ProjectFileType MainProjectFileType => GherkinProjectFileType.Instance;

        private void Indenting()
        {
            Describe<IndentingRule>()
                .Name("FeatureScenario")
                .Where(
                    Parent().HasType(GherkinNodeTypes.FEATURE),
                    Node().HasType(GherkinNodeTypes.SCENARIO).Or().HasType(GherkinNodeTypes.SCENARIO_OUTLINE).Or().HasType(GherkinNodeTypes.RULE))
                .Switch(s => s.ScenarioIndentSize, ContinuousIndentOptions(this, IndentType.External))
                .Build();

            Describe<IndentingRule>()
                .Name("RuleScenario")
                .Where(
                    Parent().HasType(GherkinNodeTypes.RULE),
                    Node().HasType(GherkinNodeTypes.SCENARIO).Or().HasType(GherkinNodeTypes.SCENARIO_OUTLINE))
                .Switch(s => s.ScenarioIndentSize, ContinuousIndentOptions(this, IndentType.External))
                .Build();

            Describe<IndentingRule>()
                .Name("ScenarioStep")
                .Where(
                    Parent().HasType(GherkinNodeTypes.SCENARIO).Or().HasType(GherkinNodeTypes.SCENARIO_OUTLINE),
                    Node().HasType(GherkinNodeTypes.STEP))
                .Switch(s => s.StepIndentSize, ContinuousIndentOptions(this, IndentType.External))
                .Build();

            Describe<IndentingRule>()
                .Name("AndIndent")
                .Where(
                    Left().HasType(GherkinNodeTypes.STEP).Satisfies2((node, _) => node.Node is GherkinStep {StepKind: GherkinStepKind.And})
                )
                .Switch(s => s.AndStepIndentSize, ContinuousIndentOptions(this, IndentType.External))
                .Build();

            Describe<IndentingRule>()
                .Name("StepPyString")
                .Where(
                    Parent().HasType(GherkinNodeTypes.STEP),
                    Node().HasType(GherkinNodeTypes.PYSTRING))
                .Switch(s => s.PyStringIndentSize, ContinuousIndentOptions(this, IndentType.External))
                .Build();

            Describe<IndentingRule>()
                .Name("ScenarioOutlineExampleBlob")
                .Where(
                    Parent().HasType(GherkinNodeTypes.SCENARIO_OUTLINE),
                    Node().HasType(GherkinNodeTypes.EXAMPLES_BLOCK))
                .Switch(s => s.ExampleIndentSize, ContinuousIndentOptions(this, IndentType.External))
                .Build();

            Describe<IndentingRule>()
                .Name("TableIndent")
                .Where(
                    Parent().HasType(GherkinNodeTypes.EXAMPLES_BLOCK).Or().HasType(GherkinNodeTypes.STEP),
                    Node().HasType(GherkinNodeTypes.TABLE))
                .Switch(s => s.SmallTableIndent,
                    When(true).Calculate((_, _) => new IndentOptionValue(IndentType.External, 0, new Whitespace(0, 2))),
                    When(false).Switch(s => s.TableIndentSize, ContinuousIndentOptions(this, IndentType.External)))
                .Build();
        }


        private void Aligning()
        {
            Describe<IntAlignRule>()
                .Name("AlignTableCells")
                .Where(
                    Node().HasType(GherkinNodeTypes.TABLE_CELL))
                .Calculate((_, _) => new IntAlignOptionValue("pipe", 1))
                .Build();
        }

        private void Formatting()
        {
            Describe<BlankLinesRule>()
                .Name("LineBeforeScenario")
                .Group(ExtendedLineBreakGroup)
                .Where(
                    Right().HasType(GherkinNodeTypes.SCENARIO).Or().HasType(GherkinNodeTypes.SCENARIO_OUTLINE))
                .SwitchBlankLines(s => s.BlankLinesBeforeScenario, true, BlankLineLimitKind.LimitBothStrict)
                .Build();

            Describe<BlankLinesRule>()
                .Name("LineBetweenStepAndExamples")
                .Group(ExtendedLineBreakGroup)
                .Where(
                    Left().HasType(GherkinNodeTypes.STEP),
                    Right().HasType(GherkinNodeTypes.EXAMPLES_BLOCK))
                .SwitchBlankLines(s => s.BlankLinesBeforeExamples, true, BlankLineLimitKind.LimitBothStrict)
                .Build();
        }

        public static IBuildableBuilder<OptionTreeBlank>[] ContinuousIndentOptions<TContext, TSettingsKey>(
            FormatterInfoProviderWithFluentApi<TContext, TSettingsKey> provider,
            IndentType indentType = IndentType.AfterFirstToken)
            where TContext : CodeFormattingContext
            where TSettingsKey : FormatSettingsKeyBase
        {
            return new IBuildableBuilder<OptionTreeBlank>[]
            {
                provider.When(ContinuousLineIndent.None).Return(indentType, 0),
                provider.When(ContinuousLineIndent.Single).Return(indentType, 1),
                provider.When(ContinuousLineIndent.Double).Return(indentType, 2),
                provider.When(0).Return(indentType, 0),
                provider.When(1).Return(indentType, 1),
                provider.When(2).Return(indentType, 2),
                provider.When(3).Return(indentType, 3),
                provider.When(4).Return(indentType, 4),
                provider.When(5).Return(indentType, 5),
                provider.When(6).Return(indentType, 6),
                provider.When(7).Return(indentType, 7)
            };
        }

    }
}