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

[DaemonStage(StagesBefore = [typeof(GlobalFileStructureCollectorStage)], StagesAfter = [typeof(CollectUsagesStage)])]
public class ExecutionFailedStepGutterIconDaemonStage(FailedStepCache failedStepCache) : IDaemonStage
{

    public IEnumerable<IDaemonStageProcess> CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings, DaemonProcessKind processKind)
    {
        if (processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
            return [];

        var gherkinFile = process.SourceFile.GetPsiFile<GherkinLanguage>(process.Document.GetDocumentRange());
        if (gherkinFile == null)
            return [];

        var daemonStageProcess = new ExecutionFailedStepGutterIconDaemonStageProcess(process, (GherkinFile) gherkinFile, failedStepCache);
        return [daemonStageProcess];
    }
}