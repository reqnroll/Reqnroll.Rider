using System;
using JetBrains.Application.Threading;
using JetBrains.Collections;
using JetBrains.DocumentModel;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using Newtonsoft.Json;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings
{
    [PsiComponent]
    public class SpecflowSettingsFilesCache : SimpleICache<SpecflowSettings>
    {
        private readonly SpecflowSettingsProvider _settingsProvider;

        public SpecflowSettingsFilesCache(Lifetime lifetime,
                                          IShellLocks shellLocks,
                                          IPersistentIndexManager persistentIndexManager,
                                          SpecflowSettingsProvider settingsProvider,
                                          SpecflowSettingsMarshaller marshaller)
            : base(lifetime, shellLocks, persistentIndexManager, marshaller)
        {
            _settingsProvider = settingsProvider;
        }

        public override string Version => "2";

        protected override bool IsApplicable(IPsiSourceFile sf)
        {
            return sf.Name == "specflow.json";
        }

        public override void MergeLoaded(object data)
        {
            base.MergeLoaded(data);

            foreach (var (key, value) in Map)
                UpdateInProvider(key, value, null);
        }

        public override object Build(IPsiSourceFile sourceFile, bool isStartup)
        {
            var specflowSettingsText = sourceFile.Document.GetText();
            try
            {
                var settings = JsonConvert.DeserializeObject<SpecflowSettings>(specflowSettingsText);
                return settings;
            }
            catch (Exception)
            {
                //Possible invalid json
            }

            return new SpecflowSettings();
        }

        public override void Merge(IPsiSourceFile sourceFile, object builtPart)
        {
            var newSettings = (SpecflowSettings) builtPart;
            Map.TryGetValue(sourceFile, out var oldSettings);

            UpdateInProvider(sourceFile, newSettings, oldSettings);

            base.Merge(sourceFile, builtPart);
        }

        private void UpdateInProvider(IPsiSourceFile specflowJsonFile, SpecflowSettings newSettings, SpecflowSettings oldSettings)
        {
            var specflowJsonProjectOwner = specflowJsonFile.GetProject();
            if (specflowJsonProjectOwner == null)
                return;

            _settingsProvider.Update(specflowJsonProjectOwner, newSettings);

            if (oldSettings?.Language.Feature == newSettings?.Language.Feature)
                return;

            var featureFilesInProject = specflowJsonProjectOwner.GetAllProjectFiles(o => o.Name.EndsWith(".feature"));
            foreach (var featureFile in featureFilesInProject)
            {
                var featurePsiFiles = featureFile.ToSourceFiles();
                foreach (var featurePsiFile in featurePsiFiles)
                {
                    var psiServices = featurePsiFile.GetPsiServices();
                    var cachedPsiFile = psiServices.Files.PsiFilesCache.TryGetCachedPsiFile(featurePsiFile, GherkinLanguage.Instance);
                    if (cachedPsiFile != null)
                    {
                        cachedPsiFile.OnDocumentChanged(new DocumentChange(featurePsiFile.Document, 0, featurePsiFile.Document.GetTextLength(),
                                                                           featurePsiFile.Document.GetText(), -1, TextModificationSide.NotSpecified));
                        psiServices.Files.MarkAsDirty(featurePsiFile);
                        psiServices.Caches.MarkAsDirty(featurePsiFile);
                    }
                }
            }
        }
    }
}