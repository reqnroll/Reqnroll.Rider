using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Files;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsUsages;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.UnresolvedReferenceHighlight;

[DaemonStage(StagesBefore = new[] {typeof(GlobalFileStructureCollectorStage)}, StagesAfter = new[] {typeof(CollectUsagesStage)})]
public class UnresolvedStepHighlightingDaemonStage(
    ResolveHighlighterRegistrar registrar,
    ReqnrollStepsDefinitionsCache reqnrollStepsDefinitionsCache,
    ReqnrollStepsUsagesCache reqnrollStepsUsagesCache)
    : IDaemonStage
{

    public IEnumerable<IDaemonStageProcess> CreateProcess(
        IDaemonProcess process,
        IContextBoundSettingsStore settings,
        DaemonProcessKind processKind
    )
    {
        if (processKind != DaemonProcessKind.SOLUTION_ANALYSIS && processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
            return Enumerable.Empty<IDaemonStageProcess>();

        if (reqnrollStepsDefinitionsCache.AllStepsPerFiles.ContainsKey(process.SourceFile))
            return reqnrollStepsUsagesCache.StepUsages.SelectMany(x => x.Value.Keys).Distinct()
                .Select(x => x.GetPsiFile<GherkinLanguage>(x.Document.GetDocumentRange()))
                .Where(x => x != null && x.IsValid())
                .Select(file => new UnresolvedStepsHighlightingDaemonStageProcess(process, (GherkinFile) file, registrar));

        var gherkinFile = process.SourceFile.GetPsiFile<GherkinLanguage>(process.Document.GetDocumentRange());
        if (gherkinFile == null)
            return Enumerable.Empty<IDaemonStageProcess>();

        return new[] {new UnresolvedStepsHighlightingDaemonStageProcess(process, (GherkinFile) gherkinFile, registrar)};
    }
}