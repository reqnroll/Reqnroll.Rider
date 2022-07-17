using System;
using System.Threading;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Features.ReSpeller;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using JetBrains.Util;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

namespace ReSharperPlugin.SpecflowRiderPlugin.Tests
{
  [ZoneDefinition]
  public interface ISpecflowRiderPluginTestZone
    : ITestsEnvZone,
      IRequire<PsiFeatureTestZone>,
      IRequire<IReSpellerZone>
  {
  }

  [SetUpFixture]
  public class TestEnvironment : ExtensionTestEnvironmentAssembly<ISpecflowRiderPluginTestZone>
  {
    static TestEnvironment()
    {
      if (PlatformUtil.IsRunningOnMono)
      {
        // Workaround for GacCacheController, which adds all Mono GAC paths to a dictionary, without checking for duplicates.
        // I think the implementation of Dictionary.Add is different on different runtimes
        Environment.SetEnvironmentVariable("MONO_GAC_PREFIX", "/whatever");
      }
    }
  }
}
