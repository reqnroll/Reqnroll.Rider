using System;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Helpers;

public class ReqnrollAttributeHelper
{
    public static readonly ClrTypeName[] BindingAttribute = [new ClrTypeName("Reqnroll.BindingAttribute"), new ClrTypeName("TechTalk.SpecFlow.BindingAttribute")];
    public static readonly ClrTypeName[] ScopeAttribute = [new ClrTypeName("Reqnroll.ScopeAttribute"), new ClrTypeName("TechTalk.SpecFlow.ScopeAttribute")];
    public static readonly string[] ScopeAttributeShortName = ["Reqnroll.ScopeAttribute", "TechTalk.SpecFlow.ScopeAttribute", "ScopeAttribute"];
    public static readonly ClrTypeName[] StepDefinitionAttribute = [new ClrTypeName("Reqnroll.StepDefinitionAttribute"), new ClrTypeName("TechTalk.SpecFlow.StepDefinitionAttribute")];
    public static readonly ClrTypeName[] GivenAttribute = [new ClrTypeName("Reqnroll.GivenAttribute"), new ClrTypeName("TechTalk.SpecFlow.GivenAttribute"), new ClrTypeName("Bobcat.GivenAttribute")];
    public static readonly ClrTypeName[] WhenAttribute = [new ClrTypeName("Reqnroll.WhenAttribute"), new ClrTypeName("TechTalk.SpecFlow.WhenAttribute"), new ClrTypeName("Bobcat.WhenAttribute")];
    public static readonly ClrTypeName[] ThenAttribute = [new ClrTypeName("Reqnroll.ThenAttribute"), new ClrTypeName("TechTalk.SpecFlow.ThenAttribute"), new ClrTypeName("Bobcat.ThenAttribute"), new ClrTypeName("Bobcat.CheckAttribute")];
    public const string StepDefinitionAttributeShortName = "StepDefinition";
    public const string GivenAttributeShortName = "Given";
    public const string WhenAttributeShortName = "When";
    public const string ThenAttributeShortName = "Then";
    public const string CheckAttributeShortName = "Check";

    /// <summary>
    /// Bobcat (https://github.com/JasperFx/bobcat) declares steps with the same attribute names as Reqnroll in its own
    /// namespace, adds <c>[Check]</c> (a Then whose method returns a bool), and has no <c>[Binding]</c>: any class that
    /// declares steps is a step class. Its attributes live in the <c>Bobcat</c> assembly, so only an assembly that
    /// references it can carry them, which is what <see cref="CanContainBobcatSteps"/> keys off.
    /// </summary>
    public const string BobcatAssemblyName = "Bobcat";

    // Step attributes that mark a step but belong to a framework without Reqnroll's GivenXxx method-naming
    // convention. The "method name does not match pattern" inspection leaves these alone.
    private static readonly ClrTypeName[] StepAttributesWithoutNamingConvention = [new ClrTypeName("Bobcat.GivenAttribute"), new ClrTypeName("Bobcat.WhenAttribute"), new ClrTypeName("Bobcat.ThenAttribute"), new ClrTypeName("Bobcat.CheckAttribute")];

    public static string GetAttributeClrName(GherkinStepKind stepKind)
    {
        switch (stepKind)
        {
            case GherkinStepKind.Given:
                return GivenAttribute.First().FullName;
            case GherkinStepKind.When:
                return WhenAttribute.First().FullName;
            case GherkinStepKind.Then:
                return ThenAttribute.First().FullName;
            default:
                throw new ArgumentOutOfRangeException(nameof(stepKind), stepKind, null);
        }
    }

    /// <summary>
    /// The step kind of a step attribute whose framework follows Reqnroll's GivenXxx method-naming convention, or null.
    /// Used by the "method name does not match pattern" inspection; third-party step attributes return null.
    /// </summary>
    public static GherkinStepKind? GetAttributeStepKind(IClrTypeName typeName)
    {
        if (StepAttributesWithoutNamingConvention.Contains(typeName))
            return null;
        if (GivenAttribute.Contains(typeName))
            return GherkinStepKind.Given;
        if (WhenAttribute.Contains(typeName))
            return GherkinStepKind.When;
        if (ThenAttribute.Contains(typeName))
            return GherkinStepKind.Then;
        return null;
    }

    public static bool IsAttributeForKindUsingShortName(GherkinStepKind stepKind, string typeShortName)
    {
        if (typeShortName.Equals(StepDefinitionAttributeShortName))
            return true;
        if (stepKind == GherkinStepKind.Given && typeShortName.Equals(GivenAttributeShortName))
            return true;
        if (stepKind == GherkinStepKind.When && typeShortName.Equals(WhenAttributeShortName))
            return true;
        if (stepKind == GherkinStepKind.Then && (typeShortName.Equals(ThenAttributeShortName) || typeShortName.Equals(CheckAttributeShortName)))
            return true;
        return false;
    }

    public static bool IsAttributeForKind(GherkinStepKind stepKind, string fullName)
    {
        if (StepDefinitionAttribute.Any(attribute => attribute.FullName == fullName))
            return true;
        if (stepKind == GherkinStepKind.Given && GivenAttribute.Any(attribute => attribute.FullName == fullName))
            return true;
        if (stepKind == GherkinStepKind.When && WhenAttribute.Any(attribute => attribute.FullName == fullName))
            return true;
        if (stepKind == GherkinStepKind.Then && ThenAttribute.Any(attribute => attribute.FullName == fullName))
            return true;
        return false;
    }

    /// <summary>True when the full CLR name is any recognised step attribute, whatever its kind.</summary>
    public static bool IsStepAttribute(string fullName)
    {
        return IsAttributeForKind(GherkinStepKind.Given, fullName)
               || IsAttributeForKind(GherkinStepKind.When, fullName)
               || IsAttributeForKind(GherkinStepKind.Then, fullName);
    }

    /// <summary>True when the short name is any recognised step attribute, whatever its kind.</summary>
    public static bool IsStepAttributeShortName(string typeShortName)
    {
        return IsAttributeForKindUsingShortName(GherkinStepKind.Given, typeShortName)
               || IsAttributeForKindUsingShortName(GherkinStepKind.When, typeShortName)
               || IsAttributeForKindUsingShortName(GherkinStepKind.Then, typeShortName);
    }

    /// <summary>
    /// True when the assembly is Bobcat itself or references it, i.e. when any of its types can carry a
    /// <c>Bobcat.*</c> step attribute at all. Lets the assembly cache skip scanning the methods of every type in
    /// every other referenced assembly for the binding-less case.
    /// </summary>
    public static bool CanContainBobcatSteps(IMetadataAssembly assembly)
    {
        if (assembly.AssemblyName?.Name == BobcatAssemblyName)
            return true;
        return assembly.ReferencedAssembliesNames.Any(x => x.Name == BobcatAssemblyName);
    }

    public static bool IsBindingAttribute(string fullAttributeName)
    {
        return BindingAttribute.Any(x => x.FullName == fullAttributeName);
    }

    public static bool IsScopeAttribute(string fullAttributeName)
    {
        return ScopeAttribute.Any(x => x.FullName == fullAttributeName);
    }

    public static bool IsScopeAttributeShortName(string fullAttributeName)
    {
        return ScopeAttributeShortName.Any(x => x == fullAttributeName);
    }
}
