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
        [TestCase("Indent - Basics")]
        [TestCase("Indent - MultipleScenario")]
        [TestCase("Formatting - Table 1")]
        public void TestFormat(string name) { DoOneTest(name); }
    }
}