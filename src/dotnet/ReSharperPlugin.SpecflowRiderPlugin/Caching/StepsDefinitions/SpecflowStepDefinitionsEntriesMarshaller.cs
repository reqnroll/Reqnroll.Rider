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
            foreach (var cacheClass in value)
            {
                writer.Write(cacheClass.ClassName);
                writer.Write(cacheClass.Methods.Count);
                foreach (var cacheMethod in cacheClass.Methods)
                {
                    writer.Write(cacheMethod.MethodName);
                    writer.Write(cacheMethod.Steps.Count);
                    foreach (var cacheStep in cacheMethod.Steps)
                    {
                        writer.Write((int) cacheStep.StepKind);
                        writer.Write(cacheStep.Pattern);
                    }
                }
            }
        }

        public SpecflowStepsDefinitionsCacheEntries Unmarshal(UnsafeReader reader)
        {
            var entries = new SpecflowStepsDefinitionsCacheEntries();
            var classCount = reader.ReadInt();
            for (var i = 0; i < classCount; i++)
            {
                var className = reader.ReadString();
                var cacheClassEntry = new SpecflowStepDefinitionCacheClassEntry(className);

                var methodCount = reader.ReadInt();
                for (var j = 0; j < methodCount; j++)
                {
                    var methodName = reader.ReadString();
                    var methodCacheEntry = cacheClassEntry.AddMethod(methodName);
                    var stepCount = reader.ReadInt();
                    for (var k = 0; k < stepCount; k++)
                    {
                        var type = reader.ReadInt();
                        var pattern = reader.ReadString();

                        methodCacheEntry.AddStep((GherkinStepKind) type, pattern);
                    }
                }

                entries.Add(cacheClassEntry);
            }
            return entries;
        }
    }
}