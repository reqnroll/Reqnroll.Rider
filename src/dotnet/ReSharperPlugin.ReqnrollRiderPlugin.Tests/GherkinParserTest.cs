using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Tests
{
    [TestFileExtension(GherkinProjectFileType.FEATURE_EXTENSION)]
    public class GherkinParserTest : ParserTestBase<GherkinLanguage>
    {
        protected override string RelativeTestDataPath => "Parsing";
        
        [TestCase("Features")]
        [TestCase("Simple")]
        [TestCase("Pystring")]
        [TestCase("Background")]
        [TestCase("BackgroundAfterScenario")]
        [TestCase("ScenarioOutline")]
        [TestCase("ScenarioOutlineTable")]
        [TestCase("ScenarioOutlineParam")]
        [TestCase("ScenarioOutlineTableWithTags")]
        [TestCase("Rule")]
        [TestCase("MultilineArgs")]
        [TestCase("MultilineFeatureDescription")]
        [TestCase("MultilineScenarioName")]
        [TestCase("NoSteps")]
        [TestCase("NotAStep")]
        [TestCase("Ruby14051")]
        [TestCase("Ruby8793")]
        [TestCase("ScenarioWithExamples")]
        [TestCase("StepParam")]
        [TestCase("TagBeforeExamples")]
        [TestCase("WithoutFeatureKeyword")]
        [TestCase("ScenarioWithTags")]
        [TestCase("PystringBacktick")]
        [TestCase("PystringWithParams")]
        [TestCase("PystringWithTripleQuoteInText")]
        [TestCase("FeatureRu")]
        public void TestParser(string name) { DoOneTest(name); }
    }
}