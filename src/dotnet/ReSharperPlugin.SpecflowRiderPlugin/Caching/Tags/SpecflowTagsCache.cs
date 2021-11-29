using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Threading;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Files;
using ReSharperPlugin.SpecflowRiderPlugin.Extensions;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.Tags
{
    [PsiComponent]
    public class SpecflowTagsCache : SimpleICache<IList<string>>
    {
        private const int VersionInt = 1;
        public override string Version => VersionInt.ToString();

        public SpecflowTagsCache(
            Lifetime lifetime,
            IShellLocks locks,
            IPersistentIndexManager persistentIndexManager
        ) : base(lifetime, locks, persistentIndexManager, new SpecflowTagMarshaller(), VersionInt)
        {
        }

        public ISet<string> GetAllTags()
        {
            return Map.Values.SelectMany(x => x).ToSet();
        }

        public override object Build(IPsiSourceFile sourceFile, bool isStartup)
        {
            if (!sourceFile.IsValid())
                return null;
            var file = sourceFile.GetPrimaryPsiFile().NotNull();
            if (!file.Language.Is<GherkinLanguage>())
                return null;
            if (!(file is GherkinFile gherkinFile))
                return null;

            var tags = new List<string>();
            var tagsNodes = gherkinFile.GetChildrenInSubtrees<GherkinTag>();
            tags.AddRange(tagsNodes.Select(x => x.GetTagText()));
            return tags;
        }
    }
}