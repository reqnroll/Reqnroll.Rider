using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Collections;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Impl.Reflection2;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Helpers;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions
{
    [PsiComponent]
    // FIXME: Save this cache, see SolutionCaches and SimpleCache and implements ICacheWithVersion
    public class AssemblyStepDefinitionCache : IAssemblyCache
    {
        private readonly IPsiAssemblyFileLoader _psiAssemblyFileLoader;
        private readonly SpecflowAssemblyStepsDefinitionMergeData _mergeData = new();
        private readonly ISpecflowStepInfoFactory _specflowStepInfoFactory;
        private readonly ScopeAttributeUtil _scopeAttributeUtil;

        // FIXME: per step kind
        public OneToSetMap<IPsiAssembly, SpecflowStepInfo> AllStepsPerAssembly => _mergeData.StepsDefinitionsPerFiles;

        public AssemblyStepDefinitionCache(
            IPsiAssemblyFileLoader psiAssemblyFileLoader,
            ISpecflowStepInfoFactory specflowStepInfoFactory,
            ScopeAttributeUtil scopeAttributeUtil
        )
        {
            _psiAssemblyFileLoader = psiAssemblyFileLoader;
            _specflowStepInfoFactory = specflowStepInfoFactory;
            _scopeAttributeUtil = scopeAttributeUtil;
        }

        public IEnumerable<SpecflowStepInfo> GetStepAccessibleForModule(IPsiModule module, GherkinStepKind stepKind)
        {
            foreach (var (stepSourceFile, stepDefinitions) in _mergeData.StepsDefinitionsPerFiles)
            {
                if (!ReferenceEquals(module, stepSourceFile.PsiModule) && !module.References(stepSourceFile.PsiModule))
                    continue;
                foreach (var stepDefinitionInfo in stepDefinitions.Where(s => s.StepKind == stepKind))
                    yield return stepDefinitionInfo;
            }
        }

        public object Load(IProgressIndicator progress, bool enablePersistence)
        {
            return null;
        }

        public void MergeLoaded(object data)
        {
        }

        public void Save(IProgressIndicator progress, bool enablePersistence)
        {
        }

        public object Build(IPsiAssembly assembly)
        {
            SpecflowStepsDefinitionsCacheEntries stepDefinitions = null;
            _psiAssemblyFileLoader.GetOrLoadAssembly(assembly, true, (_, _, metadataAssembly) =>
            {
                stepDefinitions = new SpecflowStepsDefinitionsCacheEntries();

                foreach (var type in metadataAssembly.GetTypes())
                {
                    if (type.CustomAttributesTypeNames.All(a => a.FullName.GetText() != SpecflowAttributeHelper.BindingAttribute.FullName))
                        continue;

                    var classScopes = _scopeAttributeUtil.GetScopesFromAttributes(type.CustomAttributes);
                    var classCacheEntry = new SpecflowStepDefinitionCacheClassEntry(type.FullyQualifiedName, true, classScopes);

                    foreach (var method in type.GetMethods().Where(x => x.IsPublic))
                    {
                        // FIXME: We should avoid adding method that are not step here (it's just using more memory)
                        var methodScopes = _scopeAttributeUtil.GetScopesFromAttributes(method.CustomAttributes);
                        var methodCacheEntry = classCacheEntry.AddMethod(method.Name, methodScopes);

                        for (var index = 0; index < method.CustomAttributes.Length; index++)
                        {
                            var attributeInstance = method.CustomAttributes[index];
                            if (attributeInstance.ConstructorArguments.Length == 0)
                                continue;

                            if (attributeInstance.ConstructorArguments[0].Value is not string regex)
                                continue;

                            var attributeTypeName = method.CustomAttributesTypeNames[index].FullName.ToString();
                            if (SpecflowAttributeHelper.IsAttributeForKind(GherkinStepKind.Given, attributeTypeName))
                                methodCacheEntry.AddStep(GherkinStepKind.Given, regex);
                            if (SpecflowAttributeHelper.IsAttributeForKind(GherkinStepKind.When, attributeTypeName))
                                methodCacheEntry.AddStep(GherkinStepKind.When, regex);
                            if (SpecflowAttributeHelper.IsAttributeForKind(GherkinStepKind.Then, attributeTypeName))
                                methodCacheEntry.AddStep(GherkinStepKind.Then, regex);
                        }
                    }
                    stepDefinitions.Add(classCacheEntry);
                }
            });
            return stepDefinitions;
        }

        public void Merge(IPsiAssembly assembly, object builtPart, Func<bool> checkForTermination)
        {
            RemoveFromLocalCache(assembly);
            AddToLocalCache(assembly, builtPart as SpecflowStepsDefinitionsCacheEntries);
        }

        public void Drop(IEnumerable<IPsiAssembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RemoveFromLocalCache(assembly);
            }
        }

        private void AddToLocalCache(IPsiAssembly assembly, [CanBeNull] SpecflowStepsDefinitionsCacheEntries cacheItems)
        {
            if (cacheItems == null)
                return;

            foreach (var classEntry in cacheItems)
            {
                if (classEntry.HasSpecflowBindingAttribute)
                    _mergeData.SpecflowBindingTypes.Add(classEntry.ClassName, assembly);
                else
                    _mergeData.PotentialSpecflowBindingTypes.Add(classEntry.ClassName, assembly);
                foreach (var method in classEntry.Methods)
                foreach (var step in method.Steps)
                    _mergeData.StepsDefinitionsPerFiles.Add(assembly, _specflowStepInfoFactory.Create(classEntry.ClassName, method.MethodName, step.StepKind, step.Pattern, classEntry.Scopes, method.Scopes));
            }
        }

        private void RemoveFromLocalCache(IPsiAssembly assembly)
        {
            var fileSteps = _mergeData.StepsDefinitionsPerFiles[assembly];
            foreach (var classNameInFile in fileSteps.Select(x => x.ClassFullName).Distinct())
            {
                _mergeData.SpecflowBindingTypes.Remove(classNameInFile);
                _mergeData.PotentialSpecflowBindingTypes.Remove(classNameInFile);
            }

            _mergeData.StepsDefinitionsPerFiles.RemoveKey(assembly);
        }
    }
}