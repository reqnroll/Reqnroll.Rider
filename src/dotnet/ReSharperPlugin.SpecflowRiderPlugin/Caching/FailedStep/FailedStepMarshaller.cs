using System.Collections.Generic;
using JetBrains.Serialization;
using JetBrains.Util.PersistentMap;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.TestOutput;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.FailedStep
{
    public class FailedStepMarshaller : IUnsafeMarshaller<ISet<FailedStepCacheEntry>>
    {
        public void Marshal(UnsafeWriter writer, ISet<FailedStepCacheEntry> value)
        {
            writer.Write(value.Count);
            foreach (var failedStep in value)
            {
                writer.Write(failedStep.FeatureText);
                writer.Write(failedStep.ScenarioText);
                writer.Write(failedStep.StepsOutputs.Count);
                foreach (var stepOutput in failedStep.StepsOutputs)
                {
                    writer.Write((int) stepOutput.Status);
                    writer.Write(stepOutput.StatusLine);
                    writer.Write(stepOutput.FirstLine);
                    writer.Write(stepOutput.MultiLineArgument);
                    writer.Write(stepOutput.Table);
                    writer.Write(stepOutput.ErrorOutput);
                }
            }
        }

        public ISet<FailedStepCacheEntry> Unmarshal(UnsafeReader reader)
        {
            var failedStepCount = reader.ReadInt();
            var result = new HashSet<FailedStepCacheEntry>();

            for (var i = 0; i < failedStepCount; i++)
            {
                var featureText = reader.ReadString();
                var scenarioText = reader.ReadString();
                var stepOutputCount = reader.ReadInt();
                var stepsOutputs = new List<StepTestOutput>(stepOutputCount);
                for (var j = 0; j < stepOutputCount; j++)
                {
                    var status = reader.ReadInt();
                    var statusLine = reader.ReadString();
                    var firstLine = reader.ReadString();
                    var multiLineArgument = reader.ReadString();
                    var table = reader.ReadString();
                    var errorOutput = reader.ReadString();

                    stepsOutputs.Add(new StepTestOutput
                    {
                        Status = (StepTestOutput.StepStatus) status,
                        StatusLine = statusLine,
                        Table = table,
                        ErrorOutput = errorOutput,
                        FirstLine = firstLine,
                        MultiLineArgument = multiLineArgument
                    });
                }

                result.Add(new FailedStepCacheEntry
                {
                    FeatureText = featureText,
                    ScenarioText = scenarioText,
                    StepsOutputs = stepsOutputs,
                });
            }

            return result;
        }
    }
}