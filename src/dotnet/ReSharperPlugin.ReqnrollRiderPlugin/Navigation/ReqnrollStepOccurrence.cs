using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.References;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Navigation;

public sealed class ReqnrollStepOccurrence(ReqnrollStepDeclarationReference reqnrollStepDeclarationReference) : IOccurrence
{
    public bool Navigate(
        ISolution solution,
        PopupWindowContextSource windowContext,
        bool transferFocus,
        TabOptions tabOptions = TabOptions.Default
    )
    {
        reqnrollStepDeclarationReference.GetTreeNode().NavigateToNode(true);
        PresentationOptions = OccurrencePresentationOptions.DefaultOptions;
        return true;
    }

    public ISolution GetSolution()
    {
        return reqnrollStepDeclarationReference.GetProject().GetSolution();
    }

    public string DumpToString()
    {
        var sb = new StringBuilder();
        sb.Append($"SourceFilePtr: = {SourceFile.PsiStorage.PersistentIndex}");
        sb.Append($" ReqnrollStep: = {reqnrollStepDeclarationReference.GetStepText()}");
        return sb.ToString();
    }

    public OccurrenceType OccurrenceType => OccurrenceType.Occurrence;
    public bool IsValid => SourceFile.IsValid();
    public OccurrencePresentationOptions PresentationOptions { get; set; }
    public IPsiSourceFile SourceFile => reqnrollStepDeclarationReference.GetTreeNode().GetSourceFile();

    public string GetStepText()
    {
        return reqnrollStepDeclarationReference.GetStepText();
    }

    public string GetScenarioText()
    {
        return reqnrollStepDeclarationReference.ScenarioText;
    }

    [CanBeNull]
    public string GetRelatedFilePresentation()
    {
        return SourceFile.DisplayName.Split('\\').Last();
    }

    public override string ToString()
    {
        return GetStepText();
    }
}