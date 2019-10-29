using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using NUnit.Framework;
using ReSharperPlugin.SpecflowRiderPlugin.Experiments;

namespace ReSharperPlugin.SpecflowRiderPlugin.Tests
{
  public class SampleHighlightingTest : CSharpHighlightingTestBase
  {
    protected override string RelativeTestDataPath => "CSharp";

    protected override bool HighlightingPredicate(
      IHighlighting highlighting,
      IPsiSourceFile sourceFile,
      IContextBoundSettingsStore settingsStore)
    {
      return highlighting is SampleHighlighting;
    }

    [Test] public void TestSampleTest() { DoNamedTest2(); }
  }
}