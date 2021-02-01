using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Threading;
using JetBrains.Collections;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Helpers;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    [PsiComponent]
    public class SpecflowStepsDefinitionsCache : SimpleICache<SpecflowStepsDefinitionsCacheEntries>
    {
        private const int VersionInt = 6;
        public override string Version => VersionInt.ToString();

        // FIXME: per step kind
        public OneToSetMap<IPsiSourceFile, SpecflowStepInfo> AllStepsPerFiles => _mergeData.StepsDefinitionsPerFiles;
        public OneToSetMap<string, IPsiSourceFile> AllBindingTypes => _mergeData.SpecflowBindingTypes;
        private readonly SpecflowStepsDefinitionMergeData _mergeData = new SpecflowStepsDefinitionMergeData();

        public SpecflowStepsDefinitionsCache(Lifetime lifetime, IShellLocks locks, IPersistentIndexManager persistentIndexManager)
            : base(lifetime, locks, persistentIndexManager, new SpecflowStepDefinitionsEntriesMarshaller(), VersionInt)
        {
        }

        public override object Build(IPsiSourceFile sourceFile, bool isStartup)
        {
            var file = sourceFile.GetPrimaryPsiFile().NotNull();
            if (!file.Language.Is<CSharpLanguage>())
                return null;

            var stepDefinitions = new SpecflowStepsDefinitionsCacheEntries();
            foreach (var type in GetTypeDeclarations(file))
            {
                if (!(type is IClassDeclaration classDeclaration))
                    continue;
                if (HasSpecflowBindingAttribute(classDeclaration))
                    continue;
                stepDefinitions.Add(BuildBindingClassCacheEntry(classDeclaration));
            }

            return stepDefinitions;
        }

        private static bool HasSpecflowBindingAttribute(IClassDeclaration classDeclaration)
        {
            if (classDeclaration.IsPartial)
            {
                foreach (var firstClassReference in classDeclaration.GetFirstClassReferences())
                {
                    var resolve = firstClassReference.Resolve();
                    if (resolve.ResolveErrorType == ResolveErrorType.OK)
                    {
                        if (resolve.DeclaredElement is IClass @class)
                        {
                            if (@class.GetAttributeInstances(true).Any(x => x.GetAttributeType().GetClrName().FullName == "TechTalk.SpecFlow.BindingAttribute"))
                                return true;
                        }
                    }
                }
            }

            if (classDeclaration.Attributes.Count == 0)
                return false;

            // Optimization: do not resolve all attribute. We are looking for `TechTalk.SpecFlow.BindingAttribute` only
            var potentialBindingAttributes = classDeclaration.Attributes.Where(x => x.Arguments.Count == 0).ToList();
            if (potentialBindingAttributes.Count == 0)
                return true;

            var bindingAttributeFound = false;
            foreach (var potentialBindingAttribute in potentialBindingAttributes.Select(x => x.GetAttributeInstance()))
            {
                if (potentialBindingAttribute.GetClrName().FullName == "TechTalk.SpecFlow.BindingAttribute")
                    bindingAttributeFound = true;
            }
            return bindingAttributeFound;
        }

        public override void MergeLoaded(object data)
        {
            base.MergeLoaded(data);
            PopulateLocalCache();
        }

        public override void Merge(IPsiSourceFile sourceFile, object builtPart)
        {
            RemoveFromLocalCache(sourceFile);
            AddToLocalCache(sourceFile, builtPart as SpecflowStepsDefinitionsCacheEntries);
            base.Merge(sourceFile, builtPart);
        }

        private void PopulateLocalCache()
        {
            foreach (var (psiSourceFile, cacheItem) in Map)
                AddToLocalCache(psiSourceFile, cacheItem);
        }

        private void AddToLocalCache(IPsiSourceFile sourceFile, [CanBeNull] SpecflowStepsDefinitionsCacheEntries cacheItems)
        {
            if (cacheItems == null)
                return;

            foreach (var classEntry in cacheItems)
            {
                _mergeData.SpecflowBindingTypes.Add(classEntry.ClassName, sourceFile);
                foreach (var method in classEntry.Methods)
                foreach (var step in method.Steps)
                    _mergeData.StepsDefinitionsPerFiles.Add(sourceFile, new SpecflowStepInfo(classEntry.ClassName, method.MethodName, step.StepKind, step.Pattern));
            }
        }

        private void RemoveFromLocalCache(IPsiSourceFile sourceFile)
        {
            var fileSteps = _mergeData.StepsDefinitionsPerFiles[sourceFile];
            foreach (var classNameInFile in fileSteps.Select(x => x.ClassFullName).Distinct())
                _mergeData.SpecflowBindingTypes.Remove(classNameInFile, sourceFile);

            _mergeData.StepsDefinitionsPerFiles.RemoveKey(sourceFile);
        }

        private IEnumerable<ITypeDeclaration> GetTypeDeclarations(ITreeNode node)
        {
            if (node is INamespaceDeclarationHolder namespaceDeclarationHolder)
                foreach (var typeDeclaration in namespaceDeclarationHolder.NamespaceDeclarations.SelectMany(GetTypeDeclarations))
                    yield return typeDeclaration;

            if (node is ITypeDeclarationHolder typeDeclarationHolder)
                foreach (var typeDeclaration in typeDeclarationHolder.TypeDeclarations)
                    yield return typeDeclaration;
        }

        private SpecflowStepDefinitionCacheClassEntry BuildBindingClassCacheEntry(IClassDeclaration classDeclaration)
        {
            var classCacheEntry = new SpecflowStepDefinitionCacheClassEntry(classDeclaration.CLRName);

            foreach (var member in classDeclaration.MemberDeclarations)
            {
                if (!(member is IMethodDeclaration methodDeclaration))
                    continue;

                var methodCacheEntry = classCacheEntry.AddMethod(methodDeclaration.DeclaredName);

                foreach (var attributeInstance in methodDeclaration.Attributes.Select(x => x.GetAttributeInstance()))
                {
                    if (attributeInstance.PositionParameterCount == 0)
                        continue;

                    var parameter = attributeInstance.PositionParameter(0);
                    var regex = parameter.ConstantValue.Value as string;
                    if (regex == null)
                        continue;

                    if (attributeInstance.GetAttributeType().GetClrName().Equals(SpecflowAttributeHelper.GivenAttribute))
                        methodCacheEntry.AddStep(GherkinStepKind.Given, regex);
                    if (attributeInstance.GetAttributeType().GetClrName().Equals(SpecflowAttributeHelper.WhenAttribute))
                        methodCacheEntry.AddStep(GherkinStepKind.When, regex);
                    if (attributeInstance.GetAttributeType().GetClrName().Equals(SpecflowAttributeHelper.ThenAttribute))
                        methodCacheEntry.AddStep(GherkinStepKind.Then, regex);
                }
            }
            return classCacheEntry;
        }
    }
}