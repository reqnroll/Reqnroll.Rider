using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Files;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils.Steps;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.ParameterHighlighting;

[DaemonStage(StagesBefore = new[] {typeof(GlobalFileStructureCollectorStage)}, StagesAfter = new[] {typeof(CollectUsagesStage)})]
public class ParameterHighlightingDaemonStage : IDaemonStage
{
    public ParameterHighlightingDaemonStage(
        ResolveHighlighterRegistrar registrar,
        ReqnrollStepsDefinitionsCache reqnrollStepsDefinitionsCache,
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
        if (processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
            return Enumerable.Empty<IDaemonStageProcess>();

        var gherkinFile = process.SourceFile.GetPsiFile<GherkinLanguage>(process.Document.GetDocumentRange());
        if (gherkinFile == null)
            return Enumerable.Empty<IDaemonStageProcess>();

        return new[] {new ParameterHighlightingDaemonStageProcess(process, (GherkinFile) gherkinFile)};
    }
}