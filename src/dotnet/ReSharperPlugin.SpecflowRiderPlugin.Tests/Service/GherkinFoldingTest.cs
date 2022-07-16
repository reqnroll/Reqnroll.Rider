using System.Collections.Generic;
using JetBrains.Application.Settings;
using JetBrains.DataStructures;
using JetBrains.ReSharper.Daemon.CodeFolding;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Tests.Service
{
    [TestFileExtension(GherkinProjectFileType.FEATURE_EXTENSION)]
    public class GherkinFoldingTest : HighlightingTestBase
    {
        protected override PsiLanguageType CompilerIdsLanguage => GherkinLanguage.Instance;

        protected override string RelativeTestDataPath => "Folding";

        [TestCase("Folding - Scenario")]
        [TestCase("Folding - Scenario Outline with Examples")]
        [TestCase("Folding - Rule")]
        [TestCase("Folding - Background")]
        public void TestFolding(string name) { DoOneTest(name);}

        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile, IContextBoundSettingsStore settingsStore)
        {
            return highlighting is CodeFoldingHighlighting;
        }
    }
    
    
    [DaemonStage(StagesBefore = new[] {typeof (SmartResolverStage)})]
    public class TestCodeFoldingStage : IDaemonStage
    {
        public IEnumerable<IDaemonStageProcess> CreateProcess(
            IDaemonProcess process,
            IContextBoundSettingsStore settings,
            DaemonProcessKind processKind)
        {
            if (processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
                return ImmutableArray<IDaemonStageProcess>.Empty;
            return new[]
            {
                new CodeFoldingProcess(process, settings)
            };
        }
    }
}