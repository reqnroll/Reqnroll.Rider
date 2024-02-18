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
using ReSharperPlugin.ReqnrollRiderPlugin.Helpers;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions
{
    [PsiComponent]
    // FIXME: Save this cache, see SolutionCaches and SimpleCache and implements ICacheWithVersion
    public class AssemblyStepDefinitionCache : IAssemblyCache
    {
        private readonly IPsiAssemblyFileLoader _psiAssemblyFileLoader;
        private readonly ReqnrollAssemblyStepsDefinitionMergeData _mergeData = new();
        private readonly IReqnrollStepInfoFactory _reqnrollStepInfoFactory;
        private readonly ScopeAttributeUtil _scopeAttributeUtil;

        // FIXME: per step kind
        public OneToSetMap<IPsiAssembly, ReqnrollStepInfo> AllStepsPerAssembly => _mergeData.StepsDefinitionsPerFiles;

        public AssemblyStepDefinitionCache(
            IPsiAssemblyFileLoader psiAssemblyFileLoader,
            IReqnrollStepInfoFactory reqnrollStepInfoFactory,
            ScopeAttributeUtil scopeAttributeUtil
        )
        {
            _psiAssemblyFileLoader = psiAssemblyFileLoader;
            _reqnrollStepInfoFactory = reqnrollStepInfoFactory;
            _scopeAttributeUtil = scopeAttributeUtil;
        }

        public IEnumerable<ReqnrollStepInfo> GetStepAccessibleForModule(IPsiModule module, GherkinStepKind stepKind)
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
            ReqnrollStepsDefinitionsCacheEntries stepDefinitions = null;
            _psiAssemblyFileLoader.GetOrLoadAssembly(assembly, true, (_, _, metadataAssembly) =>
            {
                stepDefinitions = new ReqnrollStepsDefinitionsCacheEntries();

                foreach (var type in metadataAssembly.GetTypes())
                {
                    if (type.CustomAttributesTypeNames.All(a => !ReqnrollAttributeHelper.IsBindingAttribute(a.FullName.GetText())))
                        continue;

                    var classScopes = _scopeAttributeUtil.GetScopesFromAttributes(type.CustomAttributes);
                    var classCacheEntry = new ReqnrollStepDefinitionCacheClassEntry(type.FullyQualifiedName, true, classScopes);

                    foreach (var method in type.GetMethods().Where(x => x.IsPublic))
                    {
                        // FIXME: We should avoid adding method that are not step here (it's just using more memory)
                        var methodScopes = _scopeAttributeUtil.GetScopesFromAttributes(method.CustomAttributes);
                        var methodParameterTypes = new string[method.Parameters.Length];
                        var methodParameterNames = new string[method.Parameters.Length];
                        for (var i = 0; i < method.Parameters.Length; i++)
                        {
                            var parameterDeclaration = method.Parameters[i];
                            methodParameterTypes[i] = parameterDeclaration.Type.FullName;
                            methodParameterNames[i] = parameterDeclaration.Name;
                        }
                        var methodCacheEntry = classCacheEntry.AddMethod(method.Name, methodParameterTypes, methodParameterNames, methodScopes);

                        for (var index = 0; index < method.CustomAttributes.Length; index++)
                        {
                            var attributeInstance = method.CustomAttributes[index];
                            if (attributeInstance.ConstructorArguments.Length == 0)
                                continue;

                            if (attributeInstance.ConstructorArguments[0].Value is not string regex)
                                continue;

                            var attributeTypeName = method.CustomAttributesTypeNames[index].FullName.ToString();
                            if (ReqnrollAttributeHelper.IsAttributeForKind(GherkinStepKind.Given, attributeTypeName))
                                methodCacheEntry.AddStep(GherkinStepKind.Given, regex);
                            if (ReqnrollAttributeHelper.IsAttributeForKind(GherkinStepKind.When, attributeTypeName))
                                methodCacheEntry.AddStep(GherkinStepKind.When, regex);
                            if (ReqnrollAttributeHelper.IsAttributeForKind(GherkinStepKind.Then, attributeTypeName))
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
            AddToLocalCache(assembly, builtPart as ReqnrollStepsDefinitionsCacheEntries);
        }

        public void Drop(IEnumerable<IPsiAssembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RemoveFromLocalCache(assembly);
            }
        }

        private void AddToLocalCache(IPsiAssembly assembly, [CanBeNull] ReqnrollStepsDefinitionsCacheEntries cacheItems)
        {
            if (cacheItems == null)
                return;

            foreach (var classEntry in cacheItems)
            {
                if (classEntry.HasReqnrollBindingAttribute)
                    _mergeData.ReqnrollBindingTypes.Add(classEntry.ClassName, assembly);
                else
                    _mergeData.PotentialReqnrollBindingTypes.Add(classEntry.ClassName, assembly);
                foreach (var method in classEntry.Methods)
                foreach (var step in method.Steps)
                    _mergeData.StepsDefinitionsPerFiles.Add(assembly, _reqnrollStepInfoFactory.Create(classEntry.ClassName, method.MethodName, method.MethodParameterTypes, method.MethodParameterNames, step.StepKind, step.Pattern, classEntry.Scopes, method.Scopes));
            }
        }

        private void RemoveFromLocalCache(IPsiAssembly assembly)
        {
            var fileSteps = _mergeData.StepsDefinitionsPerFiles[assembly];
            foreach (var classNameInFile in fileSteps.Select(x => x.ClassFullName).Distinct())
            {
                _mergeData.ReqnrollBindingTypes.Remove(classNameInFile);
                _mergeData.PotentialReqnrollBindingTypes.Remove(classNameInFile);
            }

            _mergeData.StepsDefinitionsPerFiles.RemoveKey(assembly);
        }
    }
}