using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Files;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.FailedStep;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.ExecutionFailedStep;

[DaemonStage(StagesBefore = new[] {typeof(GlobalFileStructureCollectorStage)}, StagesAfter = new[] {typeof(CollectUsagesStage)})]
public class ExecutionFailedStepGutterIconDaemonStage(FailedStepCache failedStepCache) : IDaemonStage
{

    public IEnumerable<IDaemonStageProcess> CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings, DaemonProcessKind processKind)
    {
        if (processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
            return Enumerable.Empty<IDaemonStageProcess>();

        var gherkinFile = process.SourceFile.GetPsiFile<GherkinLanguage>(process.Document.GetDocumentRange());
        if (gherkinFile == null)
            return Enumerable.Empty<IDaemonStageProcess>();

        var daemonStageProcess = new ExecutionFailedStepGutterIconDaemonStageProcess(process, (GherkinFile) gherkinFile, failedStepCache);
        return new[] {daemonStageProcess};
    }
}