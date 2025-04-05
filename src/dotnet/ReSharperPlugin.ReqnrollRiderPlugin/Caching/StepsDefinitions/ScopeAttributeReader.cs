#nullable enable
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Helpers;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;

[ShellComponent]
public class ScopeAttributeUtil
{
    public IReadOnlyList<ReqnrollStepScope>? GetScopes(IClassDeclaration classDeclaration)
    {
        return GetScopesFromAttributes(classDeclaration.Attributes);
    }

    public IReadOnlyList<ReqnrollStepScope>? GetScopes(IMethodDeclaration methodDeclaration)
    {
        return GetScopesFromAttributes(methodDeclaration.Attributes);
    }

    private IReadOnlyList<ReqnrollStepScope>? GetScopesFromAttributes(TreeNodeCollection<IAttribute> attributes)
    {
        List<ReqnrollStepScope>? scopes = null;
        foreach (var attribute in attributes)
        {
            if (!ReqnrollAttributeHelper.IsScopeAttributeShortName(attribute.Name.ShortName))
                continue;

            string? feature = null;
            string? tag = null;
            string? scenario = null;

            foreach (var propertyAssignment in attribute.Children<IPropertyAssignment>())
            {
                if (propertyAssignment.Source is not ICSharpLiteralExpression source || !source.IsConstantValue())
                    continue;
                if (!source.ConstantValue.IsString())
                    continue;
                var name = propertyAssignment.PropertyNameIdentifier.Name;
                var value = source.ConstantValue.StringValue;
                switch (name)
                {
                    case "Feature":
                        feature = value;
                        break;
                    case "Tag":
                        tag = value;
                        break;
                    case "Scenario":
                        scenario = value;
                        break;
                }
            }
            scopes ??= new List<ReqnrollStepScope>(attributes.Count);
            scopes.Add(new ReqnrollStepScope(feature, scenario, tag));
        }

        return scopes;
    }


    public IReadOnlyList<ReqnrollStepScope>? GetScopesFromAttributes(
        ICollection<IMetadataCustomAttribute> attributeInstances
    )
    {

        List<ReqnrollStepScope>? scopes = null;
        foreach (var attribute in attributeInstances)
        {
            if (!ReqnrollAttributeHelper.IsScopeAttribute(attribute.UsedConstructorSpecification?.OwnerType?.FullName)
                && ReqnrollAttributeHelper.IsScopeAttribute(attribute.UsedConstructor?.DeclaringType?.FullyQualifiedName))
                continue;

            string? feature = null;
            string? tag = null;
            string? scenario = null;

            foreach (var namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Value.Value is not string value)
                    continue;

                switch (namedArgument.MemberName)
                {
                    case "Feature":
                        feature = value;
                        break;
                    case "Tag":
                        tag = value;
                        break;
                    case "Scenario":
                        scenario = value;
                        break;
                }
            }

            scopes ??= new List<ReqnrollStepScope>(attributeInstances.Count);
            scopes.Add(new ReqnrollStepScope(feature, scenario, tag));
        }

        return scopes;
    }
}