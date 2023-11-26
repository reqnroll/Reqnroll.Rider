using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Files;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.Steps;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.SyntaxError
{
    [DaemonStage(StagesBefore = new[] {typeof(GlobalFileStructureCollectorStage)}, StagesAfter = new[] {typeof(CollectUsagesStage)})]
    public class SyntaxErrorDaemonStage : IDaemonStage
    {
        public SyntaxErrorDaemonStage(
            ResolveHighlighterRegistrar registrar,
            SpecflowStepsDefinitionsCache specflowStepsDefinitionsCache,
            IStepDefinitionBuilder stepDefinitionBuilder
        )
        {
        }

        public IEnumerable<IDaemonStageProcess> CreateProcess(
            IDaemonProcess process,
            IContextBoundSettingsStore settings,
            DaemonProcessKind processKind
        )
        {
            if (processKind != DaemonProcessKind.VISIBLE_DOCUMENT && processKind != DaemonProcessKind.SOLUTION_ANALYSIS)
                return Enumerable.Empty<IDaemonStageProcess>();

            var gherkinFile = process.SourceFile.GetPsiFile<GherkinLanguage>(process.Document.GetDocumentRange());
            if (gherkinFile == null)
                return Enumerable.Empty<IDaemonStageProcess>();

            return new[] {new SyntaxErrorDaemonStageProcess(process, (GherkinFile) gherkinFile)};
        }
    }
}