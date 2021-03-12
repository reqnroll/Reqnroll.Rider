using System.Collections.Generic;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Calculated.Interface;
using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
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
            var indentBetweenNodeAndChild = new List<(string name, GherkinNodeType parent, GherkinNodeType child)>
            {
                ("FeatureScenario", GherkinNodeTypes.FEATURE, GherkinNodeTypes.SCENARIO),
                ("FeatureScenarioOutline", GherkinNodeTypes.FEATURE, GherkinNodeTypes.SCENARIO_OUTLINE),
                ("FeatureRule", GherkinNodeTypes.FEATURE, GherkinNodeTypes.RULE),
                ("ScenarioStep", GherkinNodeTypes.SCENARIO, GherkinNodeTypes.STEP),
                ("ScenarioOutlineStep", GherkinNodeTypes.SCENARIO_OUTLINE, GherkinNodeTypes.STEP),
                ("RuleScenario", GherkinNodeTypes.RULE, GherkinNodeTypes.SCENARIO),
                ("ScenarioOutlineExampleBlob", GherkinNodeTypes.SCENARIO_OUTLINE, GherkinNodeTypes.EXAMPLES_BLOCK),
            };

            foreach (var rule in indentBetweenNodeAndChild)
            {
                Describe<IndentingRule>()
                    .Name(rule.name + "Indent")
                    .Where(
                        Parent().HasType(rule.parent),
                        Node().HasType(rule.child))
                    .Return(IndentType.External)
                    .Build();
            }

            Describe<IndentingRule>()
                .Name("TableIndent")
                .Where(
                    Parent().HasType(GherkinNodeTypes.EXAMPLES_BLOCK).Or().HasType(GherkinNodeTypes.STEP),
                    Node().HasType(GherkinNodeTypes.TABLE))
                .Return(IndentType.External)
                .Calculate((o, context) => new IndentOptionValue(IndentType.External, 0, "  "))
                .Build();
        }

        private void Aligning()
        {
            Describe<IntAlignRule>()
                .Name("AlignTableCells")
                .Where(
                    Node().HasType(GherkinNodeTypes.TABLE_CELL))
                .Calculate((o, context) => new IntAlignOptionValue("pipe", 1))
                .Build();
        }

        private void Formatting()
        {
            Describe<BlankLinesRule>()
                .Name("LineBetweenScenarios")
                .Group(ExtendedLineBreakGroup)
                .Where(
                    Left().HasType(GherkinNodeTypes.SCENARIO).Or().HasType(GherkinNodeTypes.SCENARIO_OUTLINE),
                    Right().HasType(GherkinNodeTypes.SCENARIO).Or().HasType(GherkinNodeTypes.SCENARIO_OUTLINE))
                .Return(1, 1, 2, false)
                .Build();

            Describe<BlankLinesRule>()
                .Name("LineBetweenStepAndExamples")
                .Group(ExtendedLineBreakGroup)
                .Where(
                    Left().HasType(GherkinNodeTypes.STEP),
                    Right().HasType(GherkinNodeTypes.EXAMPLES_BLOCK))
                .Return(2, 1, 1, false)
                .Build();
        }
    }
}