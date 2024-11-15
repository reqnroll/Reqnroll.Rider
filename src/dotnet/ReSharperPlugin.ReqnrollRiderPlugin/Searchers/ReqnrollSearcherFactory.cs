using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Parts;
using JetBrains.Collections;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsUsages;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Searchers
{
    [PsiSharedComponent(Instantiation.DemandAnyThreadUnsafe)]
    public class ReqnrollSearcherFactory
        : DomainSpecificSearcherFactoryBase
    {
        public override bool IsCompatibleWithLanguage(PsiLanguageType languageType)
        {
            return languageType.Is<CSharpLanguage>() || languageType.Is<GherkinLanguage>();
        }

        public override IEnumerable<string> GetAllPossibleWordsInFile(IDeclaredElement declaredElement)
        {
            if (!(declaredElement is IMethod methodDeclaration))
                return base.GetAllPossibleWordsInFile(declaredElement);

            var reqnrollStepsDefinitionsCache = declaredElement.GetPsiServices().GetComponent<ReqnrollStepsDefinitionsCache>();
            var words = new HashSet<string>();
            foreach (var sourceFile in declaredElement.GetSourceFiles())
            {
                var stepsInFile = reqnrollStepsDefinitionsCache.AllStepsPerFiles[sourceFile];
                foreach (var step in stepsInFile.Where(x => x.MethodName == declaredElement.ShortName).Where(x => x.ClassFullName == methodDeclaration.GetContainingType()?.GetClrName().FullName))
                    words.AddRange(step.Pattern.SplitByWords());
            }
            return words;
        }

        public override IDomainSpecificSearcher CreateReferenceSearcher(IDeclaredElementsSet elements, ReferenceSearcherParameters referenceSearcherParameters)
        {
            return new ReqnrollStepReferenceSearcher(elements);
        }

        public override ISearchDomain GetDeclaredElementSearchDomain(IDeclaredElement declaredElement)
        {
            if (!(declaredElement is IMethod methodDeclaration))
                return base.GetDeclaredElementSearchDomain(declaredElement);

            var reqnrollStepsUsagesCache = declaredElement.GetPsiServices().GetComponent<ReqnrollStepsUsagesCache>();
            var reqnrollStepsDefinitionsCache = declaredElement.GetPsiServices().GetComponent<ReqnrollStepsDefinitionsCache>();

            var files = new List<IPsiSourceFile>();
            foreach (var sourceFile in declaredElement.GetSourceFiles())
            {
                var stepsInFile = reqnrollStepsDefinitionsCache.AllStepsPerFiles[sourceFile];
                foreach (var step in stepsInFile.Where(x => x.MethodName == declaredElement.ShortName).Where(x => x.ClassFullName == methodDeclaration.GetContainingType()?.GetClrName().FullName))
                foreach (var (stepSourceFileUsage, stepsTexts) in reqnrollStepsUsagesCache.StepUsages[step.StepKind])
                foreach (var stepText in stepsTexts)
                {
                    if (step.Regex?.IsMatch(stepText) == true)
                    {
                        files.Add(stepSourceFileUsage);
                    }
                }
            }

            return SearchDomainFactory.Instance.CreateSearchDomain(files);
        }
    }
}