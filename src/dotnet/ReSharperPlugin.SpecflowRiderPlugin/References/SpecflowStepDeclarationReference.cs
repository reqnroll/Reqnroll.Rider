using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.UI.BindableLinq.Extensions;
using JetBrains.Collections;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.Impl.reflection2.elements.Compiled;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.Dotnet.TargetFrameworkIds;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.References
{
    public class SpecflowStepDeclarationReference : TreeReferenceBase<GherkinStep>
    {
        public SpecflowStepDeclarationReference([NotNull] GherkinStep owner) : base(owner)
        {
        }

        public override ResolveResultWithInfo ResolveWithoutCache()
        {
            var psiServices = myOwner.GetPsiServices();
            var specflowStepsDefinitionsCache = psiServices.GetComponent<SpecflowStepsDefinitionsCache>();
            var stepKind = myOwner.GetStepKind();
            var stepText = myOwner.GetStepText();
            var projectReferences = myOwner.GetProject()?.GetAllModuleReferences().OfType<SimpleProjectToProjectReference>().ToList();
            foreach (var (sourceFile, cacheEntries) in specflowStepsDefinitionsCache.AllStepsPerFiles)
            {
                if (sourceFile.GetProject() != myOwner.GetProject())
                    if (projectReferences?.Any(x => x.Name == sourceFile.GetProject()?.Name) != true)
                        continue;
                foreach (var cacheEntry in cacheEntries.Where(c => c.StepKind == stepKind).OrderByDescending(x => x.Pattern.Length))
                {
                    if (cacheEntry.Regex?.IsMatch(stepText) == true)
                    {
                        var types = psiServices.Symbols.GetTypesAndNamespacesInFile(sourceFile);
                        foreach (var decElement in types)
                        {
                            if (!(decElement is IClass cl))
                                continue;

                            var method = cl.GetMembers().OfType<IMethod>().FirstOrDefault(x => x.ShortName == cacheEntry.MethodName);
                            if (method == null)
                                continue;

                            var symbolInfo = new SymbolInfo(method);
                            var resolveResult = ResolveResultFactory.CreateResolveResult(symbolInfo.GetDeclaredElement(), symbolInfo.GetSubstitution());

                            return new ResolveResultWithInfo(resolveResult, ResolveErrorType.OK);
                        }
                    }

                }
            }
            return new ResolveResultWithInfo(EmptyResolveResult.Instance, ResolveErrorType.NOT_RESOLVED);
        }

        public override string GetName()
        {
            return myOwner.GetStepText();
        }

        public GherkinStepKind GetStepKind()
        {
            return myOwner.GetStepKind();
        }

        public string GetStepText()
        {
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

        public override IReference BindTo(IDeclaredElement element)
        {
            return BindTo(element, EmptySubstitution.INSTANCE);
        }

        public override IReference BindTo(IDeclaredElement element, ISubstitution substitution)
        {
            if (!(element is Method method))
                return this;
            /*var selector = CssElementFactory.GetInstance(myOwner).CreateSelector<ISimpleSelector>(".$0", (object) classDeclaredElement.ShortName);
            if (selector.FirstChild != selector.LastChild)
                return this;
            if (!(selector.FirstChild is IClassSelector firstChild))
                return this;
            using (WriteLockCookie.Create(myOwner.IsPhysical(), "/Product.Root/Psi.Features/Web/Core/Psi/Src/Css/Impl/Tree/References/CssClassReference.cs", nameof (BindTo)))
                ModificationUtil.ReplaceChild(myOwner, firstChild.Identifier);*/
            return this.myOwner.GetReferences<IReference>().Single();
        }

        public override IAccessContext GetAccessContext()
        {
            return new ElementAccessContext(myOwner);
        }
    }
}