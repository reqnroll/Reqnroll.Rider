using JetBrains.ReSharper.Psi;
using JetBrains.Serialization;
using JetBrains.Util.PersistentMap;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.ReqnrollJsonSettings
{
    [PsiComponent]
    public class ReqnrollSettingsMarshaller : IUnsafeMarshaller<ReqnrollSettings>
    {
        public void Marshal(UnsafeWriter writer, ReqnrollSettings value)
        {
            writer.Write(value.Language.Feature);
            writer.Write(value.Language.Tool);
            writer.Write(value.BindingCulture.Name);
        }

        public ReqnrollSettings Unmarshal(UnsafeReader reader)
        {
            var settings = new ReqnrollSettings();
            
            settings.Language.Feature = reader.ReadString();
            settings.Language.Tool = reader.ReadString();
            settings.BindingCulture.Name = reader.ReadString();

            return settings;
        }
    }
}