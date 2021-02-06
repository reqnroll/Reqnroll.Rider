using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.UI.Controls;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.Collections;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Impl;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Resources;
using JetBrains.ReSharper.Psi.CSharp.Tree;
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

namespace ReSharperPlugin.SpecflowRiderPlugin.QuickFixes.Actions
{
    public class CreateSpecflowStepFromUsageAction : IBulbAction
    {
        public string Text { get; } = "Create step";
        private readonly SpecflowStepDeclarationReference _reference;

        public CreateSpecflowStepFromUsageAction(
            SpecflowStepDeclarationReference reference
        )
        {
            _reference = reference;
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
                                                           var (classClrFullName, availableBindingClasses) = (KeyValuePair<string, ISet<SpecflowStepsDefinitionsCache.AvailableBindingClass>>) e.Key;

                                                           e.Descriptor.Icon = PsiSymbolsThemedIcons.Class.Id;
                                                           e.Descriptor.Style = MenuItemStyle.Enabled;
                                                           // FIXME: Use submenu instead of a second popup ? (I did not find how to get this working)
                                                           /*if (availableBindingClasses.Count > 1)
                                                               e.Descriptor.Style |= MenuItemStyle.CanExpand;*/

                                                           var clrTypeName = new ClrTypeName(classClrFullName);
                                                           e.Descriptor.Text = new RichText(clrTypeName.ShortName, DeclaredElementPresenterTextStyles.ParameterInfo.GetStyle(DeclaredElementPresentationPartKind.Type));
                                                           e.Descriptor.ShortcutText = clrTypeName.GetNamespaceName();
                                                       });
                    menu.ItemClicked.Advise(lifetime, key =>
                                                      {
                                                          var (_, availableBindingClasses) = (KeyValuePair<string, ISet<SpecflowStepsDefinitionsCache.AvailableBindingClass>>) key;
                                                          if (availableBindingClasses.Count == 1)
                                                          {
                                                              var availableBindingClass = availableBindingClasses.First();
                                                              AddSpecflowStep(availableBindingClass.SourceFile, availableBindingClass.ClassClrName, _reference.GetStepKind(), _reference.GetStepText());
                                                          }
                                                          else
                                                          {
                                                              OpenFileSelectionModal(jetPopupMenus, textControl, availableBindingClasses, _reference.GetStepKind(), _reference.GetStepText());
                                                          }
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

        private static void AddSpecflowStep(IPsiSourceFile targetFile, string classClrName, GherkinStepKind stepKind, string stepText)
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
                var (methodName, pattern, parametersTypes) = SpecflowStepHelper.GetMethodNameAndParameterFromStepText(stepKind, stepText, classDeclaration.GetPsiServices(), targetFile);
                var attributeType = CSharpTypeFactory.CreateType(SpecflowAttributeHelper.GetAttributeClrName(stepKind), classDeclaration.GetPsiModule());
                var methodDeclaration = factory.CreateTypeMemberDeclaration($"[$0(@\"{pattern.Replace("\"", "\"\"")}\")] public void {methodName}() {{ScenarioContext.StepIsPending();}}", attributeType) as IMethodDeclaration;
                if (methodDeclaration == null)
                    continue;
                var psiModule = classDeclaration.GetPsiModule();
                for (var i = parametersTypes.Length - 1; i >= 0; i--)
                    methodDeclaration.AddParameterDeclarationAfter(ParameterKind.VALUE, CSharpTypeFactory.CreateType(parametersTypes[i], psiModule), "p" + i, null);

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