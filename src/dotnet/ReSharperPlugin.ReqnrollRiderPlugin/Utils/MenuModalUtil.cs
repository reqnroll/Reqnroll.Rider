using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application.Parts;
using JetBrains.Application.UI.Controls;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Utils
{
    public interface IMenuModalUtil
    {
        void OpenSelectStepClassMenu<T>(IEnumerable<T> actions, string title, PopupWindowContextSource popupWindowContextSource)
            where T : class, IMenuAction;
    }

    public interface IMenuAction
    {
        RichText Text { get; }
        [CanBeNull]
        RichText ShortcutText { get; }
        IconId Icon { get; }
        void Execute();
    }

    [PsiSharedComponent(Instantiation.DemandAnyThreadUnsafe)]
    public class MenuModalUtil(
        JetPopupMenus jetPopupMenus
    ) : IMenuModalUtil
    {

        public void OpenSelectStepClassMenu<T>(IEnumerable<T> actions, string title, PopupWindowContextSource popupWindowContextSource)
            where T : class, IMenuAction
        {
            jetPopupMenus.ShowModal(JetPopupMenu.ShowWhen.AutoExecuteIfSingleEnabledItem,
                (lifetime, menu) =>
                {
                    menu.Caption.Value = WindowlessControlAutomation.Create(title);
                    menu.ItemKeys.AddRange(actions);
                    menu.DescribeItem.Advise(lifetime, e =>
                        {
                            if (e.Key is not T action)
                                return;

                            e.Descriptor.Icon = action.Icon;
                            e.Descriptor.Style = MenuItemStyle.Enabled;
                            e.Descriptor.ShortcutText = action.ShortcutText;
                            e.Descriptor.Text = action.Text;
                        }
                    );

                    menu.ItemClicked.Advise(lifetime, key =>
                        {
                            if (key is not T action)
                                return;

                            action.Execute();
                        }
                    );

                    menu.PopupWindowContextSource = popupWindowContextSource;
                }
            );
        }

    }
}