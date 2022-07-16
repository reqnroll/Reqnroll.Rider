using System;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Features.ReSpeller.SpellEngine;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.SpellCheck
{
    public class SpellCheckDaemonStageProcess : IDaemonStageProcess
    {
        private readonly GherkinFile _file;
        private readonly SpellCheckProcessor _elementProcessor;
        public IDaemonProcess DaemonProcess { get; }

        public SpellCheckDaemonStageProcess(
            ISpellService spellService,
            IDaemonProcess daemonProcess,
            GherkinFile file
        )
        {
            DaemonProcess = daemonProcess;
            _file = file;
            _elementProcessor = new SpellCheckProcessor(spellService);
        }

        public void Execute(Action<DaemonStageResult> committer)
        {
            var psiSourceFile = _file.GetSourceFile();
            if (psiSourceFile == null)
                return;
            var consumer = new FilteringHighlightingConsumer(psiSourceFile, _file, DaemonProcess.ContextBoundSettingsStore);
            _file.ProcessDescendants(_elementProcessor, consumer);
            committer(new DaemonStageResult(consumer.Highlightings));

        }
    }
}