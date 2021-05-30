using System.Collections.Generic;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.ReSharper.Host.Features.Icons;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Icons;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.ExecutionFailedStep
{
    public class ExecutionFailedStepGutterMarkType : IconGutterMarkType
    {
        public ExecutionFailedStepGutterMarkType() : base(SpecFlowIcons.FailedStepIcon)
        {
        }

        public override IAnchor Priority => BulbMenuAnchors.PermanentItem;

        public override IEnumerable<BulbMenuItem> GetBulbMenuItems(IHighlighter highlighter)
        {
            return EmptyList<BulbMenuItem>.Enumerable;
        }
    }
}