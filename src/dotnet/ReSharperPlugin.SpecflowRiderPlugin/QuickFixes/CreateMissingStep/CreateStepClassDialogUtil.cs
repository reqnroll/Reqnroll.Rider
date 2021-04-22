using System;
using System.Linq;
using JetBrains.Application.UI.Controls.FileSystem;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.PathActions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.Rider.Model.UIAutomation;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;

namespace ReSharperPlugin.SpecflowRiderPlugin.QuickFixes.CreateMissingStep
{
    public interface ICreateStepClassDialogUtil
    {
        public delegate void CreateStepClass(string className, string path, bool isPartial);

        void OpenCreateClassDialog(CreateStepClass onValidation);
    }

    [PsiComponent]
    public class CreateStepClassDialogUtil : ICreateStepClassDialogUtil
    {
        private readonly IDialogHost _dialogHost;
        private readonly IThreading _threading;
        private readonly IconHostBase _iconHost;
        private readonly ICommonFileDialogs _commonFileDialogs;
        private readonly SpecflowStepsDefinitionsCache _specflowStepsDefinitionsCache;

        public CreateStepClassDialogUtil(
            IDialogHost dialogHost,
            IThreading threading
            IconHostBase iconHost,
            ICommonFileDialogs commonFileDialogs,
            SpecflowStepsDefinitionsCache specflowStepsDefinitionsCache
        )
        {
            _dialogHost = dialogHost;
            _threading = threading;
            _iconHost = iconHost;
            _commonFileDialogs = commonFileDialogs;
            _specflowStepsDefinitionsCache = specflowStepsDefinitionsCache;
        }

        public void OpenCreateClassDialog(ICreateStepClassDialogUtil.CreateStepClass onValidation)
        {
            _threading.ReentrancyGuard.Queue("Open Create Specflow Steps Class dialog", () =>
            {
                _dialogHost.Show(lifetime =>
                {
                    var panel = CreateControl(lifetime, solution, project, defaultFolder);
                    return CreateDialog(lifetime, onValidation, panel);
                });
            });
        }

        private static BeDialog CreateDialog(Lifetime lifetime, ICreateStepClassDialogUtil.CreateStepClass onValidation, BeControl panel)
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
                            panel.GetBeControlById<BeTextBox>("className").GetText(),
                            panel.GetBeControlById<BeTextBox>("path").GetText(),
                            panel.GetBeControlById<BeCheckbox>("isPartial").Property.Value ?? false
                        );
                    },
                    disableWhenInvalid: false)
                .WithCancelButton(lifetime);
        }

        private BeControl CreateControl(Lifetime lifetime, IPsiSourceFile sourceFile)
        {
            var grid = BeControls.GetGrid();
            grid.AddElement("Class name".GetBeLabel());
            grid.AddElement(BeControls.GetTextBox(lifetime, id: "className"));
            grid.AddElement(BeControls.GetCheckBox("Partial class", "isPartial"));

            var path = new Property<string>("path", sourceFile.GetLocation().Directory.FullPath);
            grid.AddElement(BeControls.GetPathSelectionElement(new TextBoxPathSelectionActionData(path, BeInvalidValuePropagation.SAVE_ANY), lifetime, _iconHost, _commonFileDialogs, "Select Path"));
            return grid;
        }
    }
}