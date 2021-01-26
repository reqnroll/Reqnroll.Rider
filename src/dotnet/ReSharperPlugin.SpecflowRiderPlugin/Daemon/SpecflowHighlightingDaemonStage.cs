using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon
{
    [DaemonStage(StagesBefore = new[] {typeof(GlobalFileStructureCollectorStage)}, StagesAfter = new[] {typeof(CollectUsagesStage)})]
    public class SpecflowHighlightingDaemonStage : IDaemonStage
    {
        private readonly ResolveHighlighterRegistrar _registrar;
        public SpecflowHighlightingDaemonStage(ResolveHighlighterRegistrar registrar)
        {
            _registrar = registrar;
        }

        public IEnumerable<IDaemonStageProcess> CreateProcess(
            IDaemonProcess process,
            IContextBoundSettingsStore settings,
            DaemonProcessKind processKind
        )
        {
            if (processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
                return Enumerable.Empty<IDaemonStageProcess>();

            process.SourceFile.GetPsiServices().Files.AssertAllDocumentAreCommitted();
            return process.SourceFile.GetPsiFiles<GherkinLanguage>()
                .SelectNotNull(file => new UnresolvedStepsHighlightingDaemonStageProcess(process, (GherkinFile) file, _registrar));
        }
    }
}