using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Parts;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Naming.Extentions;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.ReSharper.Psi.Naming.Settings;
using JetBrains.ReSharper.Psi.Transactions;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Analytics;
using ReSharperPlugin.ReqnrollRiderPlugin.Extensions;
using ReSharperPlugin.ReqnrollRiderPlugin.Helpers;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils.Steps;

namespace ReSharperPlugin.ReqnrollRiderPlugin.QuickFixes.CreateMissingStep
{
    public interface ICreateReqnrollStepUtil
    {
        [CanBeNull] IClassMemberDeclaration AddReqnrollStep(
            IPsiSourceFile targetFile,
            string classClrName,
            GherkinStepKind stepKind,
            string stepText,
            CultureInfo getGherkinFileCulture,
            bool hasMultilineParameter,
            bool hasTableParameter
        );
    }

    [PsiSharedComponent(Instantiation.DemandAnyThreadSafe)]
    public class CreateReqnrollStepUtil(IStepDefinitionBuilder stepDefinitionBuilder)
        : ICreateReqnrollStepUtil
    {

        public IClassMemberDeclaration AddReqnrollStep(
            IPsiSourceFile targetFile,
            string classClrName,
            GherkinStepKind stepKind,
            string stepText,
            CultureInfo getGherkinFileCulture,
            bool hasMultilineParameter,
            bool hasTableParameter
        )
        {
            var cSharpFile = targetFile.GetProject().GetCSharpFile(targetFile.DisplayName.Substring(targetFile.DisplayName.LastIndexOf('>') + 2));
            if (cSharpFile == null)
                return null;

            using (CompilationContextCookie.GetOrCreate(targetFile.GetResolveContext()))
            {
                foreach (var classDeclaration in cSharpFile.GetChildrenInSubtrees<IClassDeclaration>())
                {
                    if (classDeclaration.CLRName != classClrName)
                        continue;
                    if (classDeclaration.DeclaredElement?.GetAttributeInstances(AttributesSource.Self).All(x => x.GetAttributeType().GetClrName().FullName != "Reqnroll.Binding") != true)
                        continue;

                    var factory = CSharpElementFactory.GetInstance(classDeclaration);
                    var methodName = stepDefinitionBuilder.GetStepDefinitionMethodNameFromStepText(stepKind, stepText, getGherkinFileCulture);
                    methodName = cSharpFile.GetPsiServices().Naming.Suggestion.GetDerivedName(methodName, NamedElementKinds.Method, ScopeKind.Common, CSharpLanguage.Instance.NotNull(), new SuggestionOptions(), targetFile);
                    var parameters = stepDefinitionBuilder.GetStepDefinitionParameters(stepText, getGherkinFileCulture);
                    var pattern = stepDefinitionBuilder.GetPattern(stepText, getGherkinFileCulture);
                    var attributeType = CSharpTypeFactory.CreateType(ReqnrollAttributeHelper.GetAttributeClrName(stepKind), classDeclaration.GetPsiModule());
                    var formatString = $"[$0(\"$1\")] public void {methodName}() {{ScenarioContext.StepIsPending();}}";
                    var methodDeclaration = factory.CreateTypeMemberDeclaration(formatString, attributeType, pattern) as IMethodDeclaration;
                    if (methodDeclaration == null)
                        continue;
                    var psiModule = classDeclaration.GetPsiModule();

                    foreach (var (parameterName, parameterType) in parameters)
                        methodDeclaration.AddParameterDeclarationBefore(ParameterKind.VALUE, CSharpTypeFactory.CreateType(parameterType, psiModule), parameterName, null);

                    if (hasMultilineParameter)
                        methodDeclaration.AddParameterDeclarationBefore(ParameterKind.VALUE, CSharpTypeFactory.CreateType("string", psiModule), "multilineText", null);

                    if (hasTableParameter)
                        methodDeclaration.AddParameterDeclarationBefore(ParameterKind.VALUE, CSharpTypeFactory.CreateType("Reqnroll.Table", psiModule), "table", null);

                    IClassMemberDeclaration insertedDeclaration;
                    using (new PsiTransactionCookie(classDeclaration.GetPsiServices(), DefaultAction.Commit, "Generate reqnroll step"))
                    {
                        insertedDeclaration = classDeclaration.AddClassMemberDeclaration((IClassMemberDeclaration)methodDeclaration);
                    }

                    var analyticsTransmitter = targetFile.GetSolution().GetComponent<IAnalyticsTransmitter>();
                    analyticsTransmitter.TransmitRuntimeEvent(new GenericEvent("RiderActionExecuted", new Dictionary<string, string>
                            {{"Type", "CreateStepBinding"}}
                        )
                    );

                    return insertedDeclaration;
                }
            }

            return null;
        }
    }
}