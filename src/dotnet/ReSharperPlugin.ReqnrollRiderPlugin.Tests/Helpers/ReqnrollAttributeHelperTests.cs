using JetBrains.Metadata.Reader.Impl;
using NUnit.Framework;
using ReSharperPlugin.ReqnrollRiderPlugin.Helpers;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Tests.Helpers;

public class ReqnrollAttributeHelperTests
{
    [TestCase("Reqnroll.GivenAttribute", GherkinStepKind.Given)]
    [TestCase("TechTalk.SpecFlow.GivenAttribute", GherkinStepKind.Given)]
    [TestCase("Bobcat.GivenAttribute", GherkinStepKind.Given)]
    [TestCase("Reqnroll.WhenAttribute", GherkinStepKind.When)]
    [TestCase("Bobcat.WhenAttribute", GherkinStepKind.When)]
    [TestCase("Reqnroll.ThenAttribute", GherkinStepKind.Then)]
    [TestCase("Bobcat.ThenAttribute", GherkinStepKind.Then)]
    [TestCase("Bobcat.CheckAttribute", GherkinStepKind.Then)]
    [TestCase("Reqnroll.StepDefinitionAttribute", GherkinStepKind.Given)]
    [TestCase("Reqnroll.StepDefinitionAttribute", GherkinStepKind.When)]
    [TestCase("Reqnroll.StepDefinitionAttribute", GherkinStepKind.Then)]
    public void TestIsAttributeForKindRecognisesStepAttributesByFullName(string fullName, GherkinStepKind stepKind)
    {
        Assert.That(ReqnrollAttributeHelper.IsAttributeForKind(stepKind, fullName), Is.True);
        Assert.That(ReqnrollAttributeHelper.IsStepAttribute(fullName), Is.True);
    }

    [TestCase("Bobcat.GivenAttribute", GherkinStepKind.Then)]
    [TestCase("Bobcat.CheckAttribute", GherkinStepKind.Given)]
    [TestCase("Bobcat.CheckAttribute", GherkinStepKind.When)]
    [TestCase("Reqnroll.GivenAttribute", GherkinStepKind.When)]
    public void TestIsAttributeForKindIsPerKind(string fullName, GherkinStepKind stepKind)
    {
        Assert.That(ReqnrollAttributeHelper.IsAttributeForKind(stepKind, fullName), Is.False);
    }

    [TestCase("Bobcat.BindingAttribute")]
    [TestCase("Bobcat.TableGrammarAttribute")]
    [TestCase("Reqnroll.BindingAttribute")]
    [TestCase("System.ObsoleteAttribute")]
    public void TestIsStepAttributeRejectsOtherAttributes(string fullName)
    {
        Assert.That(ReqnrollAttributeHelper.IsStepAttribute(fullName), Is.False);
    }

    [TestCase("Given", GherkinStepKind.Given)]
    [TestCase("When", GherkinStepKind.When)]
    [TestCase("Then", GherkinStepKind.Then)]
    [TestCase("Check", GherkinStepKind.Then)]
    [TestCase("StepDefinition", GherkinStepKind.Then)]
    public void TestIsAttributeForKindUsingShortNameRecognisesStepAttributes(string shortName, GherkinStepKind stepKind)
    {
        Assert.That(ReqnrollAttributeHelper.IsAttributeForKindUsingShortName(stepKind, shortName), Is.True);
        Assert.That(ReqnrollAttributeHelper.IsStepAttributeShortName(shortName), Is.True);
    }

    [TestCase("Check", GherkinStepKind.Given)]
    [TestCase("Check", GherkinStepKind.When)]
    [TestCase("Binding", GherkinStepKind.Given)]
    [TestCase("TableGrammar", GherkinStepKind.Given)]
    public void TestIsAttributeForKindUsingShortNameRejectsOtherAttributes(string shortName, GherkinStepKind stepKind)
    {
        Assert.That(ReqnrollAttributeHelper.IsAttributeForKindUsingShortName(stepKind, shortName), Is.False);
    }

    [Test]
    public void TestIsStepAttributeShortNameRejectsNonStepAttributes()
    {
        Assert.That(ReqnrollAttributeHelper.IsStepAttributeShortName("Binding"), Is.False);
        Assert.That(ReqnrollAttributeHelper.IsStepAttributeShortName("TableGrammar"), Is.False);
    }

    [Test]
    public void TestGeneratedAttributeNamesStayReqnroll()
    {
        Assert.That(ReqnrollAttributeHelper.GetAttributeClrName(GherkinStepKind.Given), Is.EqualTo("Reqnroll.GivenAttribute"));
        Assert.That(ReqnrollAttributeHelper.GetAttributeClrName(GherkinStepKind.When), Is.EqualTo("Reqnroll.WhenAttribute"));
        Assert.That(ReqnrollAttributeHelper.GetAttributeClrName(GherkinStepKind.Then), Is.EqualTo("Reqnroll.ThenAttribute"));
    }

    [TestCase("Reqnroll.GivenAttribute", GherkinStepKind.Given)]
    [TestCase("TechTalk.SpecFlow.WhenAttribute", GherkinStepKind.When)]
    [TestCase("Reqnroll.ThenAttribute", GherkinStepKind.Then)]
    public void TestMethodNamingConventionAppliesToReqnrollAndSpecFlow(string fullName, GherkinStepKind stepKind)
    {
        Assert.That(ReqnrollAttributeHelper.GetAttributeStepKind(new ClrTypeName(fullName)), Is.EqualTo(stepKind));
    }

    [TestCase("Bobcat.GivenAttribute")]
    [TestCase("Bobcat.WhenAttribute")]
    [TestCase("Bobcat.ThenAttribute")]
    [TestCase("Bobcat.CheckAttribute")]
    [TestCase("System.ObsoleteAttribute")]
    public void TestMethodNamingConventionDoesNotApplyToBobcat(string fullName)
    {
        Assert.That(ReqnrollAttributeHelper.GetAttributeStepKind(new ClrTypeName(fullName)), Is.Null);
    }

    [Test]
    public void TestBindingAttributeIsUnchanged()
    {
        Assert.That(ReqnrollAttributeHelper.IsBindingAttribute("Reqnroll.BindingAttribute"), Is.True);
        Assert.That(ReqnrollAttributeHelper.IsBindingAttribute("TechTalk.SpecFlow.BindingAttribute"), Is.True);
        Assert.That(ReqnrollAttributeHelper.IsBindingAttribute("Bobcat.GivenAttribute"), Is.False);
    }
}
