using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.SelectEmbracingConstruct;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ExtendSelection
{
    public class GherkinCodeBehindWrappedSelection : ISelectedRange
    {
        [NotNull] private readonly GherkinFile _file;
        [NotNull] private readonly ISelectedRange _codeBehindRange;

        public DocumentRange Range
            => _codeBehindRange.Range;

        public ISelectedRange Parent
        {
            get
            {
                var parent = _codeBehindRange.Parent;
                if (parent?.Range.IsValid() == true) return new GherkinCodeBehindWrappedSelection(_file, parent);
                var node = _file.FindNodeAt(Range);
                return node == null ? null : new GherkinNodeSelection(_file, node);
            }
        }

        public ExtendToTheWholeLinePolicy ExtendToWholeLine
            => _codeBehindRange.ExtendToWholeLine;

        public GherkinCodeBehindWrappedSelection([NotNull] GherkinFile file, [NotNull] ISelectedRange codeBehindRange)
        {
            _file = file;
            _codeBehindRange = codeBehindRange;
        }

        public ITreeRange TryGetTreeRange() => null;
    }
}