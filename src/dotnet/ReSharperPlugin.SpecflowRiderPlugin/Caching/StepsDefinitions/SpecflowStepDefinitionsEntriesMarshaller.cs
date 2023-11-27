using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Serialization;
using JetBrains.Util.PersistentMap;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions
{
    public class SpecflowStepDefinitionsEntriesMarshaller : IUnsafeMarshaller<SpecflowStepsDefinitionsCacheEntries>
    {
        public void Marshal(UnsafeWriter writer, SpecflowStepsDefinitionsCacheEntries value)
        {
            writer.WriteInt32(value.Count);
            foreach (var cacheClass in value)
            {
                writer.WriteString(cacheClass.ClassName);
                WriteScopes(writer, cacheClass.Scopes);
                writer.WriteBoolean(cacheClass.HasSpecflowBindingAttribute);
                writer.WriteInt32(cacheClass.Methods.Count);
                foreach (var cacheMethod in cacheClass.Methods)
                {
                    writer.WriteString(cacheMethod.MethodName);
                    WriteScopes(writer, cacheMethod.Scopes);
                    writer.WriteInt32(cacheMethod.Steps.Count);
                    foreach (var cacheStep in cacheMethod.Steps)
                    {
                        writer.WriteInt32((int)cacheStep.StepKind);
                        writer.WriteString(cacheStep.Pattern);
                    }
                }
            }
        }

        public SpecflowStepsDefinitionsCacheEntries Unmarshal(UnsafeReader reader)
        {
            var entries = new SpecflowStepsDefinitionsCacheEntries();
            var classCount = reader.ReadInt32();
            for (var i = 0; i < classCount; i++)
            {
                var className = reader.ReadString();
                var classScopes = ParseScope(reader);
                var hasSpecflowBindingAttribute = reader.ReadBool();
                var cacheClassEntry = new SpecflowStepDefinitionCacheClassEntry(className, hasSpecflowBindingAttribute, classScopes);

                var methodCount = reader.ReadInt32();
                for (var j = 0; j < methodCount; j++)
                {
                    var methodName = reader.ReadString();
                    var methodScopes = ParseScope(reader);
                    var methodCacheEntry = cacheClassEntry.AddMethod(methodName, methodScopes);
                    var stepCount = reader.ReadInt32();
                    for (var k = 0; k < stepCount; k++)
                    {
                        var type = reader.ReadInt32();
                        var pattern = reader.ReadString();

                        methodCacheEntry.AddStep((GherkinStepKind)type, pattern);
                    }
                }

                entries.Add(cacheClassEntry);
            }
            return entries;
        }

        private void WriteScopes(UnsafeWriter writer, [CanBeNull] IReadOnlyList<SpecflowStepScope> cacheMethodScopes)
        {
            if (cacheMethodScopes == null)
            {
                writer.WriteInt16(0);
                return;
            }
            writer.WriteInt16((short)cacheMethodScopes.Count);
            foreach (var (feature, scenario, tag) in cacheMethodScopes)
            {
                writer.WriteString(feature);
                writer.WriteString(scenario);
                writer.WriteString(tag);
            }
        }

        [CanBeNull]
        private IReadOnlyList<SpecflowStepScope> ParseScope(UnsafeReader reader)
        {
            var scopeCount = reader.ReadInt16();
            if (scopeCount == 0)
                return null;

            var scopes = new List<SpecflowStepScope>(scopeCount);
            for (var k = 0; k < scopeCount; k++)
            {
                var feature = reader.ReadString();
                var scenario = reader.ReadString();
                var tag = reader.ReadString();
                scopes.Add(new SpecflowStepScope(feature, scenario, tag));
            }
            return scopes;
        }
    }
}