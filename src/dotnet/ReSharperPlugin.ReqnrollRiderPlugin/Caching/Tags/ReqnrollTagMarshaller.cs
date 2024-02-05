using System.Collections.Generic;
using JetBrains.Serialization;
using JetBrains.Util;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.Tags
{
    public class ReqnrollTagMarshaller : JetBrains.Util.PersistentMap.IUnsafeMarshaller<IList<string>>
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