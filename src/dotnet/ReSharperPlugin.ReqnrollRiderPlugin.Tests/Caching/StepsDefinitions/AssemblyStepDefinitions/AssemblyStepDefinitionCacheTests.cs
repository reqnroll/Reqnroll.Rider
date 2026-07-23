using JetBrains.ReSharper.Psi.Impl.Reflection2;
using NUnit.Framework;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Tests.Caching.StepsDefinitions.AssemblyStepDefinitions;

public class AssemblyStepDefinitionCacheTests
{
    [Test]
    public void TestCacheIsApplicableToAllAssemblies()
    {
        var cache = CreateCache();

        Assert.That(cache.IsApplicable(null), Is.True);
    }

    [Test]
    public void TestCacheRequestsMetadataAndTypesWhenBuilding()
    {
        var cache = CreateCache();

        var parameters = cache.GetBuildParameters(null);

        Assert.That(parameters.AssemblyLoadOptions, Is.EqualTo(
            PsiAssemblyLoadOptions.LoadMetadataAssembly |
            PsiAssemblyLoadOptions.LoadMetadataTypes));
    }

    [Test]
    public void TestBuildReturnsNullWithoutMetadata()
    {
        var cache = CreateCache();

        var result = cache.Build(null, null, null, null);

        Assert.That(result, Is.Null);
    }

    private static AssemblyStepDefinitionCache CreateCache()
    {
        return new AssemblyStepDefinitionCache(null, null);
    }
}