using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.UI.Icons.CommonThemedIcons;
using JetBrains.Diagnostics;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Resources;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resources;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.UI.RichText;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Extensions;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.References;
using ReSharperPlugin.SpecflowRiderPlugin.Utils;

namespace ReSharperPlugin.SpecflowRiderPlugin.QuickFixes.CreateMissingStep
{
    public class CreateSpecflowStepFromUsageAction : IBulbAction
    {
        public string Text => "Create step";
        private readonly SpecflowStepDeclarationReference _reference;
        private readonly IMenuModalUtil _menuModalUtil;
        private readonly SpecflowStepsDefinitionsCache _specflowStepsDefinitionsCache;
        private readonly ICreateSpecFlowStepUtil _createSpecFlowStepUtil;

        public CreateSpecflowStepFromUsageAction(
            SpecflowStepDeclarationReference reference,
            IMenuModalUtil menuModalUtil,
            SpecflowStepsDefinitionsCache specflowStepsDefinitionsCache,
            ICreateSpecFlowStepUtil createSpecFlowStepUtil)
        {
            _reference = reference;
            _menuModalUtil = menuModalUtil;
            _specflowStepsDefinitionsCache = specflowStepsDefinitionsCache;
            _createSpecFlowStepUtil = createSpecFlowStepUtil;
        }

        public void Execute(ISolution solution, ITextControl textControl)
        {
            var availableSteps = _specflowStepsDefinitionsCache.GetBindingTypes(_reference.GetElement().GetPsiModule());

            var filesPerClasses = new OneToSetMap<string, SpecflowStepsDefinitionsCache.AvailableBindingClass>();
            foreach (var availableBindingClass in availableSteps)
                filesPerClasses.Add(availableBindingClass.ClassClrName, availableBindingClass);

            var actions = new List<CreateStepMenuAction>();

            actions.AddRange(filesPerClasses.Select(availableBindingClass => new CreateStepMenuAction(
                new RichText(availableBindingClass.Key, DeclaredElementPresenterTextStyles.ParameterInfo.GetStyle(DeclaredElementPresentationPartKind.Type)),
                PsiSymbolsThemedIcons.Class.Id,
                () => OpenPartialClassFileSelectionModal(textControl, availableBindingClass.Value, _reference.GetStepKind(), _reference.GetStepText()),
                new ClrTypeName(availableBindingClass.Key).GetNamespaceName()))
            );

            _menuModalUtil.OpenSelectStepClassMenu(actions, "Where to create the step ?", textControl.PopupWindowContextFactory.ForCaret());
        }

        private void OpenPartialClassFileSelectionModal(ITextControl textControl, ISet<SpecflowStepsDefinitionsCache.AvailableBindingClass> availableBindingClasses, GherkinStepKind getStepKind, string getStepText)
        {
            var actions = new List<CreateStepMenuAction>();

            actions.AddRange(availableBindingClasses.Select(availableBindingClass => new CreateStepMenuAction(
                new RichText(availableBindingClass.SourceFile.DisplayName),
                PsiCSharpThemedIcons.Csharp.Id,
                () =>
                {
                    var addedDeclaration = _createSpecFlowStepUtil.AddSpecflowStep(
                        availableBindingClass.NotNull().SourceFile,
                        availableBindingClass.ClassClrName,
                        getStepKind,
                        getStepText,
                        _reference.GetGherkinFileCulture(),
                        _reference.GetElement().Children<GherkinPystring>().Any(),
                        _reference.GetElement().Children<GherkinTable>().Any()
                    );

                    if (addedDeclaration != null)
                    {
                        var invocationExpression = addedDeclaration.GetChildrenInSubtrees<IInvocationExpression>().FirstOrDefault();
                        if (invocationExpression != null)
                            invocationExpression.NavigateToNode(true);
                        else
                            addedDeclaration.NavigateToNode(true);
                    }
                }
            )));

            _menuModalUtil.OpenSelectStepClassMenu(actions, "Where to create the step ?", textControl.PopupWindowContextFactory.ForCaret());
        }
    }
}