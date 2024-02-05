using System.IO;
using JetBrains.Application.Threading;
using JetBrains.Diagnostics;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.UI.Automation;
using JetBrains.ReSharper.Feature.Services.UI.Validation;
using JetBrains.ReSharper.Psi;
using JetBrains.Rider.Model.UIAutomation;

namespace ReSharperPlugin.ReqnrollRiderPlugin.QuickFixes.CreateMissingStep
{
    public interface ICreateStepPartialClassFile
    {
        public delegate void CreatePartialClassFile(string path, string filename);

        void OpenCreatePartialClassFileDialog(IPsiSourceFile otherPartSourceFile, CreatePartialClassFile onValidation);
    }

    [PsiComponent]
    public class CreateStepPartialClassFile : ICreateStepPartialClassFile
    {
        private readonly IDialogHost _dialogHost;
        private readonly IThreading _threading;

        public CreateStepPartialClassFile(
            IDialogHost dialogHost,
            IThreading threading)
        {
            _dialogHost = dialogHost;
            _threading = threading;
        }

        public void OpenCreatePartialClassFileDialog(IPsiSourceFile otherPartSourceFile, ICreateStepPartialClassFile.CreatePartialClassFile onValidation)
        {
            _threading.ReentrancyGuard.Queue("Open Create Reqnroll Steps Class dialog", () =>
            {
                _dialogHost.Show(lifetime =>
                {
                    var panel = CreateControl(lifetime, otherPartSourceFile);
                    return CreateDialog(lifetime, onValidation, panel);
                });
            });
        }

        private static BeDialog CreateDialog(Lifetime lifetime, ICreateStepPartialClassFile.CreatePartialClassFile onValidation, BeControl panel)
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
                            panel.GetBeControlById<BeTextBox>("path").GetText(),
                            panel.GetBeControlById<BeTextBox>("filename").GetText()
                        );
                    },
                    disableWhenInvalid: true)
                .WithCancelButton(lifetime);
        }

        private BeControl CreateControl(Lifetime lifetime, IPsiSourceFile otherPartSourceFile)
        {
            var grid = BeControls.GetGrid();

            var solution = otherPartSourceFile.GetSolution();
            var project = otherPartSourceFile.GetProject().NotNull();
            grid.AddElement(BeControls.GetTextBox(lifetime, id: "filename")
                .WithTextNotEmpty(lifetime, null)
                .WithValidFileName(lifetime, null)
                .WithDescription("Filename", lifetime));
            grid.AddElement(BeControls.GetTextBox(lifetime, id: "path", initialText: project.Name + Path.DirectorySeparatorChar + otherPartSourceFile.GetLocation().Parent.MakeRelativeTo(project.Location).FullPath)
                .WithTextNotEmpty(lifetime, null)
                .WithFolderCompletion(solution, lifetime)
                .WithValidPath(lifetime, ValidationIcons.Error)
                .WithFolderExistsOrMustBeCreated(solution, lifetime)
                .WithDescription("Folder", lifetime));

            return grid;
        }
    }
}