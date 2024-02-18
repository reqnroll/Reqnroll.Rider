using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.UI.RichText;
using JetBrains.Util.Media;
using ReSharperPlugin.ReqnrollRiderPlugin.Icons;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Navigation;

[OccurrencePresenter(Priority = 10.0)]
public class ReqnrollStepOccurrencePresenter : IOccurrencePresenter
{
    public bool Present(
        IMenuItemDescriptor descriptor,
        IOccurrence occurrence,
        OccurrencePresentationOptions occurrencePresentationOptions
    )
    {
        if (occurrence is ReqnrollStepOccurrence reqnrollStepOccurrence)
        {
            var text = new RichText(reqnrollStepOccurrence.GetStepText(), TextStyle.Default);
            descriptor.Text = text;
            descriptor.Style = MenuItemStyle.CanExpand | MenuItemStyle.Enabled;
            descriptor.Tooltip = reqnrollStepOccurrence.GetScenarioText();
            descriptor.ShortcutText = new RichText(reqnrollStepOccurrence.GetRelatedFilePresentation(), TextStyle.FromForeColor(JetRgbaColors.DarkGray));
            descriptor.Icon = ReqnrollIcons.ReqnrollIcon;
        }

        return true;
    }


    public bool IsApplicable(IOccurrence occurrence)
    {
        return occurrence is ReqnrollStepOccurrence;
    }
}