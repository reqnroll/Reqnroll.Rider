using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.UI.Icons;
using ReSharperPlugin.SpecflowRiderPlugin.Icons;

namespace ReSharperPlugin.SpecflowRiderPlugin.Navigation;

[ShellComponent(Instantiation.DemandAnyThread)]
public class SpecflowOccurrenceKindIconProvider : IOccurrenceKindIconProvider
{
    public IconId GetImageId(OccurrenceKind declaredElement)
    {
        if (declaredElement == SpecflowOccurenceKindProvider.SpecflowStep)
            return SpecFlowIcons.SpecFlowIcon;
        return null;
    }

    public int GetPriority(OccurrenceKind occurrenceKind)
    {
        if (occurrenceKind == SpecflowOccurenceKindProvider.SpecflowStep)
            return -8;
        return 0;
    }
}