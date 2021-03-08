using System.Collections.Generic;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Calculated.Interface;
using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Formatting
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinFormatterInfoProvider : FormatterInfoProviderWithFluentApi<CodeFormattingContext, GherkinFormatSettingsKey>
    {
        public GherkinFormatterInfoProvider(ISettingsSchema settingsSchema, ICalculatedSettingsSchema calculatedSettingsSchema, IThreading threading, Lifetime lifetime)
            : base(settingsSchema, calculatedSettingsSchema, threading, lifetime)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            Indenting();
            Aligning();
            Formatting();
        }

        public override ProjectFileType MainProjectFileType => GherkinProjectFileType.Instance;

        private void Indenting()
        {
        }

        private void Aligning()
        {
        }

        private void Formatting()
        {
        }
    }
}