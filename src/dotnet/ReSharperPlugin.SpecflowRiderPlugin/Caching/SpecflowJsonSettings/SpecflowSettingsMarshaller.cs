using JetBrains.ReSharper.Psi;
using JetBrains.Serialization;
using JetBrains.Util.PersistentMap;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings
{
    [PsiComponent]
    public class SpecflowSettingsMarshaller : IUnsafeMarshaller<SpecflowSettings>
    {
        public void Marshal(UnsafeWriter writer, SpecflowSettings value)
        {
            writer.Write(value.Language.Feature);
            writer.Write(value.Language.Tool);
            writer.Write(value.BindingCulture.Name);
        }

        public SpecflowSettings Unmarshal(UnsafeReader reader)
        {
            var settings = new SpecflowSettings();
            
            settings.Language.Feature = reader.ReadString();
            settings.Language.Tool = reader.ReadString();
            settings.BindingCulture.Name = reader.ReadString();

            return settings;
        }
    }
}