using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Serialization;
using JetBrains.Util.PersistentMap;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions
{
    public class ReqnrollStepDefinitionsEntriesMarshaller : IUnsafeMarshaller<ReqnrollStepsDefinitionsCacheEntries>
    {
        public void Marshal(UnsafeWriter writer, ReqnrollStepsDefinitionsCacheEntries value)
        {
            writer.WriteInt32(value.Count);
            foreach (var cacheClass in value)
            {
                writer.WriteString(cacheClass.ClassName);
                WriteScopes(writer, cacheClass.Scopes);
                writer.WriteBoolean(cacheClass.HasReqnrollBindingAttribute);
                writer.WriteInt32(cacheClass.Methods.Count);
                foreach (var cacheMethod in cacheClass.Methods)
                {
                    writer.WriteString(cacheMethod.MethodName);
                    WriteStringArray(writer, cacheMethod.MethodParameterTypes);
                    WriteStringArray(writer, cacheMethod.MethodParameterNames);
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

        public ReqnrollStepsDefinitionsCacheEntries Unmarshal(UnsafeReader reader)
        {
            var entries = new ReqnrollStepsDefinitionsCacheEntries();
            var classCount = reader.ReadInt32();
            for (var i = 0; i < classCount; i++)
            {
                var className = reader.ReadString();
                var classScopes = ParseScope(reader);
                var hasReqnrollBindingAttribute = reader.ReadBool();
                var cacheClassEntry = new ReqnrollStepDefinitionCacheClassEntry(className, hasReqnrollBindingAttribute, classScopes);

                var methodCount = reader.ReadInt32();
                for (var j = 0; j < methodCount; j++)
                {
                    var methodName = reader.ReadString();
                    var methodParameterTypes = ParseArrayOfString(reader);
                    var methodParameterNames = ParseArrayOfString(reader);
                    var methodScopes = ParseScope(reader);
                    var methodCacheEntry = cacheClassEntry.AddMethod(methodName, methodParameterTypes, methodParameterNames, methodScopes);
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


        private void WriteStringArray(UnsafeWriter writer, string[] cacheMethodMethodParameterTypes)
        {
            writer.WriteByte((byte)cacheMethodMethodParameterTypes.Length);
            foreach (var type in cacheMethodMethodParameterTypes)
                writer.WriteString(type);
        }

        private string[] ParseArrayOfString(UnsafeReader reader)
        {
            int count = reader.ReadByte();
            var methodParameterTypes = new string[count];
            for (var i = 0; i < count; i++)
                methodParameterTypes[i] = reader.ReadString();
            return methodParameterTypes;
        }

        private void WriteScopes(UnsafeWriter writer, [CanBeNull] IReadOnlyList<ReqnrollStepScope> cacheMethodScopes)
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
        private IReadOnlyList<ReqnrollStepScope> ParseScope(UnsafeReader reader)
        {
            var scopeCount = reader.ReadInt16();
            if (scopeCount == 0)
                return null;

            var scopes = new List<ReqnrollStepScope>(scopeCount);
            for (var k = 0; k < scopeCount; k++)
            {
                var feature = reader.ReadString();
                var scenario = reader.ReadString();
                var tag = reader.ReadString();
                scopes.Add(new ReqnrollStepScope(feature, scenario, tag));
            }
            return scopes;
        }
    }
}