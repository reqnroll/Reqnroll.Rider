using System;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.FailedStep;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.TestOutput;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.ExecutionFailedStep
{
    public class ExecutionFailedStepGutterIconDaemonStageProcess : IDaemonStageProcess
    {
        public IDaemonProcess DaemonProcess { get; }

        private readonly GherkinFile _gherkinFile;
        private readonly FailedStepCache _failedStepCache;

        public ExecutionFailedStepGutterIconDaemonStageProcess(IDaemonProcess daemonProcess, GherkinFile gherkinFile, FailedStepCache failedStepCache)
        {
            DaemonProcess = daemonProcess;
            _gherkinFile = gherkinFile;
            _failedStepCache = failedStepCache;
        }

        public void Execute(Action<DaemonStageResult> committer)
        {
            var psiSourceFile = _gherkinFile.GetSourceFile();
            if (psiSourceFile == null)
                return;

            var consumer = new FilteringHighlightingConsumer(psiSourceFile, _gherkinFile, DaemonProcess.ContextBoundSettingsStore);
            HighlightFailedSteps(psiSourceFile, consumer);
            committer(new DaemonStageResult(consumer.Highlightings));
        }

        private void HighlightFailedSteps(IPsiSourceFile psiSourceFile, FilteringHighlightingConsumer consumer)
        {
            var failedSteps = _failedStepCache.GetFailedSteps(psiSourceFile);
            foreach (var failedStep in failedSteps)
            {
                var feature = _gherkinFile.GetFeature(failedStep.FeatureText);
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
}