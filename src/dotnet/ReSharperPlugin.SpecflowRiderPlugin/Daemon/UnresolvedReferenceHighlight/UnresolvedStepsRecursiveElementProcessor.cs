using System;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Daemon.Errors;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.UnresolvedReferenceHighlight
{
    public class UnresolvedStepsRecursiveElementProcessor : IRecursiveElementProcessor<IHighlightingConsumer>
    {
        private readonly ResolveHighlighterRegistrar _resolveHighlighterRegistrar;

        public IDaemonProcess DaemonProcess { get; }

        public UnresolvedStepsRecursiveElementProcessor(IDaemonProcess daemonProcess, ResolveHighlighterRegistrar resolveHighlighterRegistrar)
        {
            DaemonProcess = daemonProcess;
            _resolveHighlighterRegistrar = resolveHighlighterRegistrar;
        }

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
                var references = gherkinStep.GetFirstClassReferences();
                foreach (var reference in references)
                {
                    var error = reference.CheckResolveResult();
                    if (error == null)
                        throw new InvalidOperationException("ResolveErrorType is null for reference " +
                                                            reference.GetType().FullName);

                    if (error == ResolveErrorType.OK)
                        return;

                    // ReSharper disable once AssignNullToNotNullAttribute
                    if (_resolveHighlighterRegistrar.ContainsHandler(GherkinLanguage.Instance, error))
                    {
                        var highlighting = _resolveHighlighterRegistrar.GetResolveHighlighting(reference, error);
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