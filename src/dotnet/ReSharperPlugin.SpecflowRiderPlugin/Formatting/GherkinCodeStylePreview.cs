using System;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.OptionPages.CodeStyle;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Formatting
{
  [CodePreviewPreparatorComponent]
  public class GherkinCodeStylePreview : CodePreviewPreparator
  {
    public override KnownLanguage Language => GherkinLanguage.Instance;
    public override ProjectFileType ProjectFileType => GherkinProjectFileType.Instance;

    protected override ITreeNode Parse(IParser parser, PreviewParseType parseType)
    {
      var gherkinParser = (GherkinParser)parser;
      switch (parseType)
      {
        case PreviewParseType.File:
          return gherkinParser.ParseFile();

        case PreviewParseType.None:
          return null;

        default:
          throw new NotImplementedException();
      }
    }
  }
}