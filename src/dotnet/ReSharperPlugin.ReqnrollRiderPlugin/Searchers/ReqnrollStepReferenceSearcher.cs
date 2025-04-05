using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Extensions;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Searchers;

public class ReqnrollStepReferenceSearcher(IDeclaredElementsSet declaredElements) : IDomainSpecificSearcher
{

    public bool ProcessProjectItem<TResult>(IPsiSourceFile sourceFile, IFindResultConsumer<TResult> consumer)
    {
        return sourceFile.LanguageType.Is<GherkinProjectFileType>() && sourceFile.GetPsiFiles<GherkinLanguage>().Any(file => ProcessElement(file, consumer));
    }

    public bool ProcessElement<TResult>(ITreeNode element, IFindResultConsumer<TResult> consumer)
    {
        if (!element.Language.Is<GherkinLanguage>())
            return false;
        var containingFile = element.GetContainingFile();
        if (containingFile == null)
            return false;
        var projectFile = containingFile.GetSourceFile().ToProjectFile();
        if (projectFile == null || !projectFile.IsValid())
            return false;

        foreach (var declaredElement in declaredElements)
        {
            if (!(declaredElement is IMethod method))
                continue;
            foreach (var gherkinStep in element.GetChildrenInSubtrees<GherkinStep>())
            {
                var reference = gherkinStep.GetStepReference();
                var resolveResultWithInfo = reference.Resolve();
                if (resolveResultWithInfo.ResolveErrorType == ResolveErrorType.OK)
                {
                    foreach (var declarationMethod in resolveResultWithInfo.Result.Elements<IMethod>())
                    {
                        if (declarationMethod.Element.Equals(method) && consumer.Accept(new FindResultReference(reference)) == FindExecution.Stop)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}