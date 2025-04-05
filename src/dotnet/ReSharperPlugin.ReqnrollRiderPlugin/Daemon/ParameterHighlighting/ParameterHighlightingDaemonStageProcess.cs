using System;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.ParameterHighlighting;

public class ParameterHighlightingDaemonStageProcess(IDaemonProcess daemonProcess, GherkinFile file) : IDaemonStageProcess
{
    private readonly ParameterHighlightingProcessor _elementProcessor = new();
    public IDaemonProcess DaemonProcess { get; } = daemonProcess;

    public void Execute(Action<DaemonStageResult> committer)
    {
        var psiSourceFile = file.GetSourceFile();
        if (psiSourceFile == null)
            return;
        var consumer = new FilteringHighlightingConsumer(psiSourceFile, file, DaemonProcess.ContextBoundSettingsStore);
        file.ProcessDescendants(_elementProcessor, consumer);
        committer(new DaemonStageResult(consumer.CollectHighlightings()));
    }
}