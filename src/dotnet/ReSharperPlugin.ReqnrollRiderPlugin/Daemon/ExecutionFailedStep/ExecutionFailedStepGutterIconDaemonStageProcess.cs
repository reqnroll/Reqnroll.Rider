using System;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.FailedStep;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils.TestOutput;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.ExecutionFailedStep;

public class ExecutionFailedStepGutterIconDaemonStageProcess(IDaemonProcess daemonProcess, GherkinFile gherkinFile, FailedStepCache failedStepCache) : IDaemonStageProcess
{
    public IDaemonProcess DaemonProcess { get; } = daemonProcess;

    public void Execute(Action<DaemonStageResult> committer)
    {
        var psiSourceFile = gherkinFile.GetSourceFile();
        if (psiSourceFile == null)
            return;

        var consumer = new FilteringHighlightingConsumer(psiSourceFile, gherkinFile, DaemonProcess.ContextBoundSettingsStore);
        HighlightFailedSteps(psiSourceFile, consumer);
        committer(new DaemonStageResult(consumer.CollectHighlightings()));
    }

    private void HighlightFailedSteps(IPsiSourceFile psiSourceFile, FilteringHighlightingConsumer consumer)
    {
        var failedSteps = failedStepCache.GetFailedSteps(psiSourceFile);
        foreach (var failedStep in failedSteps)
        {
            var feature = gherkinFile.GetFeature(failedStep.FeatureText);
            var scenario = feature?.GetScenario(failedStep.ScenarioText);
            if (scenario == null)
                continue;

            var steps = scenario.GetSteps().ToList();
            for (var i = 0; i < steps.Count && i < failedStep.StepsOutputs.Count; i++)
            {
                var stepTestOutput = failedStep.StepsOutputs[i];
                if (!steps[i].Match(stepTestOutput))
                    continue; // Does not match, maybe the file has changed
                if (stepTestOutput.Status != StepTestOutput.StepStatus.Done
                    && stepTestOutput.Status != StepTestOutput.StepStatus.Skipped
                    && stepTestOutput.Status != StepTestOutput.StepStatus.NotImplemented)
                    consumer.AddHighlighting(new ExecutionFailedStepHighlighting(steps[i], stepTestOutput));
            }
        }
    }
}