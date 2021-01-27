using JetBrains.Serialization;
using JetBrains.Util.PersistentMap;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsUsages
{
    public class SpecflowStepUsagesEntriesMarshaller : IUnsafeMarshaller<SpecflowStepsUsagesCacheEntries>
    {
        public void Marshal(UnsafeWriter writer, SpecflowStepsUsagesCacheEntries value)
        {
            writer.Write(value.Count);
            foreach (var entry in value)
            {
                writer.Write((int) entry.StepKind);
                writer.Write(entry.StepText);
            }
        }

        public SpecflowStepsUsagesCacheEntries Unmarshal(UnsafeReader reader)
        {
            var entries = new SpecflowStepsUsagesCacheEntries();
            var count = reader.ReadInt();
            for (var i = 0; i < count; i++)
            {
                var type = reader.ReadInt();
                var stepText = reader.ReadString();

                entries.Add(new SpecflowStepUsageCacheEntry((GherkinStepKind) type, stepText));
            }
            return entries;
        }
    }
}