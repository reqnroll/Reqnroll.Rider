using System;
using JetBrains.Annotations;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils;

namespace ReSharperPlugin.ReqnrollRiderPlugin.QuickFixes.CreateMissingStep;

public class CreateStepMenuAction(RichText text, IconId icon, Action onExecute, [CanBeNull] RichText shortcutText = null)
    : IMenuAction
{
    public RichText Text { get; } = text;
    public IconId Icon { get; } = icon;
    public RichText ShortcutText { get; } = shortcutText;
    private Action OnExecute { get; } = onExecute;

    public void Execute()
    {
        OnExecute.Invoke();
    }
}