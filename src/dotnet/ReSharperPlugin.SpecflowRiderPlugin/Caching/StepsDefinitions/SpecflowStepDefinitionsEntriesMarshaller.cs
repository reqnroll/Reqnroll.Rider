using JetBrains.Serialization;
using JetBrains.Util.PersistentMap;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    public class SpecflowStepDefinitionsEntriesMarshaller : IUnsafeMarshaller<SpecflowStepsDefinitionsCacheEntries>
    {
        public void Marshal(UnsafeWriter writer, SpecflowStepsDefinitionsCacheEntries value)
        {
            writer.Write(value.Count);
            foreach (var entry in value)
            {
                writer.Write((int) entry.StepKind);
                writer.Write(entry.Pattern);
                writer.Write(entry.MethodName);
            }
        }

        public SpecflowStepsDefinitionsCacheEntries Unmarshal(UnsafeReader reader)
        {
            var entries = new SpecflowStepsDefinitionsCacheEntries();
            var count = reader.ReadInt();
            for (var i = 0; i < count; i++)
            {
                var type = reader.ReadInt();
                var pattern = reader.ReadString();
                var methodName = reader.ReadString();

                entries.Add(new SpecflowStepDefinitionCacheEntry(pattern, (GherkinStepKind) type, methodName));
            }
            return entries;
        }
    }
}