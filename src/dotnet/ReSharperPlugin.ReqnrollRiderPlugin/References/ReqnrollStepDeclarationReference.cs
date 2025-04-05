using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.Collections;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.Impl.Reflection2;
using JetBrains.ReSharper.Psi.Impl.reflection2.elements.Compiled;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TestRunner.Abstractions.Extensions;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using IClassDeclaration = JetBrains.ReSharper.Psi.CSharp.Tree.IClassDeclaration;

namespace ReSharperPlugin.ReqnrollRiderPlugin.References
{
    public class ReqnrollStepDeclarationReference : TreeReferenceBase<GherkinStep>
    {
        public ReqnrollStepDeclarationReference([NotNull] GherkinStep owner) : base(owner)
        {
        }

        public Regex RegexPattern { get; private set; }
        public string ScenarioText { get; private set; }
        public string FeatureText { get; private set; }
        public IList<string> Tags { get; private set; }
        public string StepPattern { get; private set; }

        public override ResolveResultWithInfo ResolveWithoutCache()
        {
            var psiServices = myOwner.GetPsiServices();
            var reqnrollStepsDefinitionsCache = psiServices.GetComponent<ReqnrollStepsDefinitionsCache>();
            var stepKind = myOwner.EffectiveStepKind;
            var stepText = myOwner.GetStepText();
            var psiModule = myOwner.GetPsiModule();
            ScenarioText = myOwner.GetScenarioText();
            FeatureText = myOwner.GetFeatureText();
            Tags = myOwner.GetEffectiveTags().ToList();

            var containingScenario = myOwner.GetContainingNode<IGherkinScenario>();
            if (containingScenario is GherkinScenarioOutline scenarioOutline)
                stepText = myOwner.GetStepTextForExample(scenarioOutline.GetExampleData(0));

            foreach (var (sourceFile, cacheEntries) in reqnrollStepsDefinitionsCache.AllStepsPerFiles)
            {
                if (!psiModule.Equals(sourceFile.PsiModule) && !psiModule.References(sourceFile.PsiModule))
                    continue;

                foreach (var cacheEntry in cacheEntries.Where(c => c.StepKind == stepKind))
                {
                    if (!myOwner.MatchScope(cacheEntry.Scopes))
                        continue;
                    if (cacheEntry.Regex?.IsMatch(stepText) == true)
                    {
                        var types = psiServices.Symbols.GetTypesAndNamespacesInFile(sourceFile);
                        foreach (var decElement in types)
                        {
                            if (decElement is not IClass cl)
                                continue;
                            if (cl.GetClrName().FullName != cacheEntry.ClassFullName)
                                continue;

                            IDeclaredElement matchingMethod = null;
                            foreach (var method in GetAllMethodFromClassAndBaseClasses(cl))
                            {
                                if (method.ShortName != cacheEntry.MethodName)
                                    continue;
                                if (method.Parameters.Count != cacheEntry.MethodParameterTypes.Length)
                                    continue;
                                var allParameterTypesMatch = true;
                                for (var i = 0; i < method.Parameters.Count; i++)
                                {
                                    var methodParameter = method.Parameters[i];
                                    var expectedParameterName = cacheEntry.MethodParameterNames[i];
                                    if (methodParameter.ShortName != expectedParameterName)
                                    {
                                        allParameterTypesMatch = false;
                                        break;
                                    }
                                    var expectedTypeName = cacheEntry.MethodParameterTypes[i];
                                    if (expectedTypeName != null)
                                    {
                                        if (methodParameter.Type is IDeclaredType declarationType
                                            && !declarationType.GetClrName().FullName.EndsWith(expectedTypeName)
                                            && !declarationType.GetLongPresentableName(CSharpLanguage.Instance).SubstringBefore("<").EndsWith(expectedTypeName.SubstringBefore("<")))
                                        {
                                            allParameterTypesMatch = false;
                                            break;
                                        }
                                    }
                                }

                                if (allParameterTypesMatch)
                                    matchingMethod = method;
                            }
                            if (matchingMethod == null)
                                continue;

                            var symbolInfo = new SymbolInfo(matchingMethod);
                            var resolveResult = ResolveResultFactory.CreateResolveResult(symbolInfo.GetDeclaredElement(), symbolInfo.GetSubstitution());

                            RegexPattern = cacheEntry.Regex;
                            return new ResolveResultWithInfo(resolveResult, ResolveErrorType.OK);
                        }
                    }
                }
            }

            var assemblyStepDefinitionCache = psiServices.GetComponent<AssemblyStepDefinitionCache>();
            var psiAssemblyFileLoader = psiServices.GetComponent<IPsiAssemblyFileLoader>();
            foreach (var (psiAssembly, cacheEntries) in assemblyStepDefinitionCache.AllStepsPerAssembly)
            {
                // FIXME: Should use `assembly` from reqnroll.json instead
                if (!psiModule.Equals(psiAssembly.PsiModule) && !psiModule.References(psiAssembly.PsiModule))
                    continue;

                foreach (var cacheEntry in cacheEntries.Where(c => c.StepKind == stepKind))
                {
                    if (!myOwner.MatchScope(cacheEntry.Scopes))
                        continue;
                    if (cacheEntry.Regex?.IsMatch(stepText) == true)
                    {
                        var assemblyFile = psiAssemblyFileLoader.GetOrLoadAssembly(psiAssembly, false);
                        if (assemblyFile == null)
                            continue;
                        foreach (var decElement in assemblyFile.Types)
                        {
                            if (!(decElement is IClass cl))
                                continue;

                            var method = cl.GetMembers().OfType<IMethod>().FirstOrDefault(x => x.ShortName == cacheEntry.MethodName);
                            if (method == null)
                                continue;

                            var symbolInfo = new SymbolInfo(method);
                            var resolveResult = ResolveResultFactory.CreateResolveResult(symbolInfo.GetDeclaredElement(), symbolInfo.GetSubstitution());

                            RegexPattern = cacheEntry.Regex;
                            return new ResolveResultWithInfo(resolveResult, ResolveErrorType.OK);
                        }
                    }

                }
            }
            return new ResolveResultWithInfo(EmptyResolveResult.Instance, ResolveErrorType.NOT_RESOLVED);
        }

        private static IEnumerable<IMethod> GetAllMethodFromClassAndBaseClasses(IClass clazz)
        {
            foreach (var method in clazz.Methods)
                yield return method;
            var baseClassType = clazz.GetBaseClassType()?.GetTypeElement()?.GetSingleDeclaration();
            while (baseClassType is IClassDeclaration baseClassDeclaration)
            {
                foreach (var declaredElementMethod in baseClassDeclaration.MethodDeclarationsEnumerable)
                    yield return declaredElementMethod.DeclaredElement;
                baseClassType = baseClassDeclaration.DeclaredElement?.GetBaseClassType()?.GetTypeElement()?.GetSingleDeclaration();
            }
        }

        public override string GetName()
        {
            return myOwner.GetStepText();
        }

        public GherkinStepKind GetStepKind()
        {
            return myOwner.EffectiveStepKind;
        }

        public string GetStepText()
        {
            var containingScenario = myOwner.GetContainingNode<IGherkinScenario>();
            if (containingScenario is GherkinScenarioOutline scenarioOutline)
                return myOwner.GetStepTextForExample(scenarioOutline.GetExampleData(0));
            return myOwner.GetStepText();
        }

        public override ISymbolTable GetReferenceSymbolTable(bool useReferenceName)
        {
            throw new NotImplementedException();
        }

        public override TreeTextRange GetTreeTextRange()
        {
            return myOwner.GetTreeTextRange();
        }

        public IProject GetProject()
        {
            return myOwner.GetProject();
        }

        public override IReference BindTo(IDeclaredElement element)
        {
            return BindTo(element, EmptySubstitution.INSTANCE);
        }

        public override IReference BindTo(IDeclaredElement element, ISubstitution substitution)
        {
            if (!(element is Method))
                return this;
            /*var selector = CssElementFactory.GetInstance(myOwner).CreateSelector<ISimpleSelector>(".$0", (object) classDeclaredElement.ShortName);
            if (selector.FirstChild != selector.LastChild)
                return this;
            if (!(selector.FirstChild is IClassSelector firstChild))
                return this;
            using (WriteLockCookie.Create(myOwner.IsPhysical(), "/Product.Root/Psi.Features/Web/Core/Psi/Src/Css/Impl/Tree/References/CssClassReference.cs", nameof (BindTo)))
                ModificationUtil.ReplaceChild(myOwner, firstChild.Identifier);*/
            return myOwner.GetReferences<IReference>().Single();
        }

        public override IAccessContext GetAccessContext()
        {
            return new ElementAccessContext(myOwner);
        }
        
        public CultureInfo GetGherkinFileCulture()
        {
            return new (myOwner.GetContainingNode<GherkinFile>().NotNull().Lang);
        }

        public bool IsInsideScenarioOutline()
        {
            return myOwner.Parent is GherkinScenarioOutline;
        }
    }
}