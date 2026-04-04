using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Calculated.Interface;
using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Format;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Formatting;

[Language(typeof(GherkinLanguage))]
public class GherkinFormatterInfoProvider(ISettingsSchema settingsSchema, ICalculatedSettingsSchema calculatedSettingsSchema, IThreading threading, Lifetime lifetime)
    : FormatterInfoProviderWithFluentApi<CodeFormattingContext, GherkinFormatSettingsKey>(settingsSchema, calculatedSettingsSchema, threading, lifetime)
{
    private static readonly NodeTypeSet Comments = new(GherkinNodeTypes.COMMENT);

    protected override void Initialize()
    {
        base.Initialize();

        Indenting();
        Formatting();
    }

    public override ProjectFileType MainProjectFileType => GherkinProjectFileType.Instance;

    private void Indenting()
    {
        Describe<IndentingRule>()
            .Name("FeatureScenario")
            .Where(
                Parent().HasType(GherkinNodeTypes.FEATURE),
                Node().HasType(GherkinNodeTypes.SCENARIO).Or().HasType(GherkinNodeTypes.SCENARIO_OUTLINE).Or().HasType(GherkinNodeTypes.RULE).OptionallyPreceededBy(Comments))
            .Switch(s => s.IndentFeatureChildren, IndentIfTrue(this))
            .Build();

        Describe<IndentingRule>()
            .Name("RuleScenario")
            .Where(
                Parent().HasType(GherkinNodeTypes.RULE),
                Node().HasType(GherkinNodeTypes.SCENARIO).Or().HasType(GherkinNodeTypes.SCENARIO_OUTLINE).OptionallyPreceededBy(Comments))
            .Switch(s => s.IndentRuleChildren, IndentIfTrue(this))
            .Build();

        Describe<IndentingRule>()
            .Name("ScenarioStep")
            .Where(
                Parent().HasType(GherkinNodeTypes.SCENARIO).Or().HasType(GherkinNodeTypes.SCENARIO_OUTLINE),
                Node().HasType(GherkinNodeTypes.STEP).OptionallyPreceededBy(Comments))
            .Switch(s => s.IndentSteps, IndentIfTrue(this))
            .Build();

        Describe<IndentingRule>()
            .Name("AndIndent")
            .Where(
                Left().HasType(GherkinNodeTypes.STEP).Satisfies2((node, _) => node.Node is GherkinStep {StepKind: GherkinStepKind.And})
                    .OptionallyPreceededBy(Comments)
            )
            .Switch(s => s.IndentAndSteps, IndentIfTrue(this))
            .Build();

        Describe<IndentingRule>()
            .Name("StepPyString")
            .Where(
                Parent().HasType(GherkinNodeTypes.STEP),
                Node().HasType(GherkinNodeTypes.PYSTRING))
            .Switch(s => s.IndentDocString, IndentIfTrue(this))
            .Build();

        Describe<IndentingRule>()
            .Name("ScenarioOutlineExampleBlob")
            .Where(
                Parent().HasType(GherkinNodeTypes.SCENARIO_OUTLINE),
                Node().HasType(GherkinNodeTypes.EXAMPLES_BLOCK).OptionallyPreceededBy(Comments))
            .Switch(s => s.IndentExamples, IndentIfTrue(this))
            .Build();

        Describe<IndentingRule>()
            .Name("DataTableIndent")
            .Where(
                Parent().HasType(GherkinNodeTypes.STEP),
                Node().HasType(GherkinNodeTypes.TABLE))
            .Switch(s => s.IndentDataTable, IndentIfTrue(this))
            .Build();

        Describe<IndentingRule>()
            .Name("ExamplesTableIndent")
            .Where(
                Parent().HasType(GherkinNodeTypes.EXAMPLES_BLOCK),
                Node().HasType(GherkinNodeTypes.TABLE))
            .Switch(s => s.IndentExamplesTable, IndentIfTrue(this))
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

        Describe<FormattingRule>()
            .Name("WrapTagOnDifferentLines")
            .Where(
                Left().HasType(GherkinNodeTypes.TAG),
                Right().HasType(GherkinNodeTypes.TAG))
            .Switch(s => s.WrapTagsOnDifferentLines,
                When(true).Return(IntervalFormatType.InsertNewLine),
                When(false).Return(IntervalFormatType.Space)
            )
            .Build();
    }

    private static IBuildableBuilder<OptionTreeBlank>[] IndentIfTrue<TContext, TSettingsKey>(
        FormatterInfoProviderWithFluentApi<TContext, TSettingsKey> provider)
        where TContext : CodeFormattingContext
        where TSettingsKey : FormatSettingsKeyBase
    {
        return
        [
            provider.When(true).Return(IndentType.External, indent: 1),
            provider.When(false).Return(IndentType.External, indent: 0),
        ];
    }
}