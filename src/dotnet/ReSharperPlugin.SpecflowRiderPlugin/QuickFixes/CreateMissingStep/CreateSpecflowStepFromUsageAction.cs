using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.UI.Icons.CommonThemedIcons;
using JetBrains.Diagnostics;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Properties;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions.CreateDeclaration;
using JetBrains.ReSharper.Feature.Services.Intentions.DataProviders;
using JetBrains.ReSharper.Feature.Services.Intentions.Impl.DeclarationBuilders;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Resources;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resources;
using JetBrains.ReSharper.Psi.Transactions;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Refactorings.Convert.Type2Partial.Data;
using JetBrains.ReSharper.Refactorings.CSharp.Type2Partial;
using JetBrains.TextControl;
using JetBrains.UI.RichText;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Extensions;
using ReSharperPlugin.SpecflowRiderPlugin.Helpers;
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
        private readonly ICreateStepClassDialogUtil _createStepClassDialogUtil;
        private readonly ICreateStepPartialClassFile _createStepPartialClassFile;
        private readonly SpecflowStepsDefinitionsCache _specflowStepsDefinitionsCache;
        private readonly ICreateSpecFlowStepUtil _createSpecFlowStepUtil;

        public CreateSpecflowStepFromUsageAction(
            SpecflowStepDeclarationReference reference,
            IMenuModalUtil menuModalUtil,
            ICreateStepClassDialogUtil createStepClassDialogUtil,
            ICreateStepPartialClassFile createStepPartialClassFile,
            SpecflowStepsDefinitionsCache specflowStepsDefinitionsCache,
            ICreateSpecFlowStepUtil createSpecFlowStepUtil)
        {
            _reference = reference;
            _menuModalUtil = menuModalUtil;
            _createStepClassDialogUtil = createStepClassDialogUtil;
            _createStepPartialClassFile = createStepPartialClassFile;
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

            actions.Add(new CreateStepMenuAction("Create new binding class", CommonThemedIcons.Create.Id, () =>
            {
                _createStepClassDialogUtil.OpenCreateClassDialog((className, path, isPartial) =>
                {
                    using (ReadLockCookie.Create())
                    {
                        var classDeclaration = CreateCSharpFile(solution, className, path, isPartial);
                        if (classDeclaration != null)
                            AddSpecFlowStep(classDeclaration.GetSourceFile(), classDeclaration.CLRName);
                    }
                });
            }));

            actions.AddRange(filesPerClasses.Select(availableBindingClass =>
                {
                    return new CreateStepMenuAction(
                        new RichText(availableBindingClass.Key, DeclaredElementPresenterTextStyles.ParameterInfo.GetStyle(DeclaredElementPresentationPartKind.Type)),
                        PsiSymbolsThemedIcons.Class.Id,
                        () => OpenPartialClassFileSelectionModal(textControl, solution, availableBindingClass.Key, availableBindingClass.Value),
                        new ClrTypeName(availableBindingClass.Key).GetNamespaceName());
                })
            );

            _menuModalUtil.OpenSelectStepClassMenu(actions, "Where to create the step ?", textControl.PopupWindowContextFactory.ForCaret());
        }

        private void OpenPartialClassFileSelectionModal(ITextControl textControl, ISolution solution, string fullClassName, ISet<SpecflowStepsDefinitionsCache.AvailableBindingClass> availableBindingClasses)
        {
            var actions = new List<CreateStepMenuAction>();


            actions.Add(new CreateStepMenuAction("Create new file", CommonThemedIcons.Create.Id, () =>
            {
                _createStepPartialClassFile.OpenCreatePartialClassFileDialog((filename) =>
                {

                    /*
                    var createdFile = this.Type2PartialManager.CreateProjectFile(this.Model, this.Model.FilePath, this.Helper[typeDeclaration.Language].GetExtension());
                    this.DataModel = new Type2PartialDataModel(this.Model.TypeDeclaration, this.Model.ExistingPart, this.Model.Elements, createdFile);
                    new CSharpType2PartialHelper().AddNewTypePart(new Type2PartialDataModel(, null, EmptyList<IDeclaration>.Instance,));*/
                    /*
                    var cle = new ClrTypeName("");
                    var referenceName = CSharpElementFactory.GetInstance(_reference.GetElement()).CreateReferenceName(fullClassName);
                    var declaredType = CSharpTypeFactory.CreateDeclaredType(referenceName, NullableAnnotation.Unknown);
                    declaredType.GetTypeElement();
                    var classDeclaration = CreatePartialClassFile(textControl. fullClassName,  className, path, true);
                    AddSpecFlowStep(classDeclaration.GetSourceFile(), classDeclaration.CLRName);*/
                });
            }));

            actions.AddRange(availableBindingClasses.Select(availableBindingClass => new CreateStepMenuAction(
                new RichText(availableBindingClass.SourceFile.DisplayName),
                PsiCSharpThemedIcons.Csharp.Id,
                () => { AddSpecFlowStep(availableBindingClass.NotNull().SourceFile, availableBindingClass.ClassClrName); }
            )));

            _menuModalUtil.OpenSelectStepClassMenu(actions, "Where to create the step ?", textControl.PopupWindowContextFactory.ForCaret());
        }

        [CanBeNull]
        protected IClassDeclaration CreateCSharpFile(
            ISolution solution,
            string className,
            string path,
            bool isPartial
        )
        {
            var targetFolder = FileSystemPath.Parse(path, FileSystemPathInternStrategy.INTERN);
            var project = FindProjectContainingPath(solution, targetFolder.FullPath);
            if (project == null)
                return null;

            // var nameSpace = ComputeNamespace(project, targetFolder, className, srcFeatureProjectFile, isPartial);

            var createNewFileTarget = new CreateNewFileTarget(
                _reference.GetTreeNode(),
                project,
                targetFolder,
                project.Name,
                className,
                CSharpProjectFileType.Instance,
                null,
                CSharpLanguage.Instance
            );
            createNewFileTarget.PreExecute();

            using (new PsiTransactionCookie(solution.GetPsiServices(), DefaultAction.Commit, "Creating new step class"))
            {
                var result = ClassDeclarationBuilder.CreateClass(new CreateClassDeclarationContext
                {
                    ClassName = className,
                    IsPartial = isPartial,
                    AccessRights = AccessRights.PUBLIC,
                    IsStatic = false,
                    IsInterface = false,
                    Target = createNewFileTarget
                });
                if (result?.ResultDeclaration is not IClassDeclaration classDeclaration)
                    return null;

                using (CompilationContextCookie.GetOrCreate(project.GetResolveContext()))
                {
                    var bindingAttributeType = TypeFactory.CreateTypeByCLRName(SpecflowAttributeHelper.BindingAttribute.FullName, _reference.GetTreeNode().GetPsiModule());
                    var specFlowBindingAttribute = CSharpElementFactory.GetInstance(_reference.GetElement()).CreateAttribute(bindingAttributeType.GetTypeElement());
                    classDeclaration.AddAttributeAfter(specFlowBindingAttribute, null);
                }

                var expectedNamespace = classDeclaration.GetSourceFile().ToProjectFile()?.CalculateExpectedNamespace(CSharpLanguage.Instance.NotNull());
                if (expectedNamespace != null)
                {
                    var cSharpElementFactory = CSharpElementFactory.GetInstance(classDeclaration);
                    var cSharpNamespaceDeclaration = cSharpElementFactory.CreateNamespaceDeclaration(expectedNamespace);
                    classDeclaration.OwnerNamespaceDeclaration.SetQualifiedName(cSharpNamespaceDeclaration.QualifiedName);
                }
                return classDeclaration;
            }
        }

        [CanBeNull]
        private IProject FindProjectContainingPath(ISolution solution, string path)
        {
            path += Path.DirectorySeparatorChar;
            return solution.GetAllProjects()
                .Where(p => p.IsOpened)
                .Where(p => path.Contains(p.Location.FullPath + Path.DirectorySeparatorChar))
                .OrderByDescending(p => p.Location.FullPath.Length)
                .FirstOrDefault();
        }

        private void AddSpecFlowStep(IPsiSourceFile psiSourceFile, string classClrName)
        {
            var addedDeclaration = _createSpecFlowStepUtil.AddSpecFlowStep(
                psiSourceFile,
                classClrName,
                _reference.GetStepKind(),
                _reference.GetStepText(),
                _reference.GetGherkinFileCulture(),
                _reference.GetElement().Children<GherkinPystring>().Any(),
                _reference.GetElement().Children<GherkinTable>().Any()
            );

            NavigateToAddedElement(addedDeclaration);
        }

        private static void NavigateToAddedElement([CanBeNull] ITreeNode addedDeclaration)
        {
            if (addedDeclaration != null)
            {
                var invocationExpression = addedDeclaration.GetChildrenInSubtrees<IInvocationExpression>().FirstOrDefault();
                if (invocationExpression != null)
                    invocationExpression.NavigateToNode(true);
                else
                    addedDeclaration.NavigateToNode(true);
            }
        }
    }
}