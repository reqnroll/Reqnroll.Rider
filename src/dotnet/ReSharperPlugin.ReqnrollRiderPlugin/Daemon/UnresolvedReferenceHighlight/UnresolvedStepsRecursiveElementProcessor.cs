using System;
using System.Linq;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Daemon.Errors;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.UnresolvedReferenceHighlight;

public class UnresolvedStepsRecursiveElementProcessor(IDaemonProcess daemonProcess, ResolveHighlighterRegistrar resolveHighlighterRegistrar) : IRecursiveElementProcessor<IHighlightingConsumer>
{

    public IDaemonProcess DaemonProcess { get; } = daemonProcess;

    public bool InteriorShouldBeProcessed(ITreeNode element, IHighlightingConsumer context)
    {
        if (element is GherkinScenario)
            return true;
        if (element is GherkinScenarioOutline)
            return true;
        if (element is GherkinFeature)
            return true;
        if (element is GherkinRule)
            return true;

        return false;
    }

    public bool IsProcessingFinished(IHighlightingConsumer context)
    {
        if (DaemonProcess.InterruptFlag)
            throw new OperationCanceledException();

        return false;
    }

    public void ProcessBeforeInterior(ITreeNode element, IHighlightingConsumer context)
    {
    }

    public void ProcessAfterInterior(ITreeNode element, IHighlightingConsumer context)
    {
        if (element is GherkinStep gherkinStep)
        {
            var isIgnored = gherkinStep.GetEffectiveTags().Any(tag => tag.Equals("ignore", StringComparison.OrdinalIgnoreCase));
            var references = gherkinStep.GetFirstClassReferences();
            foreach (var reference in references)
            {
                var error = reference.CheckResolveResult();
                if (error == null)
                    throw new InvalidOperationException("ResolveErrorType is null for reference " +
                                                        reference.GetType().FullName);

                if (error == ResolveErrorType.OK)
                    return;

                if (isIgnored)
                    context.AddHighlighting(new IgnoredStepNotResolvedInfo(gherkinStep));
                else
                {
                    if (resolveHighlighterRegistrar.ContainsHandler(GherkinLanguage.Instance.NotNull(), error))
                    {
                        var highlighting = resolveHighlighterRegistrar.GetResolveHighlighting(reference, error);
                        if (highlighting != null)
                            context.AddHighlighting(highlighting);
                    }
                    else
                        context.AddHighlighting(new StepNotResolvedError(gherkinStep));
                }
            }
        }
    }
}