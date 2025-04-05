using System;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.UnresolvedReferenceHighlight;

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
        var psiSourceFile = _file.GetSourceFile();
        if (psiSourceFile == null)
            return;
        var consumer = new FilteringHighlightingConsumer(psiSourceFile, _file, DaemonProcess.ContextBoundSettingsStore);
        _file.ProcessDescendants(_elementProcessor, consumer);
        committer(new DaemonStageResult(consumer.CollectHighlightings()));
    }
}