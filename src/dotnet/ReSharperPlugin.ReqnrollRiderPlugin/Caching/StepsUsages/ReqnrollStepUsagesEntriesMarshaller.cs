using JetBrains.Serialization;
using JetBrains.Util.PersistentMap;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsUsages
{
    public class ReqnrollStepUsagesEntriesMarshaller : IUnsafeMarshaller<ReqnrollStepsUsagesCacheEntries>
    {
        public void Marshal(UnsafeWriter writer, ReqnrollStepsUsagesCacheEntries value)
        {
            writer.Write(value.Count);
            foreach (var entry in value)
            {
                writer.Write((int) entry.StepKind);
                writer.Write(entry.StepText);
            }
        }

        public ReqnrollStepsUsagesCacheEntries Unmarshal(UnsafeReader reader)
        {
            var entries = new ReqnrollStepsUsagesCacheEntries();
            var count = reader.ReadInt();
            for (var i = 0; i < count; i++)
            {
                var type = reader.ReadInt();
                var stepText = reader.ReadString();

                entries.Add(new ReqnrollStepUsageCacheEntry((GherkinStepKind) type, stepText));
            }
            return entries;
        }
    }
}