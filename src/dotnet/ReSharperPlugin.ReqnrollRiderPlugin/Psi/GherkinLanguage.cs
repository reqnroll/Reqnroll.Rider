using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi
{
    [LanguageDefinition(Name)]
    public class GherkinLanguage : KnownLanguage
    {
        public new const string Name = "GHERKIN";

        [CanBeNull] [UsedImplicitly] public static GherkinLanguage Instance { get; private set; }

        private GherkinLanguage() : base(Name, "GherkinDotnet")
        {
        }

        protected GherkinLanguage([NotNull] string name) : base(name)
        {
        }

        protected GherkinLanguage([NotNull] string name, [NotNull] string presentableName) : base(name, presentableName)
        {
        }
    }
}