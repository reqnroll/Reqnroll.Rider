using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.SelectEmbracingConstruct;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ExtendSelection
{
    [ProjectFileType(typeof(GherkinProjectFileType))]
    [Language(typeof(GherkinLanguage))]
    public class GherkinSelectEmbracingConstructProvider : ISelectEmbracingConstructProvider
    {
        public bool IsAvailable(IPsiSourceFile sourceFile)
            => sourceFile.Properties.ShouldBuildPsi;

        public ISelectedRange GetSelectedRange(IPsiSourceFile sourceFile, DocumentRange documentRange)
        {
            if (sourceFile.GetPrimaryPsiFile() is not GherkinFile gherkinFile)
                return null;

            var gherkinNode = gherkinFile.FindNodeAt(documentRange);
            if (gherkinNode == null)
                return null;

            return new GherkinNodeSelection(gherkinFile, gherkinNode);
        }
    }
}