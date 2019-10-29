using JetBrains.Application.Settings;

namespace ReSharperPlugin.SpecflowRiderPlugin.Experiments
{
//    [SettingsKey(
//        typeof(EnvironmentSettings),
//        typeof(CodeEditingSettings),
//        "Settings for SpecflowRiderPlugin")]
    public class SampleSettings
    {
        [SettingsEntry(DefaultValue: "<default>", Description: "Sample Description")]
        public string SampleText;
    }
}