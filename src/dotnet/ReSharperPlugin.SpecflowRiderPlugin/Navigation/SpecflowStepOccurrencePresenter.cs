using System.Text;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.UI.RichText;
using JetBrains.Util.Media;
using ReSharperPlugin.SpecflowRiderPlugin.Icons;

namespace ReSharperPlugin.SpecflowRiderPlugin.Navigation;

[OccurrencePresenter(Priority = 10.0)]
public class SpecflowStepOccurrencePresenter : IOccurrencePresenter
{
    public bool Present(
        IMenuItemDescriptor descriptor,
        IOccurrence occurrence,
        OccurrencePresentationOptions occurrencePresentationOptions
    )
    {
        if (occurrence is SpecflowStepOccurrence specflowStepOccurrence)
        {
            var text = new RichText(specflowStepOccurrence.GetStepText(), TextStyle.Default);
            descriptor.Text = text;
            descriptor.Style = MenuItemStyle.CanExpand | MenuItemStyle.Enabled;
            descriptor.Tooltip = specflowStepOccurrence.GetScenarioText();
            descriptor.ShortcutText = new RichText(specflowStepOccurrence.GetRelatedFilePresentation(), TextStyle.FromForeColor(JetRgbaColors.DarkGray));
            descriptor.Icon = SpecFlowIcons.SpecFlowIcon;
        }

        return true;
    }


    public bool IsApplicable(IOccurrence occurrence)
    {
        return occurrence is SpecflowStepOccurrence;
    }
}