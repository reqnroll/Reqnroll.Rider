using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Parts;
using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils.TestOutput;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.FailedStep
{

    [PsiComponent(Instantiation.DemandAnyThreadUnsafe)]
    public class FailedStepCache : SimpleICache<ISet<FailedStepCacheEntry>>
    {
        public override string Version => "5";

        public FailedStepCache(
            Lifetime lifetime,
            IShellLocks locks,
            IPersistentIndexManager persistentIndexManager,
            long? version = null
        ) : base(lifetime, locks, persistentIndexManager, new FailedStepMarshaller(), version)
        {
        }

        protected override bool IsApplicable(IPsiSourceFile sf)
        {
            return sf.LanguageType.Is<GherkinProjectFileType>();
        }

        public override object Build(IPsiSourceFile sourceFile, bool isStartup)
        {
            if (Map.ContainsKey(sourceFile))
                return Map[sourceFile];
            return null;
        }

        public ISet<FailedStepCacheEntry> GetFailedSteps(IPsiSourceFile sourceFile)
        {
            if (!Map.ContainsKey(sourceFile))
                return EmptySet<FailedStepCacheEntry>.Instance;

            return Map[sourceFile];
        }

        public bool RemoveFailedStep(IPsiSourceFile sourceFile, string featureText, string scenarioText)
        {
            if (!Map.ContainsKey(sourceFile))
                return false;

            var list = Map[sourceFile];
            var matchingElements = list.Where(x => x.FeatureText == featureText && x.ScenarioText == scenarioText).ToList();
            if (matchingElements.Count == 0)
                return false;

            list.RemoveRange(matchingElements);
            Map[sourceFile] = list;
            return true;
        }

        public bool AddFailedStep(IPsiSourceFile sourceFile, string featureText, string scenarioText, List<StepTestOutput> stepsOutputs)
        {
            if (!Map.ContainsKey(sourceFile))
                Map[sourceFile] = new HashSet<FailedStepCacheEntry>();

            var list = Map[sourceFile];

            var existingFailedStep = list.FirstOrDefault(x => x.FeatureText == featureText && x.ScenarioText == scenarioText);
            if (existingFailedStep != null)
            {
                if (existingFailedStep.StepsOutputs.SequenceEqual(stepsOutputs))
                    return false;
                existingFailedStep.StepsOutputs = stepsOutputs;

            }
            else
                list.Add(new FailedStepCacheEntry
                {
                    FeatureText = featureText,
                    ScenarioText = scenarioText,
                    StepsOutputs = stepsOutputs
                });

            Map[sourceFile] = list;
            return true;
        }
    }
}