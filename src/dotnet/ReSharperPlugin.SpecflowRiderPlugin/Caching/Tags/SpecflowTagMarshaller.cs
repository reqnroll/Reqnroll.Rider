using System.Collections.Generic;
using JetBrains.Serialization;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.Tags
{
    public class SpecflowTagMarshaller : JetBrains.Util.PersistentMap.IUnsafeMarshaller<System.Collections.Generic.IList<string>>
    {
        public void Marshal(UnsafeWriter writer, IList<string> values)
        {
            writer.Write(values.Count);
            foreach (var v in values)
            {
                writer.Write(v);
            }
        }

        public IList<string> Unmarshal(UnsafeReader reader)
        {
            var count = reader.ReadInt32();
            if (count == 0)
                return EmptyList<string>.InstanceList;
            var tags = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                tags.Add(reader.ReadString());
            }
            return tags;
        }
    }
}