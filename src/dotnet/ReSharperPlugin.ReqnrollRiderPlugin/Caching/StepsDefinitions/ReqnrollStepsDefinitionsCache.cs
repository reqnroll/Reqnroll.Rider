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
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Impl.Types;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Helpers;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils.Steps;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions
{
    [PsiComponent(Instantiation.DemandAnyThreadUnsafe)]
    public class ReqnrollStepsDefinitionsCache : SimpleICache<ReqnrollStepsDefinitionsCacheEntries>
    {
        private const int VersionInt = 15;
        public override string Version => VersionInt.ToString();

        // FIXME: per step kind
        public OneToSetMap<IPsiSourceFile, ReqnrollStepInfo> AllStepsPerFiles => _mergeData.StepsDefinitionsPerFiles;
        private readonly ReqnrollStepsDefinitionMergeData _mergeData = new();
        private readonly IReqnrollStepInfoFactory _reqnrollStepInfoFactory;
        private readonly IUnderscoresMethodNameStepDefinitionUtil _underscoresMethodNameStepDefinitionUtil;
        private readonly ScopeAttributeUtil _scopeAttributeUtil;

        public ReqnrollStepsDefinitionsCache(
            Lifetime lifetime,
            IShellLocks locks,
            IPersistentIndexManager persistentIndexManager,
            IReqnrollStepInfoFactory reqnrollStepInfoFactory,
            IUnderscoresMethodNameStepDefinitionUtil underscoresMethodNameStepDefinitionUtil,
            ScopeAttributeUtil scopeAttributeUtil
        )
            : base(lifetime, locks, persistentIndexManager, new ReqnrollStepDefinitionsEntriesMarshaller(), VersionInt)
        {
            _reqnrollStepInfoFactory = reqnrollStepInfoFactory;
            _underscoresMethodNameStepDefinitionUtil = underscoresMethodNameStepDefinitionUtil;
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

        public IEnumerable<AvailableBindingClass> GetBindingTypes(IPsiModule module)
        {
            foreach (var (fullClassName, sourceFile) in _mergeData.ReqnrollBindingTypes.SelectMany(e => e.Value.Select(v => (fullClassName: e.Key, sourceFile: v))))
            {
                if (!module.Equals(sourceFile.PsiModule) && !module.References(sourceFile.PsiModule))
                    continue;
                yield return new AvailableBindingClass(sourceFile, fullClassName);
            }

            // Since we cannot know when cache are built if a partial class in a given file has an attribute or not.
            // This is due to the fact that `DeclaredElement` are resolved using cache and we cannot depends ond cache when building a cache.
            // The check need to be done at this time.
            foreach (var (fullClassName, sourceFile) in _mergeData.PotentialReqnrollBindingTypes.SelectMany(e => e.Value.Select(v => (fullClassName: e.Key, sourceFile: v))))
            {
                if (!module.Equals(sourceFile.PsiModule) && !module.References(sourceFile.PsiModule))
                    continue;
                var type = CSharpTypeFactory.CreateType(fullClassName, sourceFile.PsiModule);
                if (!(type is DeclaredTypeFromCLRName declaredTypeFromClrName))
                    continue;
                var resolveResult = declaredTypeFromClrName.Resolve();
                if (!resolveResult.IsValid())
                    continue;
                if (!(resolveResult.DeclaredElement is IClass @class))
                    continue;
                if (@class.GetAttributeInstances(AttributesSource.Self).All(x => x.GetAttributeType().GetClrName().FullName != "Reqnroll.BindingAttribute"))
                    continue;
                yield return new AvailableBindingClass(sourceFile, fullClassName);
            }
        }

        public override object Build(IPsiSourceFile sourceFile, bool isStartup)
        {
            if (!sourceFile.IsValid())
                return null;
            var file = sourceFile.GetPrimaryPsiFile().NotNull();
            if (!file.Language.Is<CSharpLanguage>())
                return null;

            var stepDefinitions = new ReqnrollStepsDefinitionsCacheEntries();
            foreach (var type in GetTypeDeclarations(file))
            {
                if (!(type is IClassDeclaration classDeclaration))
                    continue;
                var hasBindingAttribute = HasBindingAttribute(classDeclaration);
                if (!hasBindingAttribute && !classDeclaration.IsPartial)
                    continue;
                if (IsReqnrollFeatureFile(classDeclaration))
                    continue;
                stepDefinitions.Add(BuildBindingClassCacheEntry(classDeclaration, hasBindingAttribute));
            }

            if (stepDefinitions.Count == 0)
                return null;

            return stepDefinitions;
        }

        protected override bool IsApplicable(IPsiSourceFile sf)
        {
            return sf.LanguageType.Is<CSharpProjectFileType>();
        }

        private bool IsReqnrollFeatureFile(IClassDeclaration classDeclaration)
        {

            if (classDeclaration.Attributes.Count == 0)
                return false;

            // Optimization: do not resolve all attribute. We are looking for `System.CodeDom.Compiler.GeneratedCodeAttribute` only
            var potentialBindingAttributes = classDeclaration.Attributes.Where(x => x.Arguments.Count == 2).ToList();
            if (potentialBindingAttributes.Count == 0)
                return false;

            var reqnrollGeneratedAttribute = false;
            foreach (var potentialBindingAttribute in potentialBindingAttributes.Select(x => x.GetAttributeInstance()))
            {
                if (potentialBindingAttribute.GetClrName().FullName == "System.CodeDom.Compiler.GeneratedCodeAttribute"
                    && potentialBindingAttribute.PositionParameter(0).ConstantValue.StringValue == "Reqnroll")
                    reqnrollGeneratedAttribute = true;
            }
            return reqnrollGeneratedAttribute;
        }

        private static bool HasBindingAttribute(IClassDeclaration classDeclaration)
        {
            if (classDeclaration.Attributes.Count == 0)
                return false;

            // Optimization: do not resolve all attribute. We are looking for `Reqnroll.BindingAttribute` only
            var potentialBindingAttributes = classDeclaration.Attributes.Where(x => x.Arguments.Count == 0).ToList();
            if (potentialBindingAttributes.Count == 0)
                return false;

            var bindingAttributeFound = false;
            foreach (var potentialBindingAttribute in potentialBindingAttributes.Select(x => x.GetAttributeInstance()))
            {
                var fullName = potentialBindingAttribute.GetClrName().FullName;

                if (ReqnrollAttributeHelper.BindingAttribute.Any(x => x.FullName == fullName))
                {
                    bindingAttributeFound = true;
                    break;
                }

                if (fullName.IsEmpty() && potentialBindingAttribute.GetAttributeShortName() == "Binding")
                {
                    bindingAttributeFound = true;
                    break;
                }
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
            AddToLocalCache(sourceFile, builtPart as ReqnrollStepsDefinitionsCacheEntries);
            base.Merge(sourceFile, builtPart);
        }

        private void PopulateLocalCache()
        {
            foreach (var (psiSourceFile, cacheItem) in Map)
                AddToLocalCache(psiSourceFile, cacheItem);
        }

        private void AddToLocalCache(IPsiSourceFile sourceFile, [CanBeNull] ReqnrollStepsDefinitionsCacheEntries cacheItems)
        {
            if (cacheItems == null)
                return;

            foreach (var classEntry in cacheItems)
            {
                if (classEntry.HasReqnrollBindingAttribute)
                    _mergeData.ReqnrollBindingTypes.Add(classEntry.ClassName, sourceFile);
                else
                    _mergeData.PotentialReqnrollBindingTypes.Add(classEntry.ClassName, sourceFile);
                foreach (var method in classEntry.Methods)
                foreach (var step in method.Steps)
                    _mergeData.StepsDefinitionsPerFiles.Add(sourceFile, _reqnrollStepInfoFactory.Create(classEntry.ClassName, method.MethodName, method.MethodParameterTypes, method.MethodParameterNames, step.StepKind, step.Pattern, classEntry.Scopes, method.Scopes));
            }
        }

        private void RemoveFromLocalCache(IPsiSourceFile sourceFile)
        {
            var fileSteps = _mergeData.StepsDefinitionsPerFiles[sourceFile];
            foreach (var classNameInFile in fileSteps.Select(x => x.ClassFullName).Distinct())
            {
                _mergeData.ReqnrollBindingTypes.Remove(classNameInFile, sourceFile);
                _mergeData.PotentialReqnrollBindingTypes.Remove(classNameInFile, sourceFile);
            }

            _mergeData.StepsDefinitionsPerFiles.RemoveKey(sourceFile);
        }

        public override void Drop(IPsiSourceFile sourceFile)
        {
            RemoveFromLocalCache(sourceFile);
            base.Drop(sourceFile);
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

        private ReqnrollStepDefinitionCacheClassEntry BuildBindingClassCacheEntry(IClassDeclaration classDeclaration, bool hasReqnrollBindingAttribute)
        {
            var classScopes = _scopeAttributeUtil.GetScopes(classDeclaration);
            var classCacheEntry = new ReqnrollStepDefinitionCacheClassEntry(classDeclaration.CLRName, hasReqnrollBindingAttribute, classScopes);

            ReadStepsFromMethodsOfClass(classDeclaration, classCacheEntry);
            return classCacheEntry;
        }

        private void ReadStepsFromMethodsOfClass(IClassDeclaration classDeclaration, ReqnrollStepDefinitionCacheClassEntry classCacheEntry)
        {
            foreach (var methodDeclaration in classDeclaration.MethodDeclarations)
            {
                var methodParameterTypes = new string[methodDeclaration.ParameterDeclarations.Count];
                var methodParameterNames = new string[methodDeclaration.ParameterDeclarations.Count];
                for (var i = 0; i < methodDeclaration.ParameterDeclarations.Count; i++)
                {
                    var parameterDeclaration = methodDeclaration.ParameterDeclarations[i];
                    methodParameterNames[i] = parameterDeclaration.DeclaredName;
                    methodParameterTypes[i] = parameterDeclaration.TypeUsage?.GetText();
                }

                var methodScopes = _scopeAttributeUtil.GetScopes(methodDeclaration);
                var methodCacheEntry = classCacheEntry.AddMethod(methodDeclaration.DeclaredName, methodParameterTypes, methodParameterNames, methodScopes);

                foreach (var attribute in methodDeclaration.Attributes)
                {
                    if (attribute.Arguments.Count == 1)
                        AddToCacheEntryBasedOnAttributeRegex(attribute, methodCacheEntry);
                    if (attribute.Arguments.Count == 0)
                        AddToCacheEntryBasedOnMethodName(methodDeclaration, attribute, methodCacheEntry);
                }
            }

            using (CompilationContextCookie.GetOrCreate(classDeclaration.GetResolveContext()))
            {
                var baseClassType = classDeclaration.DeclaredElement?.GetBaseClassType()?.GetTypeElement()?.GetSingleDeclaration();
                if (baseClassType is IClassDeclaration baseClassDeclaration)
                    ReadStepsFromMethodsOfClass(baseClassDeclaration, classCacheEntry);
            }
        }

        private static void AddToCacheEntryBasedOnAttributeRegex(
            IAttribute attribute,
            ReqnrollStepDefinitionCacheMethodEntry methodCacheEntry
        )
        {

            var attributeArgument = attribute.Arguments[0];
            if (attributeArgument.Value?.ConstantValue?.IsString(out var regex) != true)
                return;

            // FIXME: If at some point this is not enough we could check that attribute.Name.QualifiedName contains Reqnroll or that
            // Reqnroll is in the `using` list somewhere in a parent

            if (ReqnrollAttributeHelper.IsAttributeForKindUsingShortName(GherkinStepKind.Given, attribute.Name.ShortName))
                methodCacheEntry.AddStep(GherkinStepKind.Given, regex);
            if (ReqnrollAttributeHelper.IsAttributeForKindUsingShortName(GherkinStepKind.When, attribute.Name.ShortName))
                methodCacheEntry.AddStep(GherkinStepKind.When, regex);
            if (ReqnrollAttributeHelper.IsAttributeForKindUsingShortName(GherkinStepKind.Then, attribute.Name.ShortName))
                methodCacheEntry.AddStep(GherkinStepKind.Then, regex);
        }

        private void AddToCacheEntryBasedOnMethodName(
            IMethodDeclaration memberDeclaration,
            IAttribute attribute,
            ReqnrollStepDefinitionCacheMethodEntry methodCacheEntry
        )
        {
            var regex = _underscoresMethodNameStepDefinitionUtil.BuildRegexFromMethodName(memberDeclaration);
            if (ReqnrollAttributeHelper.IsAttributeForKindUsingShortName(GherkinStepKind.Given, attribute.Name.ShortName))
                methodCacheEntry.AddStep(GherkinStepKind.Given, regex);
            if (ReqnrollAttributeHelper.IsAttributeForKindUsingShortName(GherkinStepKind.When, attribute.Name.ShortName))
                methodCacheEntry.AddStep(GherkinStepKind.When, regex);
            if (ReqnrollAttributeHelper.IsAttributeForKindUsingShortName(GherkinStepKind.Then, attribute.Name.ShortName))
                methodCacheEntry.AddStep(GherkinStepKind.Then, regex);
        }

        public class AvailableBindingClass
        {
            public IPsiSourceFile SourceFile { get; }
            public string ClassClrName { get; }

            public AvailableBindingClass(IPsiSourceFile sourceFile, string classClrName)
            {
                SourceFile = sourceFile;
                ClassClrName = classClrName;
            }
        }
    }
}