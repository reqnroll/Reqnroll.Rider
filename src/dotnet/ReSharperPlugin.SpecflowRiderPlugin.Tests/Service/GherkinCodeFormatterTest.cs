using JetBrains.ReSharper.FeaturesTestFramework.Formatter;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using ReSharperPlugin.SpecflowRiderPlugin.Formatting;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Tests.Service
{
    [TestFileExtension(GherkinProjectFileType.FEATURE_EXTENSION)]
    [TestSettingsKey(typeof(GherkinFormatSettingsKey))]
    public class GherkinCodeFormatterTest : CodeFormatterWithExplicitSettingsTestBase<GherkinLanguage>
    {
        protected override string RelativeTestDataPath => "Formatting";

        [TestCase("BlankLines - BetweenScenario")]
        [TestCase("BlankLines - BetweenStepAndExamplesBlock")]
        [TestCase("Formatting - Table 1")]
        [TestCase("Indent - Background")]
        [TestCase("Indent - Basics")]
        [TestCase("Indent - Configuration - AllFalse")]
        [TestCase("Indent - Configuration - AndStepIndentSize")]
        [TestCase("Indent - Configuration - BlankLinesBeforeExamples")]
        [TestCase("Indent - Configuration - BlankLinesBeforeScenario")]
        [TestCase("Indent - Configuration - ExampleIndentSize")]
        [TestCase("Indent - Configuration - FeatureIndentSize")]
        [TestCase("Indent - Configuration - PyStringIndentSize")]
        [TestCase("Indent - Configuration - ScenarioIndentSize")]
        [TestCase("Indent - Configuration - ScenarioIndentSize")]
        [TestCase("Indent - Configuration - SmallTableIndent")]
        [TestCase("Indent - Configuration - StepIndentSize")]
        [TestCase("Indent - MultipleScenario")]
        [TestCase("Indent - Rule")]
        public void TestFormat(string name) { DoOneTest(name); }
    }
}