using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.UI.Icons.CommonThemedIcons;
using JetBrains.Diagnostics;
using JetBrains.DocumentManagers.impl;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions.CreateDeclaration;
using JetBrains.ReSharper.Feature.Services.Intentions.DataProviders;
using JetBrains.ReSharper.Feature.Services.Intentions.Impl.DeclarationBuilders;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Resources;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Resources;
using JetBrains.ReSharper.Psi.Transactions;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.UI.RichText;
using JetBrains.Util;
using JetBrains.Util.Media;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Extensions;
using ReSharperPlugin.ReqnrollRiderPlugin.Helpers;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.References;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils;

namespace ReSharperPlugin.ReqnrollRiderPlugin.QuickFixes.CreateMissingStep
{
    public class CreateReqnrollStepFromUsageAction : IBulbAction
    {
        public string Text => "Create step";
        private readonly ReqnrollStepDeclarationReference _reference;
        private readonly IMenuModalUtil _menuModalUtil;
        private readonly ICreateStepClassDialogUtil _createStepClassDialogUtil;
        private readonly ICreateStepPartialClassFile _createStepPartialClassFile;
        private readonly ReqnrollStepsDefinitionsCache _reqnrollStepsDefinitionsCache;
        private readonly ICreateReqnrollStepUtil _createReqnrollStepUtil;

        public CreateReqnrollStepFromUsageAction(
            ReqnrollStepDeclarationReference reference,
            IMenuModalUtil menuModalUtil,
            ICreateStepClassDialogUtil createStepClassDialogUtil,
            ICreateStepPartialClassFile createStepPartialClassFile,
            ReqnrollStepsDefinitionsCache reqnrollStepsDefinitionsCache,
            ICreateReqnrollStepUtil createReqnrollStepUtil)
        {
            _reference = reference;
            _menuModalUtil = menuModalUtil;
            _createStepClassDialogUtil = createStepClassDialogUtil;
            _createStepPartialClassFile = createStepPartialClassFile;
            _reqnrollStepsDefinitionsCache = reqnrollStepsDefinitionsCache;
            _createReqnrollStepUtil = createReqnrollStepUtil;
        }

        public void Execute(ISolution solution, ITextControl textControl)
        {
            var availableSteps = _reqnrollStepsDefinitionsCache.GetBindingTypes(_reference.GetElement().GetPsiModule());

            var filesPerClasses = new OneToSetMap<string, ReqnrollStepsDefinitionsCache.AvailableBindingClass>();
            foreach (var availableBindingClass in availableSteps)
                filesPerClasses.Add(availableBindingClass.ClassClrName, availableBindingClass);

            var actions = new List<CreateStepMenuAction>();

            actions.Add(new CreateStepMenuAction("Create new binding class", CommonThemedIcons.Create.Id, () =>
            {
                var (project, defaultFolder) = SelectDefaultFolderForNewFile();
                _createStepClassDialogUtil.OpenCreateClassDialog(solution, project, defaultFolder, (className, path, isPartial) =>
                {
                    using (ReadLockCookie.Create())
                    {
                        var classDeclaration = CreateCSharpFile(solution, className, path, isPartial);
                        if (classDeclaration != null)
                            AddReqnrollStep(classDeclaration.GetSourceFile(), classDeclaration.CLRName);
                    }
                });
            }));

            actions.AddRange(filesPerClasses.OrderBy(x => x.Key.Split('.').Last()).Select(availableBindingClass =>
                {
                    var richText = new RichText(availableBindingClass.Key.Split('.').Last(), DeclaredElementPresenterTextStyles.ParameterInfo.GetStyle(DeclaredElementPresentationPartKind.Type));
                    richText.Append($" (in {new ClrTypeName(availableBindingClass.Key).GetNamespaceName()})",
                        TextStyle.FromForeColor(JetRgbaColor.FromArgb(byte.MaxValue,124,129,144)));
                    return new CreateStepMenuAction(
                        richText,
                        PsiSymbolsThemedIcons.Class.Id,
                        () => OpenPartialClassFileSelectionModal(textControl, solution, availableBindingClass.Key, availableBindingClass.Value)
                        );
                })
            );

            _menuModalUtil.OpenSelectStepClassMenu(actions, "Where to create the step ?", textControl.PopupWindowContextFactory.ForCaret());
        }

        private (IProject, VirtualFileSystemPath) SelectDefaultFolderForNewFile()
        {
            var gherkinStep = _reference.GetElement();
            var nearestStep = TreeNodeHelper.GetPreviousNodeOfType<GherkinStep>(gherkinStep)
                              ?? TreeNodeHelper.GetNextNodeOfType<GherkinStep>(gherkinStep);
            var reference = nearestStep?.GetFirstClassReferences().FirstOrDefault();
            if (reference != null && reference.CheckResolveResult() == ResolveErrorType.OK)
            {
                var file = reference.Resolve().Result.DeclaredElement?.GetSourceFiles().FirstOrDefault();
                if (file != null)
                    return (file.GetProject(), file.GetLocation().Parent);
            }

            return (_reference.GetProject(), _reference.GetElement().GetProject()?.Location);
        }

        private void OpenPartialClassFileSelectionModal(ITextControl textControl, ISolution solution, string fullClassName, ISet<ReqnrollStepsDefinitionsCache.AvailableBindingClass> availableBindingClasses)
        {
            var actions = new List<CreateStepMenuAction>();

            if (IsBindingTypePartial(fullClassName, availableBindingClasses))
            {
                actions.Add(new CreateStepMenuAction("Create new file", CommonThemedIcons.Create.Id, () =>
                {
                    _createStepPartialClassFile.OpenCreatePartialClassFileDialog(availableBindingClasses.First().SourceFile, (path, filename) =>
                    {
                        using (ReadLockCookie.Create())
                        {
                            var classDeclaration = CreatePartialPartCSharpFile(solution, fullClassName, path, filename);
                            if (classDeclaration != null)
                                AddReqnrollStep(classDeclaration.GetSourceFile(), classDeclaration.CLRName);
                        }
                    });
                }));
            }

            actions.AddRange(availableBindingClasses.Select(availableBindingClass => new CreateStepMenuAction(
                new RichText(availableBindingClass.SourceFile.DisplayName),
                PsiCSharpThemedIcons.Csharp.Id,
                () => { AddReqnrollStep(availableBindingClass.NotNull().SourceFile, availableBindingClass.ClassClrName); }
            )));

            _menuModalUtil.OpenSelectStepClassMenu(actions, "Where to create the step ?", textControl.PopupWindowContextFactory.ForCaret());
        }

        private static bool IsBindingTypePartial(string fullClassName, ISet<ReqnrollStepsDefinitionsCache.AvailableBindingClass> availableBindingClasses)
        {
            if (availableBindingClasses.Count != 1)
                return true;

            var psiSourceFile = availableBindingClasses.First().SourceFile;
            var @class = psiSourceFile.GetPsiServices().Symbols
                .GetTypesAndNamespacesInFile(psiSourceFile)
                .OfType<IClass>()
                .FirstOrDefault(x => x.GetClrName().FullName == fullClassName);
            if (@class == null)
                return false;
            if (@class.GetDeclarations().FirstOrDefault() is not IClassDeclaration classDeclaration)
                return false;
            return classDeclaration.IsPartial;
        }

        [CanBeNull]
        protected IClassDeclaration CreateCSharpFile(
            ISolution solution,
            string className,
            string path,
            bool isPartial
        )
        {
            var projectFolder = GetOrCreateFolder(solution, path);
            if (projectFolder == null)
                return null;

            var project = projectFolder.GetProject().NotNull();

            var createNewFileTarget = new CreateNewFileTarget(
                _reference.GetTreeNode(),
                project,
                projectFolder.Location,
                projectFolder.Name,
                className,
                CSharpProjectFileType.Instance,
                null,
                CSharpLanguage.Instance
            );
            
            createNewFileTarget.PreExecute();
            if (createNewFileTarget.GetTargetDeclarationFile() == null)
                return null;

            using (new PsiTransactionCookie(solution.GetPsiServices(), DefaultAction.Commit, "Creating new step class"))
            {
                var result = ClassDeclarationBuilder.CreateClass(new CreateClassDeclarationContext
                {
                    ClassName = className,
                    IsPartial = isPartial,
                    AccessRights = AccessRights.PUBLIC,
                    IsStatic = false,
                    Target = createNewFileTarget
                });
                if (result?.ResultDeclaration is not IClassDeclaration classDeclaration)
                    return null;

                using (CompilationContextCookie.GetOrCreate(project.GetResolveContext()))
                {
                    var bindingAttributeType = TypeFactory.CreateTypeByCLRName(ReqnrollAttributeHelper.BindingAttribute.First().FullName, _reference.GetTreeNode().GetPsiModule());
                    var reqnrollBindingAttribute = CSharpElementFactory.GetInstance(_reference.GetElement()).CreateAttribute(bindingAttributeType.GetTypeElement().NotNull());
                    classDeclaration.AddAttributeAfter(reqnrollBindingAttribute, null);
                }

                var expectedNamespace = classDeclaration.GetSourceFile().ToProjectFile()?.CalculateExpectedNamespace(CSharpLanguage.Instance.NotNull());
                if (expectedNamespace != null)
                {
                    var cSharpElementFactory = CSharpElementFactory.GetInstance(classDeclaration);
                    var cSharpNamespaceDeclaration = cSharpElementFactory.CreateNamespaceDeclaration(expectedNamespace, false);
                    classDeclaration.OwnerNamespaceDeclaration.SetQualifiedName(cSharpNamespaceDeclaration.QualifiedName);
                }
                return classDeclaration;
            }
        }

        [CanBeNull]
        protected IClassDeclaration CreatePartialPartCSharpFile(
            ISolution solution,
            string fullClassName,
            string path,
            string filename
        )
        {
            var projectFolder = GetOrCreateFolder(solution, path);
            if (projectFolder == null)
                return null;

            var clrTypeName = new ClrTypeName(fullClassName);
            var project = projectFolder.GetProject().NotNull();

            var createNewFileTarget = new CreateNewFileTarget(
                _reference.GetTreeNode(),
                project,
                projectFolder.Location,
                string.Join(".", clrTypeName.NamespaceNames),
                filename,
                CSharpProjectFileType.Instance,
                null,
                CSharpLanguage.Instance
            );
            createNewFileTarget.PreExecute();

            using (new PsiTransactionCookie(solution.GetPsiServices(), DefaultAction.Commit, "Creating new step class"))
            {
                var result = ClassDeclarationBuilder.CreateClass(new CreateClassDeclarationContext
                {
                    ClassName = clrTypeName.ShortName,
                    IsPartial = true,
                    AccessRights = AccessRights.PUBLIC,
                    IsStatic = false,
                    Target = createNewFileTarget
                });

                if (result?.ResultDeclaration is not IClassDeclaration classDeclaration)
                    return null;

                return classDeclaration;
            }
        }

        private void AddReqnrollStep(IPsiSourceFile psiSourceFile, string classClrName)
        {
            var addedDeclaration = _createReqnrollStepUtil.AddReqnrollStep(
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

        private IProjectFolder GetOrCreateFolder(ISolution solution, string path)
        {
            // From: ChooseProjectFolderController.ParseFolderName
            var names = path.TrimFromEnd("\\").TrimFromEnd("/").Split('\\', '/');
            if (names.Length == 0)
                return null;
            var matchingProjects = solution.GetTopLevelProjects()
                .Where(p => string.Equals(p.Name, names[0], StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matchingProjects.Count != 1)
                return null;
            var project = matchingProjects.First();
            return project.GetOrCreateProjectFolder(project.Location.Combine(Path.Combine(names.Skip(1).ToArray())));
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