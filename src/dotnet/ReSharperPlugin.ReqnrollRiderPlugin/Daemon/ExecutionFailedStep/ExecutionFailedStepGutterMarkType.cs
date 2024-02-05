using System.Collections.Generic;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Icons;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.ExecutionFailedStep
{
    public class ExecutionFailedStepGutterMarkType : IconGutterMarkType
    {
        public ExecutionFailedStepGutterMarkType() : base(ReqnrollIcons.FailedStepIcon)
        {
        }

        public override IAnchor Priority => BulbMenuAnchors.PermanentItem;

        public override IEnumerable<BulbMenuItem> GetBulbMenuItems(IHighlighter highlighter)
        {
            return EmptyList<BulbMenuItem>.Enumerable;
        }
    }
}