using System.Collections.Generic;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.References;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.UnresolvedReferenceHighlight
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinResolveProblemHighlighter : IResolveProblemHighlighter
    {
        public IHighlighting Run(IReference reference)
        {
            if (reference is SpecflowStepDeclarationReference specflowStepDeclarationReference)
                return new NotResolvedError(specflowStepDeclarationReference.GetElement());
            return null;
        }

        public IEnumerable<ResolveErrorType> ErrorTypes { get; } = new []
        {
            ResolveErrorType.NOT_RESOLVED
        };
    }
}