using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.UI.Controls;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.Collections;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Resources;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Naming.Extentions;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.ReSharper.Psi.Naming.Settings;
using JetBrains.ReSharper.Psi.Resources;
using JetBrains.ReSharper.Psi.Transactions;
using JetBrains.RiderTutorials.Utils;
using JetBrains.TextControl;
using JetBrains.UI.RichText;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Helpers;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.References;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.Steps;

namespace ReSharperPlugin.SpecflowRiderPlugin.QuickFixes.CreateMissingStep
{
    public class CreateSpecflowStepFromUsageAction : IBulbAction
    {
        public string Text { get; } = "Create step";
        private readonly SpecflowStepDeclarationReference _reference;
        private readonly IStepDefinitionBuilder _stepDefinitionBuilder;

        public CreateSpecflowStepFromUsageAction(
            SpecflowStepDeclarationReference reference,
            IStepDefinitionBuilder stepDefinitionBuilder
        )
        {
            _reference = reference;
            _stepDefinitionBuilder = stepDefinitionBuilder;
        }

        public void Execute(ISolution solution, ITextControl textControl)
        {
            var jetPopupMenus = solution.GetPsiServices().GetComponent<JetPopupMenus>();
            var cache = solution.GetComponent<SpecflowStepsDefinitionsCache>();
            jetPopupMenus.ShowModal(JetPopupMenu.ShowWhen.AutoExecuteIfSingleEnabledItem,
                (lifetime, menu) =>
                {
                    menu.Caption.Value = WindowlessControlAutomation.Create("Where to create the step ?");

                    var availableSteps = cache.GetBindingTypes(_reference.GetElement().GetPsiModule());
                    var filesPerClasses = new OneToSetMap<string, SpecflowStepsDefinitionsCache.AvailableBindingClass>();
                    foreach (var availableBindingClass in availableSteps)
                        filesPerClasses.Add(availableBindingClass.ClassClrName, availableBindingClass);

                    menu.ItemKeys.AddRange(filesPerClasses);
                    menu.DescribeItem.Advise(lifetime, e =>
                                                       {
                                                           var (classClrFullName, _) = (KeyValuePair<string, ISet<SpecflowStepsDefinitionsCache.AvailableBindingClass>>) e.Key;

                                                           e.Descriptor.Icon = PsiSymbolsThemedIcons.Class.Id;
                                                           e.Descriptor.Style = MenuItemStyle.Enabled;

                                                           var clrTypeName = new ClrTypeName(classClrFullName);
                                                           e.Descriptor.Text = new RichText(clrTypeName.ShortName, DeclaredElementPresenterTextStyles.ParameterInfo.GetStyle(DeclaredElementPresentationPartKind.Type));
                                                           e.Descriptor.ShortcutText = clrTypeName.GetNamespaceName();
                                                       });
                    menu.ItemClicked.Advise(lifetime, key =>
                                                      {
                                                          var (_, availableBindingClasses) = (KeyValuePair<string, ISet<SpecflowStepsDefinitionsCache.AvailableBindingClass>>) key;
                                                          OpenFileSelectionModal(jetPopupMenus, textControl, availableBindingClasses, _reference.GetStepKind(), _reference.GetStepText());
                                                      });
                    menu.PopupWindowContextSource = textControl.PopupWindowContextFactory.ForCaret();
                });
        }

        private void OpenFileSelectionModal(JetPopupMenus jetPopupMenus, ITextControl textControl, ISet<SpecflowStepsDefinitionsCache.AvailableBindingClass> availableBindingClasses, GherkinStepKind getStepKind, string getStepText)
        {
            jetPopupMenus.ShowModal(JetPopupMenu.ShowWhen.AutoExecuteIfSingleEnabledItem,
                (lifetime, menu) =>
                {
                    menu.Caption.Value = WindowlessControlAutomation.Create("Where to create the step ?");
                    menu.ItemKeys.AddRange(availableBindingClasses);
                    menu.DescribeItem.Advise(lifetime, e =>
                                                       {
                                                           var key = (SpecflowStepsDefinitionsCache.AvailableBindingClass) e.Key;
                                                           e.Descriptor.Icon = PsiCSharpThemedIcons.Csharp.Id;
                                                           e.Descriptor.Style = MenuItemStyle.Enabled;
                                                           e.Descriptor.Text = new RichText(key.SourceFile.DisplayName);
                                                       });
                    menu.ItemClicked.Advise(lifetime, e =>
                                                      {
                                                          var availableBindingClass = (SpecflowStepsDefinitionsCache.AvailableBindingClass) e;
                                                          AddSpecflowStep(availableBindingClass.SourceFile, availableBindingClass.ClassClrName, getStepKind, getStepText);
                                                      });
                    menu.PopupWindowContextSource = textControl.PopupWindowContextFactory.ForCaret();
                });
        }

        private void AddSpecflowStep(IPsiSourceFile targetFile, string classClrName, GherkinStepKind stepKind, string stepText)
        {
            var cSharpFile = targetFile.GetProject().GetCSharpFile(targetFile.DisplayName.Substring(targetFile.DisplayName.LastIndexOf('>') + 2));
            if (cSharpFile == null)
                return;

            foreach (var type in cSharpFile.GetChildrenInSubtrees<IClassDeclaration>())
            {
                if (!(type is IClassDeclaration classDeclaration))
                    continue;
                if (classDeclaration.CLRName != classClrName)
                    continue;
                if (classDeclaration.DeclaredElement?.GetAttributeInstances(AttributesSource.Self).All(x => x.GetAttributeType().GetClrName().FullName != "TechTalk.SpecFlow.Binding") != true)
                    continue;

                var factory = CSharpElementFactory.GetInstance(classDeclaration);
                var methodName = _stepDefinitionBuilder.GetStepDefinitionMethodNameFromStepText(stepKind, stepText, _reference.IsInsideScenarioOutline());
                methodName = cSharpFile.GetPsiServices().Naming.Suggestion.GetDerivedName(methodName, NamedElementKinds.Method, ScopeKind.Common, CSharpLanguage.Instance, new SuggestionOptions(), targetFile);
                var parameters = _stepDefinitionBuilder.GetStepDefinitionParameters(stepText, _reference.IsInsideScenarioOutline());
                var pattern = _stepDefinitionBuilder.GetPattern(stepText, _reference.IsInsideScenarioOutline());

                var attributeType = CSharpTypeFactory.CreateType(SpecflowAttributeHelper.GetAttributeClrName(stepKind), classDeclaration.GetPsiModule());
                var formatString = $"[$0(@\"$1\")] public void {methodName}() {{ScenarioContext.StepIsPending();}}";
                var methodDeclaration = factory.CreateTypeMemberDeclaration(formatString, attributeType, pattern.Replace("\"", "\"\"")) as IMethodDeclaration;
                if (methodDeclaration == null)
                    continue;
                var psiModule = classDeclaration.GetPsiModule();
                foreach (var (parameterName, parameterType) in parameters)
                    methodDeclaration.AddParameterDeclarationAfter(ParameterKind.VALUE, CSharpTypeFactory.CreateType(parameterType, psiModule), parameterName, null);

                IClassMemberDeclaration insertedDeclaration;
                using (new PsiTransactionCookie(type.GetPsiServices(), DefaultAction.Commit, "Generate specflow step"))
                {
                    insertedDeclaration = classDeclaration.AddClassMemberDeclaration((IClassMemberDeclaration) methodDeclaration);
                }

                var invocationExpression = insertedDeclaration.GetChildrenInSubtrees<IInvocationExpression>().FirstOrDefault();
                if (invocationExpression != null)
                    invocationExpression.NavigateToNode(true);
                else
                    insertedDeclaration.NavigateToNode(true);
            }
        }
    }
}