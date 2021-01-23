using System;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon
{
    public class UnresolvedStepsHighlightingDaemonStageProcess : IDaemonStageProcess
    {
        private readonly GherkinFile _file;
        private readonly UnresolvedStepsRecursiveElementProcessor _elementProcessor;
        public IDaemonProcess DaemonProcess { get; }

        public UnresolvedStepsHighlightingDaemonStageProcess(IDaemonProcess daemonProcess, GherkinFile file, ResolveHighlighterRegistrar resolveHighlighterRegistrar)
        {
            DaemonProcess = daemonProcess;
            _file = file;
            _elementProcessor = new UnresolvedStepsRecursiveElementProcessor(DaemonProcess, resolveHighlighterRegistrar);
        }

        public void Execute(Action<DaemonStageResult> committer)
        {
            var consumer = new FilteringHighlightingConsumer(DaemonProcess.SourceFile, _file, DaemonProcess.ContextBoundSettingsStore);
            _file.ProcessDescendants(_elementProcessor, consumer);
            committer(new DaemonStageResult(consumer.Highlightings));
        }
    }
}