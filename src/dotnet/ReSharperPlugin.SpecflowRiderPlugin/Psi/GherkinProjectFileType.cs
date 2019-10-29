using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ProjectModel;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    [ProjectFileTypeDefinition(Name)]
    public class GherkinProjectFileType : KnownProjectFileType
    {
        public new const string Name = "GHERKIN";
        
        [CanBeNull]
        [UsedImplicitly]
        public new static GherkinProjectFileType Instance { get; private set; }
        
        private GherkinProjectFileType() : base(Name, "Gherkin", new[] {".feature"})  {  }
        
        protected GherkinProjectFileType([NotNull] string name) : base(name)
        {
        }

        protected GherkinProjectFileType([NotNull] string name, [NotNull] string presentableName) : base(name, presentableName)
        {
        }

        protected GherkinProjectFileType([NotNull] string name, [NotNull] string presentableName, [NotNull] IEnumerable<string> extensions) : base(name, presentableName, extensions)
        {
        }
    }
}