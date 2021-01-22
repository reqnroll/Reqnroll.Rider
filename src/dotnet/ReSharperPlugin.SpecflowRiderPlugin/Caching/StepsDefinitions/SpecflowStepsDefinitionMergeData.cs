using System;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    public class SpecflowStepsDefinitionMergeData
    {
        public readonly OneToSetMap<IPsiSourceFile, SpecflowStepDefinitionCacheEntry> StepsDefinitionsPerFiles = new OneToSetMap<IPsiSourceFile, SpecflowStepDefinitionCacheEntry>();
    }
}