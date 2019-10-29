using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinDaemonBehaviour : LanguageSpecificDaemonBehavior
    {
        public override ErrorStripeRequest InitialErrorStripe(IPsiSourceFile sourceFile)
        {
            if (sourceFile.Properties.ShouldBuildPsi && sourceFile.Properties.ProvidesCodeModel && sourceFile.IsLanguageSupported<GherkinLanguage>())
                return ErrorStripeRequest.STRIPE_AND_ERRORS;

            return ErrorStripeRequest.NONE;
        }
    }
}