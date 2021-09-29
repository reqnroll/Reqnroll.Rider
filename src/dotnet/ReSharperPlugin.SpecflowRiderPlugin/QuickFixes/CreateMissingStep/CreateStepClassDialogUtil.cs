using System.IO;
using JetBrains.Annotations;
using JetBrains.Application.Threading;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.UI.Automation;
using JetBrains.ReSharper.Feature.Services.UI.Validation;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.QuickFixes.CreateMissingStep
{
    public interface ICreateStepClassDialogUtil
    {
        public delegate void CreateStepClass(string className, string path, bool isPartial);

        void OpenCreateClassDialog(ISolution solution, IProject project, [CanBeNull] VirtualFileSystemPath defaultFolder, CreateStepClass onValidation);
    }

    [PsiComponent]
    public class CreateStepClassDialogUtil : ICreateStepClassDialogUtil
    {
        private readonly IDialogHost _dialogHost;
        private readonly IThreading _threading;

        public CreateStepClassDialogUtil(
            IDialogHost dialogHost,
            IThreading threading
        )
        {
            _dialogHost = dialogHost;
            _threading = threading;
        }

        public void OpenCreateClassDialog(
            ISolution solution,
            IProject project,
            VirtualFileSystemPath defaultFolder,
            ICreateStepClassDialogUtil.CreateStepClass onValidation
        )
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
                    "CreateStepClassDialog",
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
                    disableWhenInvalid: true)
                .WithCancelButton(lifetime);
        }

        private BeControl CreateControl(Lifetime lifetime, ISolution solution, IProject project, [CanBeNull] VirtualFileSystemPath defaultFolder)
        {
            var grid = BeControls.GetGrid();
            grid.AddElement(BeControls.GetTextBox(lifetime, id: "className")
                .WithTextNotEmpty(lifetime, null)
                .WithValidName(CSharpLanguage.Instance, lifetime, ValidationIcons.Error, CLRDeclaredElementType.CLASS)
                .WithDescription("Class name", lifetime));

            grid.AddElement(BeControls.GetTextBox(lifetime, id: "path", initialText: project?.Name + (project?.Name == null ? string.Empty : Path.DirectorySeparatorChar) + defaultFolder?.MakeRelativeTo(project.Location).FullPath)
                .WithTextNotEmpty(lifetime, null)
                .WithFolderCompletion(solution, lifetime)
                .WithValidPath(lifetime, ValidationIcons.Error)
                .WithFolderExistsOrMustBeCreated(solution, lifetime)
                .WithDescription("Folder", lifetime));

            grid.AddElement(BeControls.GetCheckBox("Partial class", "isPartial"));
            return grid;
        }
    }
}