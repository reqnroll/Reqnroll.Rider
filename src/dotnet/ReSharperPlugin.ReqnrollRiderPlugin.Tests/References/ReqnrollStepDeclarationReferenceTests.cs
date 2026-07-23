using System.Reflection;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Impl.Reflection2;
using NUnit.Framework;
using ReSharperPlugin.ReqnrollRiderPlugin.References;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Tests.References;

public class ReqnrollStepDeclarationReferenceTests
{
    [Test]
    public void TestLoadAssemblyFileRequestsTheAssemblyFile()
    {
        var loader = new PsiAssemblyFileLoaderStub();

        var result = InvokeLoadAssemblyFile(loader, null);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Null);
            Assert.That(loader.LoadCount, Is.EqualTo(1));
            Assert.That(loader.LoadOptions, Is.EqualTo(PsiAssemblyLoadOptions.LoadAssemblyFile));
        });
    }

    private static IPsiAssemblyFile InvokeLoadAssemblyFile(
        IPsiAssemblyFileLoader psiAssemblyFileLoader,
        IPsiAssembly psiAssembly)
    {
        var method = typeof(ReqnrollStepDeclarationReference).GetMethod(
            "LoadAssemblyFile",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        return (IPsiAssemblyFile)method!.Invoke(
            null,
            [psiAssemblyFileLoader, psiAssembly]);
    }

    private sealed class PsiAssemblyFileLoaderStub : IPsiAssemblyFileLoader
    {
        public int LoadCount { get; private set; }
        public PsiAssemblyLoadOptions LoadOptions { get; private set; }

        public void LoadAssembly<TContext>(
            IPsiAssembly assembly,
            PsiAssemblyLoadOptions options,
            TContext context,
            PsiAssemblyLoadHandler<TContext> handler)
        {
            LoadCount++;
            LoadOptions = options;
            handler(assembly, null, null, context);
        }
    }
}