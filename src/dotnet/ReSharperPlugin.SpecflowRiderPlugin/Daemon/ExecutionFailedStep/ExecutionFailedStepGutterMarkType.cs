using System.Collections.Generic;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.ReSharper.Host.Features.Icons;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.ExecutionFailedStep
{
    public class ExecutionFailedStepGutterMarkType : IconGutterMarkType
    {
        public ExecutionFailedStepGutterMarkType() : base(new FrontendIconId("icons/failed-step-icon.svg"))
        {
        }

        public override IEnumerable<BulbMenuItem> GetBulbMenuItems(IHighlighter highlighter)
        {
            return EmptyList<BulbMenuItem>.Enumerable;
        }
    }
}