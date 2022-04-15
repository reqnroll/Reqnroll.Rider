using System.Collections.Generic;
using System.Linq;
using JetBrains.Collections;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsUsages;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Searchers
{
    [PsiSharedComponent]
    public class SpecflowSearcherFactory : DomainSpecificSearcherFactoryBase
    {
        public override bool IsCompatibleWithLanguage(PsiLanguageType languageType)
        {
            return languageType.Is<CSharpLanguage>() || languageType.Is<GherkinLanguage>();
        }

        public override IEnumerable<string> GetAllPossibleWordsInFile(IDeclaredElement declaredElement)
        {
            if (!(declaredElement is IMethod methodDeclaration))
                return base.GetAllPossibleWordsInFile(declaredElement);

            var specflowStepsDefinitionsCache = declaredElement.GetPsiServices().GetComponent<SpecflowStepsDefinitionsCache>();
            var words = new HashSet<string>();
            foreach (var sourceFile in declaredElement.GetSourceFiles())
            {
                var stepsInFile = specflowStepsDefinitionsCache.AllStepsPerFiles[sourceFile];
                foreach (var step in stepsInFile.Where(x => x.MethodName == declaredElement.ShortName).Where(x => x.ClassFullName == methodDeclaration.GetContainingType()?.GetClrName().FullName))
                    words.AddRange(step.Pattern.SplitByWords());
            }
            return words;
        }

        public override IDomainSpecificSearcher CreateReferenceSearcher(IDeclaredElementsSet elements, ReferenceSearcherParameters referenceSearcherParameters)
        {
            return new SpecflowStepReferenceSearcher(elements);
        }

        public override ISearchDomain GetDeclaredElementSearchDomain(IDeclaredElement declaredElement)
        {
            if (!(declaredElement is IMethod methodDeclaration))
                return base.GetDeclaredElementSearchDomain(declaredElement);

            var specflowStepsUsagesCache = declaredElement.GetPsiServices().GetComponent<SpecflowStepsUsagesCache>();
            var specflowStepsDefinitionsCache = declaredElement.GetPsiServices().GetComponent<SpecflowStepsDefinitionsCache>();

            var files = new List<IPsiSourceFile>();
            foreach (var sourceFile in declaredElement.GetSourceFiles())
            {
                var stepsInFile = specflowStepsDefinitionsCache.AllStepsPerFiles[sourceFile];
                foreach (var step in stepsInFile.Where(x => x.MethodName == declaredElement.ShortName).Where(x => x.ClassFullName == methodDeclaration.GetContainingType()?.GetClrName().FullName))
                foreach (var (stepSourceFileUsage, stepsTexts) in specflowStepsUsagesCache.StepUsages[step.StepKind])
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