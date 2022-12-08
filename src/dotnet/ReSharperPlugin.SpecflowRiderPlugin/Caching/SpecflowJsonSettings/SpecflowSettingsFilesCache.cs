#nullable enable
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using JetBrains.Application.Threading;
using JetBrains.Collections;
using JetBrains.Diagnostics;
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
            return GetConfigSource(sf) != ConfigSource.None;
        }

        public override void MergeLoaded(object data)
        {
            base.MergeLoaded(data);

            foreach (var (key, value) in Map)
                UpdateInProvider(key, value);
        }

        public override object? Build(IPsiSourceFile sourceFile, bool isStartup)
        {
            return GetConfigSource(sourceFile) switch
            {
                ConfigSource.Json => LoadSpecflowJson(sourceFile),
                ConfigSource.AppConfig => LoadFromAppConfig(sourceFile),
                _ => null,
            };
        }

        public override void Drop(IPsiSourceFile sourceFile)
        {
            UpdateInProvider(sourceFile, null);
            base.Drop(sourceFile);
        }

        public override void Merge(IPsiSourceFile sourceFile, object? builtPart)
        {
            var newSettings = (SpecflowSettings?)builtPart;

            UpdateInProvider(sourceFile, newSettings);

            base.Merge(sourceFile, builtPart);
        }

        private ConfigSource GetConfigSource(IPsiSourceFile file)
        {
            if (file.Name == "specflow.json")
                return ConfigSource.Json;
            if (file.Name.Equals("app.config", StringComparison.OrdinalIgnoreCase))
                return ConfigSource.AppConfig;
            return ConfigSource.None;
        }

        private static SpecflowSettings? LoadFromAppConfig(IPsiSourceFile sourceFile)
        {
            try
            {
                var xml = sourceFile.Document.GetText();
                using var sr = new StringReader(xml);
                using var xmlReader = XmlReader.Create(sr);
                var document = XDocument.Load(xmlReader);

                var element = document.XPathSelectElement("//configuration/specFlow");
                if (element == null)
                    return null;

                var xmlSerializer = new XmlSerializer(typeof(SpecflowSettings));
                using var reader = element.CreateReader();
                var specflowSettings = xmlSerializer.Deserialize(reader);
                return (SpecflowSettings?)specflowSettings;

            }
            catch (Exception)
            {
                //Possible invalid json
            }

            return null;
        }

        private static SpecflowSettings? LoadSpecflowJson(IPsiSourceFile sourceFile)
        {
            var specflowSettingsText = sourceFile.Document.GetText();
            try
            {
                return JsonConvert.DeserializeObject<SpecflowSettings>(specflowSettingsText);
            }
            catch (Exception)
            {
                //Possible invalid json
            }
            return null;
        }

        private void UpdateInProvider(IPsiSourceFile file, SpecflowSettings? newSettings)
        {
            var specflowJsonProjectOwner = file.GetProject();
            if (specflowJsonProjectOwner == null)
                return;

            var oldSettings = _settingsProvider.GetSettings(specflowJsonProjectOwner);
            if (!_settingsProvider.TryUpdate(specflowJsonProjectOwner, GetConfigSource(file), newSettings))
                return;

            if (oldSettings.Language.Feature == newSettings?.Language.Feature)
                return;

            var featureFilesInProject = specflowJsonProjectOwner.GetAllProjectFiles(o => o.Name.EndsWith(".feature"));
            foreach (var featureFile in featureFilesInProject)
            {
                var featurePsiFiles = featureFile.ToSourceFiles();
                foreach (var featurePsiFile in featurePsiFiles)
                {
                    var psiServices = featurePsiFile.GetPsiServices();
                    var cachedPsiFile = psiServices.Files.PsiFilesCache.TryGetCachedPsiFile(featurePsiFile, GherkinLanguage.Instance.NotNull());
                    if (cachedPsiFile != null)
                    {
                        cachedPsiFile.OnDocumentChanged(
                            new DocumentChange(
                                featurePsiFile.Document,
                                0,
                                featurePsiFile.Document.GetTextLength(),
                                featurePsiFile.Document.GetText(),
                                featurePsiFile.Document.LastModificationStamp,
                                TextModificationSide.NotSpecified));
                        psiServices.Files.MarkAsDirty(featurePsiFile);
                        psiServices.Caches.MarkAsDirty(featurePsiFile);
                    }
                }
            }
        }
    }
}