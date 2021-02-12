using JetBrains.Annotations;
using JetBrains.Application.PersistentMap;
using JetBrains.Serialization;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching
{
    [PolymorphicMarshaller(2)]
    public class CacheVersion
    {
        [UsedImplicitly] public static UnsafeReader.ReadDelegate<object> ReadDelegate = r => new CacheVersion();
        [UsedImplicitly] public static UnsafeWriter.WriteDelegate<object> WriteDelegate = (w, o) => { };
    }
}