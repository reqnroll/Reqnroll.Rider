using System.Collections.Generic;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using ReSharperPlugin.ReqnrollRiderPlugin.Daemon.Errors;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.References;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.UnresolvedReferenceHighlight;

[Language(typeof(GherkinLanguage))]
public class GherkinResolveProblemHighlighter : IResolveProblemHighlighter
{
    public IHighlighting Run(IReference reference)
    {
        if (reference is ReqnrollStepDeclarationReference reqnrollStepDeclarationReference)
            return new StepNotResolvedError(reqnrollStepDeclarationReference.GetElement());
        return null;
    }

    public IEnumerable<ResolveErrorType> ErrorTypes { get; } = new []
    {
        ResolveErrorType.NOT_RESOLVED
    };
}