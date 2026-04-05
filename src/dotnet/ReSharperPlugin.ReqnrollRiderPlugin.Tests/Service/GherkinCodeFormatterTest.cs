using JetBrains.ReSharper.FeaturesTestFramework.Formatter;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using ReSharperPlugin.ReqnrollRiderPlugin.Formatting;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Tests.Service;

[TestFileExtension(GherkinProjectFileType.FEATURE_EXTENSION)]
[TestSettingsKey(typeof(GherkinFormatSettingsKey))]
public class GherkinCodeFormatterTest : CodeFormatterWithExplicitSettingsTestBase<GherkinLanguage>
{
    protected override string RelativeTestDataPath => "Formatting";

    [TestCase("BlankLines - BetweenScenario")]
    [TestCase("BlankLines - BetweenStepAndExamplesBlock")]
    [TestCase("Indent - Background")]
    [TestCase("Indent - Basics")]
    [TestCase("Indent - Configuration - AllFalse")]
    [TestCase("Indent - Configuration - AllTrue")]
    [TestCase("Indent - Configuration - BlankLinesBeforeExamples")]
    [TestCase("Indent - Configuration - BlankLinesBeforeScenario")]
    [TestCase("Indent - Configuration - IndentAndSteps")]
    [TestCase("Indent - Configuration - IndentDataTable")]
    [TestCase("Indent - Configuration - IndentDocString")]
    [TestCase("Indent - Configuration - IndentExamples")]
    [TestCase("Indent - Configuration - IndentExamplesTable")]
    [TestCase("Indent - Configuration - IndentFeatureChildren")]
    [TestCase("Indent - Configuration - IndentRuleChildren")]
    [TestCase("Indent - Configuration - IndentSteps")]
    [TestCase("Indent - MultipleScenario")]
    [TestCase("Indent - Rule")]
    [TestCase("Table - Configuration - TableCellRightAlignNumericContent")]
    [TestCase("Table - Table 1")]
    [TestCase("Table - Table With Blank Line")]
    public void TestFormat(string name) { DoOneTest(name); }
}
