using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.References;

namespace ReSharperPlugin.SpecflowRiderPlugin.Navigation;

public sealed class SpecflowStepOccurrence(SpecflowStepDeclarationReference specflowStepDeclarationReference) : IOccurrence
{
    public bool Navigate(
        ISolution solution,
        PopupWindowContextSource windowContext,
        bool transferFocus,
        TabOptions tabOptions = TabOptions.Default
    )
    {
        specflowStepDeclarationReference.GetTreeNode().NavigateToNode(true);
        PresentationOptions = OccurrencePresentationOptions.DefaultOptions;
        return true;
    }

    public ISolution GetSolution()
    {
        return specflowStepDeclarationReference.GetProject().GetSolution();
    }

    public string DumpToString()
    {
        var sb = new StringBuilder();
        sb.Append($"SourceFilePtr: = {SourceFile.PsiStorage.PersistentIndex}");
        sb.Append($" SpecflowStep: = {specflowStepDeclarationReference.GetStepText()}");
        return sb.ToString();
    }

    public OccurrenceType OccurrenceType => OccurrenceType.Occurrence;
    public bool IsValid => SourceFile.IsValid();
    public OccurrencePresentationOptions PresentationOptions { get; set; }
    public IPsiSourceFile SourceFile => specflowStepDeclarationReference.GetTreeNode().GetSourceFile();

    public string GetStepText()
    {
        return specflowStepDeclarationReference.GetStepText();
    }

    public string GetScenarioText()
    {
        return specflowStepDeclarationReference.ScenarioText;
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