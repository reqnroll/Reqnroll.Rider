using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Feature.Services.Occurrences.OccurrenceInformation;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Pointers;
using JetBrains.Util;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Navigation;

[SolutionFeaturePart]
public class ReqnrollStepOccurenceInfoProvider : IOccurrenceInformationProvider2
{
    public IDeclaredElementEnvoy GetTypeMember(IOccurrence occurrence)
    {
        return null;
    }

    public IDeclaredElementEnvoy GetTypeElement(IOccurrence occurrence)
    {
        return null;
    }

    public IDeclaredElementEnvoy GetNamespace(IOccurrence occurrence)
    {
        return null;
    }

    public OccurrenceMergeContext GetMergeContext(IOccurrence occurrence)
    {
        return new OccurrenceMergeContext(occurrence);
    }

    public TextRange GetTextRange(IOccurrence occurrence)
    {
        return TextRange.InvalidRange;
    }

    public ProjectModelElementEnvoy GetProjectModelElementEnvoy(IOccurrence occurrence)
    {
        var sourceFile = (occurrence as ReqnrollStepOccurrence)?.SourceFile;
        if (sourceFile != null)
        {
            var map = sourceFile.GetSolution().GetComponent<DocumentToProjectFileMappingStorage>();
            var miscFilesProjectFile = map.TryGetProjectFile(sourceFile.Document);
            return miscFilesProjectFile != null ? ProjectModelElementEnvoy.Create(miscFilesProjectFile) : null;
        }

        return null;
    }

    public SourceFilePtr GetSourceFilePtr(IOccurrence occurrence)
    {
        return (occurrence as ReqnrollStepOccurrence)?.SourceFile.Ptr() ?? SourceFilePtr.Fake;
    }

    public bool IsApplicable(IOccurrence occurrence)
    {
        return occurrence is ReqnrollStepOccurrence;
    }

    public void SetTabOptions(TabOptions tabOptions, IOccurrence occurrence)
    {
    }
}