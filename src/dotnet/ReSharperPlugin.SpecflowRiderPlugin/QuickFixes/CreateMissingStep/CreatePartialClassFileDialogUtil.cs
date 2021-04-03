using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.Rider.Model.UIAutomation;

namespace ReSharperPlugin.SpecflowRiderPlugin.QuickFixes.CreateMissingStep
{
    public interface ICreateStepPartialClassFile
    {
        public delegate void CreatePartialClassFile(string filename);

        void OpenCreatePartialClassFileDialog(CreatePartialClassFile onValidation);
    }

    [PsiComponent]
    public class CreateStepPartialClassFile : ICreateStepPartialClassFile
    {
        private readonly IDialogHost _dialogHost;

        public CreateStepPartialClassFile(
            IDialogHost dialogHost
        )
        {
            _dialogHost = dialogHost;
        }

        public void OpenCreatePartialClassFileDialog(ICreateStepPartialClassFile.CreatePartialClassFile onValidation)
        {
            var lifetimeDefinition = new LifetimeDefinition();
            var panel = CreateControl(lifetimeDefinition.Lifetime);
            _dialogHost.Show(lifetime => CreateDialog(lifetime, onValidation, panel, lifetimeDefinition), onDialogDispose: () => { lifetimeDefinition.Terminate(); });
        }

        private static BeDialog CreateDialog(Lifetime lifetime, ICreateStepPartialClassFile.CreatePartialClassFile onValidation, BeControl panel, LifetimeDefinition lifetimeDefinition)
        {
            return panel.InDialog(
                    "Create new binding class",
                    "",
                    DialogModality.MODAL,
                    BeControlSizes.GetSize(BeControlSizeType.MEDIUM))
                .WithOkButton(
                    lifetime,
                    () =>
                    {
                        onValidation(
                            panel.GetBeControlById<BeTextBox>("filename").GetText()
                        );
                    },
                    disableWhenInvalid: false)
                .WithCancelButton(lifetimeDefinition.Lifetime);
        }

        private BeControl CreateControl(Lifetime lifetime)
        {
            var grid = BeControls.GetGrid();
            grid.AddElement("Filename".GetBeLabel());
            grid.AddElement(BeControls.GetTextBox(lifetime, id: "filename"));
            return grid;
        }
    }
}