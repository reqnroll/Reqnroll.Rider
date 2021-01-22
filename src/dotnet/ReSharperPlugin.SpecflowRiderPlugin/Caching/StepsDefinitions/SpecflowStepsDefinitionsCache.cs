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
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Helpers;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    [PsiComponent]
    public class SpecflowStepsDefinitionsCache : SimpleICache<SpecflowStepsDefinitionsCacheEntries>
    {
        public const int VersionInt = 4;
        public override string Version => VersionInt.ToString();

        public OneToSetMap<IPsiSourceFile, SpecflowStepDefinitionCacheEntry> AllStepsPerFiles => _mergeData.StepsDefinitionsPerFiles;
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
            if (!(file is ICSharpFile cSharpFile))
                return null;
            if (!cSharpFile.Imports.Any(x => x.ImportedSymbolName.QualifiedName == "TechTalk.SpecFlow"))
                return null;

            var stepDefinitions = new SpecflowStepsDefinitionsCacheEntries();
            foreach (var type in GetTypeDeclarations(file))
            {
                if (!(type.DeclaredElement is IClass @class))
                    continue;

                foreach (var gherkinCache in ListSpecflowSteps(@class))
                    stepDefinitions.Add(gherkinCache);
            }

            return stepDefinitions;
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

            foreach (var stepDefinition in cacheItems)
            {
                _mergeData.StepsDefinitionsPerFiles.Add(sourceFile, stepDefinition);
            }
        }

        private void RemoveFromLocalCache(IPsiSourceFile sourceFile)
        {
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

        private IEnumerable<SpecflowStepDefinitionCacheEntry> ListSpecflowSteps(IClass @class)
        {
            foreach (var member in @class.GetMembers())
            {
                if (!(member is IMethod method))
                    continue;

                var attributeInstances = method.GetAttributeInstances(AttributesSource.All);
                foreach (var attributeInstance in attributeInstances)
                {
                    if (attributeInstance.PositionParameterCount == 0)
                        continue;

                    var parameter = attributeInstance.PositionParameter(0);
                    var regex = parameter.ConstantValue.Value as string;
                    if (regex == null)
                        continue;

                    if (attributeInstance.GetAttributeType().GetClrName().Equals(SpecflowAttributeHelper.GivenAttribute))
                        yield return new SpecflowStepDefinitionCacheEntry(regex, GherkinStepKind.Given, method.ShortName);
                    if (attributeInstance.GetAttributeType().GetClrName().Equals(SpecflowAttributeHelper.WhenAttribute))
                        yield return new SpecflowStepDefinitionCacheEntry(regex, GherkinStepKind.When, method.ShortName);
                    if (attributeInstance.GetAttributeType().GetClrName().Equals(SpecflowAttributeHelper.ThenAttribute))
                        yield return new SpecflowStepDefinitionCacheEntry(regex, GherkinStepKind.Then, method.ShortName);
                }
            }
        }
    }
}