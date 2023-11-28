using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Occurrences;

namespace ReSharperPlugin.SpecflowRiderPlugin.Navigation;

[SolutionComponent]
public class SpecflowOccurenceKindProvider : IOccurrenceKindProvider
{
    public static readonly OccurrenceKind SpecflowStep = OccurrenceKind.CreateSemantic("SpecflowStep");

    public ICollection<OccurrenceKind> GetOccurrenceKinds(IOccurrence occurrence)
    {
        if (occurrence is SpecflowStepOccurrence)
            return [SpecflowStep];
        return [];
    }

    public IEnumerable<OccurrenceKind> GetAllPossibleOccurrenceKinds()
    {
        return [SpecflowStep];
    }
}

