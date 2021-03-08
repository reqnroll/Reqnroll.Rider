using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.Application.Threading;
using JetBrains.Diagnostics;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Formatting
{
    [CodeCleanupModule]
    public class GherkinReformatCode : ICodeCleanupModule
    {
        public string Name => "Reformat Gherkin";
        public PsiLanguageType LanguageType => GherkinLanguage.Instance.NotNull();
        public bool IsAvailableOnSelection => true;

        private static readonly Descriptor ourDescriptor = new Descriptor();
        public ICollection<CodeCleanupOptionDescriptor> Descriptors
        {
            get { return new CodeCleanupOptionDescriptor[] { ourDescriptor }; }
        }

        public bool IsAvailable(IPsiSourceFile sourceFile) => sourceFile.IsLanguageSupported<GherkinLanguage>();
        public bool IsAvailable(CodeCleanupProfile profile) => profile.GetSetting(ourDescriptor);

        public void SetDefaultSetting(CodeCleanupProfile profile, CodeCleanup.DefaultProfileType profileType)
        {
            switch (profileType)
            {
                case CodeCleanup.DefaultProfileType.FULL:
                case CodeCleanup.DefaultProfileType.REFORMAT:
                case CodeCleanup.DefaultProfileType.CODE_STYLE:
                    profile.SetSetting(ourDescriptor, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("profileType");
            }
        }

        public void Process(IPsiSourceFile sourceFile, IRangeMarker rangeMarker, CodeCleanupProfile profile, IProgressIndicator progressIndicator, IUserDataHolder cache)
        {
            var solution = sourceFile.GetSolution();

            if (!profile.GetSetting(ourDescriptor)) return;

            var psiServices = sourceFile.GetPsiServices();
            GherkinFile[] files;
            using (new ReleaseLockCookie(psiServices.Locks, LockKind.FullWrite))
            {
                psiServices.Locks.AssertReadAccessAllowed();
                files = sourceFile.GetPsiFiles<GherkinLanguage>().Cast<GherkinFile>().ToArray();
            }
            using (progressIndicator.SafeTotal(Name, files.Length))
            {
                foreach (var file in files)
                {
                    using (var indicator = progressIndicator.CreateSubProgress(1))
                    {
                        var service = file.Language.LanguageService();
                        if (service == null) return;

                        var formatter = service.CodeFormatter;

                        sourceFile.GetPsiServices().Transactions.Execute("Code cleanup", delegate
                        {
                            if (rangeMarker != null && rangeMarker.IsValid)
                                CodeFormatterHelper.Format(file.Language,
                                    solution, rangeMarker.DocumentRange, CodeFormatProfile.DEFAULT, true, false, indicator);
                            else
                            {
                                formatter.FormatFile(
                                    file,
                                    CodeFormatProfile.DEFAULT,
                                    indicator);
                            }
                        });
                    }
                }
            }
        }

        [DefaultValue(false)]
        [DisplayName("Reformat code")]
        [Category("Gherkin")]
        private class Descriptor : CodeCleanupBoolOptionDescriptor
        {
            public Descriptor() : base("GherkinReformatCode") { }
        }

    }
}