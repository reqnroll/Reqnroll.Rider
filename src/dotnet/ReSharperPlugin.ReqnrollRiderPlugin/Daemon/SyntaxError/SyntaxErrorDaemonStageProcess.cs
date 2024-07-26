using System;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.SyntaxError
{
    public class SyntaxErrorDaemonStageProcess : IDaemonStageProcess
    {
        private readonly GherkinFile _file;
        private readonly SyntaxErrorProcessor _elementProcessor;
        public IDaemonProcess DaemonProcess { get; }

        public SyntaxErrorDaemonStageProcess(IDaemonProcess daemonProcess, GherkinFile file)
        {
            DaemonProcess = daemonProcess;
            _file = file;
            _elementProcessor = new SyntaxErrorProcessor();
        }

        public void Execute(Action<DaemonStageResult> committer)
        {
            var psiSourceFile = _file.GetSourceFile();
            if (psiSourceFile == null)
                return;
            var consumer = new FilteringHighlightingConsumer(psiSourceFile, _file, DaemonProcess.ContextBoundSettingsStore);
            _file.ProcessDescendants(_elementProcessor, consumer);
            committer(new DaemonStageResult(consumer.CollectHighlightings()));
        }
    }
}