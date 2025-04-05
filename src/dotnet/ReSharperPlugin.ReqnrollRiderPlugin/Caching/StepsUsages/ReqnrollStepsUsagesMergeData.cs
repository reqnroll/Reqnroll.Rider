using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsUsages;

public class ReqnrollStepsUsagesMergeData
{
    public readonly IDictionary<GherkinStepKind, OneToSetMap<IPsiSourceFile, string>> StepsUsages = new Dictionary<GherkinStepKind, OneToSetMap<IPsiSourceFile, string>>
    {
        [GherkinStepKind.Given] = new OneToSetMap<IPsiSourceFile, string>(),
        [GherkinStepKind.When] = new OneToSetMap<IPsiSourceFile, string>(),
        [GherkinStepKind.Then] = new OneToSetMap<IPsiSourceFile, string>()
    };
}