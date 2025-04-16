using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ProjectModel;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

[ProjectFileTypeDefinition(Name)]
public class GherkinProjectFileType : KnownProjectFileType
{
    public new const string Name = "GHERKIN";
    // ReSharper disable once InconsistentNaming
    public const string FEATURE_EXTENSION = ".feature";
        
    [CanBeNull]
    [UsedImplicitly]
    public new static GherkinProjectFileType Instance { get; private set; }
        
    private GherkinProjectFileType() : base(Name, "GherkinDotnet", [FEATURE_EXTENSION])  {  }
        
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