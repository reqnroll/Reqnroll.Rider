using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.SelectEmbracingConstruct;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ExtendSelection;

public class GherkinCodeBehindWrappedSelection([NotNull] GherkinFile file, [NotNull] ISelectedRange codeBehindRange) : ISelectedRange
{

    public DocumentRange Range
        => codeBehindRange.Range;

    public ISelectedRange Parent
    {
        get
        {
            var parent = codeBehindRange.Parent;
            if (parent?.Range.IsValid() == true) return new GherkinCodeBehindWrappedSelection(file, parent);
            var node = file.FindNodeAt(Range);
            return node == null ? null : new GherkinNodeSelection(file, node);
        }
    }

    public ExtendToTheWholeLinePolicy ExtendToWholeLine
        => codeBehindRange.ExtendToWholeLine;

    public ITreeRange TryGetTreeRange() => null;
}