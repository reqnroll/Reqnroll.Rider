using System;
using JetBrains.Annotations;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils;

namespace ReSharperPlugin.ReqnrollRiderPlugin.QuickFixes.CreateMissingStep
{
    public class CreateStepMenuAction : IMenuAction
    {
        public RichText Text { get; }
        public IconId Icon { get; }
        public RichText ShortcutText { get; }
        private Action OnExecute { get; }

        public CreateStepMenuAction(RichText text, IconId icon, Action onExecute, [CanBeNull] RichText shortcutText = null)
        {
            Text = text;
            Icon = icon;
            OnExecute = onExecute;
            ShortcutText = shortcutText;
        }

        public void Execute()
        {
            OnExecute.Invoke();
        }
    }
}