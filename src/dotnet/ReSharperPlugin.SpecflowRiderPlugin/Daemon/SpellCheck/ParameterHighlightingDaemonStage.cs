/*using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Features.ReSpeller.SpellEngine;
using JetBrains.ReSharper.Psi.Files;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.SpellCheck
{
    [DaemonStage(StagesBefore = new[] {typeof(GlobalFileStructureCollectorStage)}, StagesAfter = new[] {typeof(CollectUsagesStage)})]
    public class SpellCheckDaemonStage : IDaemonStage
    {
        public IEnumerable<IDaemonStageProcess> CreateProcess(
            IDaemonProcess process,
            IContextBoundSettingsStore settings,
            DaemonProcessKind processKind
        )
        {
            if (processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
                return Enumerable.Empty<IDaemonStageProcess>();

            var gherkinFile = process.SourceFile.GetPsiFile<GherkinLanguage>(process.Document.GetDocumentRange());
            if (gherkinFile == null)
                return Enumerable.Empty<IDaemonStageProcess>();

            return new[] {new SpellCheckDaemonStageProcess(process.Solution.GetComponent<ISpellService>(), process, (GherkinFile) gherkinFile)};
        }
    }
}*/