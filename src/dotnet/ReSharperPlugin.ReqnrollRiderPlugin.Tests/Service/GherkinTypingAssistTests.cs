using JetBrains.ReSharper.FeaturesTestFramework.TypingAssist;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using ReSharperPlugin.ReqnrollRiderPlugin.Formatting;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Tests.Service;

[TestFileExtension(GherkinProjectFileType.FEATURE_EXTENSION)]
[TestSettingsKey(typeof(GherkinFormatSettingsKey))]
public class GherkinTypingAssistTests : TypingAssistTestBase
{
    protected override string RelativeTestDataPath => "features/service/typingAssist";

    [TestCase("Enter 01 - Feature")]
    [TestCase("Enter 02 - Scenario")]
    [TestCase("Enter 03 - ScenarioOutline")]
    [TestCase("Enter 04 - Step")]
    [TestCase("Enter 05 - Rule")]
    [TestCase("Enter 06 - Background")]
    public void TestTypingAssist(string name) { DoOneTest(name); }
}