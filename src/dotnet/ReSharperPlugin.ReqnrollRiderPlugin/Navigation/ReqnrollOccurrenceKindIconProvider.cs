using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.UI.Icons;
using ReSharperPlugin.ReqnrollRiderPlugin.Icons;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Navigation;

[ShellComponent(Instantiation.DemandAnyThread)]
public class ReqnrollOccurrenceKindIconProvider : IOccurrenceKindIconProvider
{
    public IconId GetImageId(OccurrenceKind declaredElement)
    {
        if (declaredElement == ReqnrollOccurenceKindProvider.ReqnrollStep)
            return ReqnrollIcons.ReqnrollIcon;
        return null;
    }

    public int GetPriority(OccurrenceKind occurrenceKind)
    {
        if (occurrenceKind == ReqnrollOccurenceKindProvider.ReqnrollStep)
            return -8;
        return 0;
    }
}