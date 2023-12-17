#nullable enable
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Helpers;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;

[ShellComponent]
public class ScopeAttributeUtil
{
    public IReadOnlyList<SpecflowStepScope>? GetScopes(IClassDeclaration classDeclaration)
    {
        var attributeInstances = classDeclaration.DeclaredElement?.GetAttributeInstances(AttributesSource.Self);
        if (attributeInstances == null)
            return null;

        return GetScopesFromAttributes(attributeInstances);
    }

    public IReadOnlyList<SpecflowStepScope>? GetScopes(IMethodDeclaration methodDeclaration)
    {
        var attributeInstances = methodDeclaration.DeclaredElement?.GetAttributeInstances(AttributesSource.Self);
        if (attributeInstances == null)
            return null;

        return GetScopesFromAttributes(attributeInstances);
    }

    public IReadOnlyList<SpecflowStepScope>? GetScopesFromAttributes(ICollection<IAttributeInstance> attributeInstances)
    {
        List<SpecflowStepScope>? scopes = null;
        foreach (var attribute in attributeInstances)
        {
            if (attribute.GetAttributeType().GetClrName().FullName != SpecflowAttributeHelper.ScopeAttribute.FullName)
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

            scopes ??= new List<SpecflowStepScope>(attributeInstances.Count);
            scopes.Add(new SpecflowStepScope(feature, scenario, tag));
        }

        return scopes;
    }


    public IReadOnlyList<SpecflowStepScope>? GetScopesFromAttributes(
        ICollection<IMetadataCustomAttribute> attributeInstances
    )
    {

        List<SpecflowStepScope>? scopes = null;
        foreach (var attribute in attributeInstances)
        {
            if (attribute.UsedConstructorSpecification?.OwnerType?.FullName != SpecflowAttributeHelper.ScopeAttribute.FullName
                && attribute.UsedConstructor?.DeclaringType?.FullyQualifiedName != SpecflowAttributeHelper.ScopeAttribute.FullName)
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

            scopes ??= new List<SpecflowStepScope>(attributeInstances.Count);
            scopes.Add(new SpecflowStepScope(feature, scenario, tag));
        }

        return scopes;
    }
}
