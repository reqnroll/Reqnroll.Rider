using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi.Search;
using ReSharperPlugin.ReqnrollRiderPlugin.References;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Navigation;

[OccurrenceProvider(Priority = 10)]
public class ReqnrollOccurenceProvider : IOccurrenceProvider
{
    public IOccurrence MakeOccurrence(FindResult findResult)
    {
        if (findResult is FindResultReference findResultReference)
        {
            if (findResultReference.Reference is ReqnrollStepDeclarationReference reqnrollStepDeclarationReference)
                return new ReqnrollStepOccurrence(reqnrollStepDeclarationReference);
        }

        return null;
    }
}