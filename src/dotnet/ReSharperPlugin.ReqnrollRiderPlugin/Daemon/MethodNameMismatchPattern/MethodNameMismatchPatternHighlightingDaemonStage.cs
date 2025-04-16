using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils.Steps;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.MethodNameMismatchPattern;

[DaemonStage(StagesBefore = [typeof(GlobalFileStructureCollectorStage)], StagesAfter = [typeof(CollectUsagesStage)])]
public class MethodNameMismatchPatternHighlightingDaemonStage : IDaemonStage
{
    private readonly ReqnrollStepsDefinitionsCache _reqnrollStepsDefinitionsCache;
    private readonly IStepDefinitionBuilder _stepDefinitionBuilder;

    public MethodNameMismatchPatternHighlightingDaemonStage(
        ResolveHighlighterRegistrar registrar,
        ReqnrollStepsDefinitionsCache reqnrollStepsDefinitionsCache,
        IStepDefinitionBuilder stepDefinitionBuilder
    )
    {
        _reqnrollStepsDefinitionsCache = reqnrollStepsDefinitionsCache;
        _stepDefinitionBuilder = stepDefinitionBuilder;
    }

    public IEnumerable<IDaemonStageProcess> CreateProcess(
        IDaemonProcess process,
        IContextBoundSettingsStore settings,
        DaemonProcessKind processKind
    )
    {
        if (processKind != DaemonProcessKind.SOLUTION_ANALYSIS && processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
            return [];

        if (!_reqnrollStepsDefinitionsCache.AllStepsPerFiles.ContainsKey(process.SourceFile))
            return [];

        return process.SourceFile.GetPsiFiles<CSharpLanguage>()
            .SelectNotNull(file => new MethodNameMismatchPatternHighlightingDaemonStageProcess(process, (ICSharpFile) file, _stepDefinitionBuilder));
    }
}