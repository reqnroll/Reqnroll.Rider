using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi.Search;
using ReSharperPlugin.SpecflowRiderPlugin.References;

namespace ReSharperPlugin.SpecflowRiderPlugin.Navigation;

[OccurrenceProvider(Priority = 10)]
public class SpecflowOccurenceProvider : IOccurrenceProvider
{
    public IOccurrence MakeOccurrence(FindResult findResult)
    {
        if (findResult is FindResultReference findResultReference)
        {
            if (findResultReference.Reference is SpecflowStepDeclarationReference specflowStepDeclarationReference)
                return new SpecflowStepOccurrence(specflowStepDeclarationReference);
        }

        return null;
    }
}