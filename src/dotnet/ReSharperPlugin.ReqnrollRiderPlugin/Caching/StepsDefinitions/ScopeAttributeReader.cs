#nullable enable
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Helpers;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;

[ShellComponent]
public class ScopeAttributeUtil
{
    public IReadOnlyList<ReqnrollStepScope>? GetScopes(IClassDeclaration classDeclaration)
    {
        var attributeInstances = classDeclaration.DeclaredElement?.GetAttributeInstances(AttributesSource.Self);
        if (attributeInstances == null)
            return null;

        return GetScopesFromAttributes(attributeInstances);
    }

    public IReadOnlyList<ReqnrollStepScope>? GetScopes(IMethodDeclaration methodDeclaration)
    {
        var attributeInstances = methodDeclaration.DeclaredElement?.GetAttributeInstances(AttributesSource.Self);
        if (attributeInstances == null)
            return null;

        return GetScopesFromAttributes(attributeInstances);
    }

    public IReadOnlyList<ReqnrollStepScope>? GetScopesFromAttributes(ICollection<IAttributeInstance> attributeInstances)
    {
        List<ReqnrollStepScope>? scopes = null;
        foreach (var attribute in attributeInstances)
        {
            if (!ReqnrollAttributeHelper.IsScopeAttribute(attribute.GetAttributeType().GetClrName().FullName))
                continue;

            string? feature = null;
            string? tag = null;
            string? scenario = null;

            foreach (var (name, value) in attribute.NamedParameters())
            {
                if (!value.IsConstant)
                    continue;
                switch (name)
                {
                    case "Feature":
                        feature = value.ConstantValue.StringValue;
                        break;
                    case "Tag":
                        tag = value.ConstantValue.StringValue;
                        break;
                    case "Scenario":
                        scenario = value.ConstantValue.StringValue;
                        break;
                }
            }

            scopes ??= new List<ReqnrollStepScope>(attributeInstances.Count);
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
