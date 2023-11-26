using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Files;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsUsages;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.UnresolvedReferenceHighlight
{
    [DaemonStage(StagesBefore = new[] {typeof(GlobalFileStructureCollectorStage)}, StagesAfter = new[] {typeof(CollectUsagesStage)})]
    public class UnresolvedStepHighlightingDaemonStage : IDaemonStage
    {
        private readonly ResolveHighlighterRegistrar _registrar;
        private readonly SpecflowStepsDefinitionsCache _specflowStepsDefinitionsCache;
        private readonly SpecflowStepsUsagesCache _specflowStepsUsagesCache;

        public UnresolvedStepHighlightingDaemonStage(
            ResolveHighlighterRegistrar registrar,
            SpecflowStepsDefinitionsCache specflowStepsDefinitionsCache,
            SpecflowStepsUsagesCache specflowStepsUsagesCache
        )
        {
            _registrar = registrar;
            _specflowStepsDefinitionsCache = specflowStepsDefinitionsCache;
            _specflowStepsUsagesCache = specflowStepsUsagesCache;
        }

        public IEnumerable<IDaemonStageProcess> CreateProcess(
            IDaemonProcess process,
            IContextBoundSettingsStore settings,
            DaemonProcessKind processKind
        )
        {
            if (processKind != DaemonProcessKind.SOLUTION_ANALYSIS && processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
                return Enumerable.Empty<IDaemonStageProcess>();

            if (_specflowStepsDefinitionsCache.AllStepsPerFiles.ContainsKey(process.SourceFile))
                return _specflowStepsUsagesCache.StepUsages.SelectMany(x => x.Value.Keys).Distinct()
                    .Select(x => x.GetPsiFile<GherkinLanguage>(x.Document.GetDocumentRange()))
                    .Where(x => x != null && x.IsValid())
                    .Select(file => new UnresolvedStepsHighlightingDaemonStageProcess(process, (GherkinFile) file, _registrar));

            var gherkinFile = process.SourceFile.GetPsiFile<GherkinLanguage>(process.Document.GetDocumentRange());
            if (gherkinFile == null)
                return Enumerable.Empty<IDaemonStageProcess>();

            return new[] {new UnresolvedStepsHighlightingDaemonStageProcess(process, (GherkinFile) gherkinFile, _registrar)};
        }
    }
}