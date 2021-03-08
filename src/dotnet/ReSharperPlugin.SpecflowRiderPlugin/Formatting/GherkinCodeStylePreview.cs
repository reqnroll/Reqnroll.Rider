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
    protected override ITreeNode Parse(IParser parser, PreviewParseType parseType)
    {
      var shaderLabParser = (GherkinParser)parser;
      switch (parseType)
      {
        case PreviewParseType.File:
          return shaderLabParser.ParseFile();

        case PreviewParseType.None:
          return null;

        default:
          throw new NotImplementedException();
      }
    }

    public override KnownLanguage Language
    {
      get { return GherkinLanguage.Instance; }
    }

    public override ProjectFileType ProjectFileType
    {
      get { return GherkinProjectFileType.Instance; }
    }
  }
}