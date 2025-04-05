using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon;

[Language(typeof(GherkinLanguage))]
public class GherkinDaemonBehaviour : LanguageSpecificDaemonBehavior
{
    public override ErrorStripeRequestWithDescription InitialErrorStripe(IPsiSourceFile sourceFile)
    {
        if (sourceFile.Properties.ShouldBuildPsi && sourceFile.Properties.ProvidesCodeModel && sourceFile.IsLanguageSupported<GherkinLanguage>())
            return ErrorStripeRequestWithDescription.StripeAndErrors;

        return ErrorStripeRequestWithDescription.None("");
    }
}