using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Parts;
using JetBrains.Application.Threading;
using JetBrains.Collections;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Extensions;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsUsages;

[PsiComponent(Instantiation.DemandAnyThreadUnsafe)]
public class ReqnrollStepsUsagesCache(Lifetime lifetime, IShellLocks locks, IPersistentIndexManager persistentIndexManager, ILogger logger)
    : SimpleICache<ReqnrollStepsUsagesCacheEntries>(lifetime, locks, persistentIndexManager, new ReqnrollStepUsagesEntriesMarshaller(), VersionInt)
{
    public const int VersionInt = 4;
    public override string Version => VersionInt.ToString();

    public IDictionary<GherkinStepKind, OneToSetMap<IPsiSourceFile, string>> StepUsages => _mergeData.StepsUsages;

    private readonly ReqnrollStepsUsagesMergeData _mergeData = new ReqnrollStepsUsagesMergeData();

    public override object Build(IPsiSourceFile sourceFile, bool isStartup)
    {
        if (!sourceFile.IsValid())
            return null;
        var file = sourceFile.GetPrimaryPsiFile().NotNull();
        if (!file.Language.Is<GherkinLanguage>())
            return null;
        if (!(file is GherkinFile gherkinFile))
            return null;

        var stepUsages = new ReqnrollStepsUsagesCacheEntries();
        var steps = gherkinFile.GetChildrenInSubtrees<GherkinStep>();
        stepUsages.AddRange(steps.Select(step => new ReqnrollStepUsageCacheEntry(step.EffectiveStepKind, step.GetStepText())));
        return stepUsages;
    }

    protected override bool IsApplicable(IPsiSourceFile sf)
    {
        return sf.LanguageType.Is<GherkinProjectFileType>();
    }

    public override void MergeLoaded(object data)
    {
        base.MergeLoaded(data);
        PopulateLocalCache();
    }

    public override void Merge(IPsiSourceFile sourceFile, object builtPart)
    {
        RemoveFromLocalCache(sourceFile);
        AddToLocalCache(sourceFile, builtPart as ReqnrollStepsUsagesCacheEntries);
        base.Merge(sourceFile, builtPart);
    }

    private void PopulateLocalCache()
    {
        foreach (var (psiSourceFile, cacheItem) in Map)
            AddToLocalCache(psiSourceFile, cacheItem);
    }

    public override void Drop(IPsiSourceFile sourceFile)
    {
        RemoveFromLocalCache(sourceFile);
        base.Drop(sourceFile);
    }

    private void AddToLocalCache(IPsiSourceFile sourceFile, [CanBeNull] ReqnrollStepsUsagesCacheEntries cacheItems)
    {
        if (cacheItems == null)
            return;

        foreach (var stepUsages in cacheItems)
        {
            if (!_mergeData.StepsUsages.ContainsKey(stepUsages.StepKind))
            {
                logger.Error("Failed to determine the kind of step the step {0} in {1}.", stepUsages.StepText, sourceFile);
                continue;
            }

            _mergeData.StepsUsages[stepUsages.StepKind].Add(sourceFile, stepUsages.StepText);
        }
    }

    private void RemoveFromLocalCache(IPsiSourceFile sourceFile)
    {
        _mergeData.StepsUsages[GherkinStepKind.Given].RemoveKey(sourceFile);
        _mergeData.StepsUsages[GherkinStepKind.When].RemoveKey(sourceFile);
        _mergeData.StepsUsages[GherkinStepKind.Then].RemoveKey(sourceFile);
    }
}