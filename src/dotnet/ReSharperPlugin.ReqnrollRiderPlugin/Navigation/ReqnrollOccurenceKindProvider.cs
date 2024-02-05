using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Occurrences;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Navigation;

[SolutionComponent]
public class ReqnrollOccurenceKindProvider : IOccurrenceKindProvider
{
    public static readonly OccurrenceKind ReqnrollStep = OccurrenceKind.CreateSemantic("ReqnrollStep");

    public ICollection<OccurrenceKind> GetOccurrenceKinds(IOccurrence occurrence)
    {
        if (occurrence is ReqnrollStepOccurrence)
            return [ReqnrollStep];
        return [];
    }

    public IEnumerable<OccurrenceKind> GetAllPossibleOccurrenceKinds()
    {
        return [ReqnrollStep];
    }
}

