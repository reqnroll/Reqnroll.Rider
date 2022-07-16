using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Features.ReSpeller.Analyzers;
using JetBrains.ReSharper.Features.ReSpeller.Highlightings;
using JetBrains.ReSharper.Features.ReSpeller.SpellEngine;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.SpellCheck
{
    public class SpellCheckProcessor :
        IRecursiveElementProcessor<IHighlightingConsumer>
    {
        private readonly ISpellService _spellService;

        public SpellCheckProcessor(ISpellService spellService)
        {
            _spellService = spellService;
        }

        public bool InteriorShouldBeProcessed(ITreeNode element, IHighlightingConsumer context) => true;

        public bool IsProcessingFinished(IHighlightingConsumer context) => false;

        public void ProcessBeforeInterior(ITreeNode element, IHighlightingConsumer consumer)
        {
        }

        public void ProcessAfterInterior(ITreeNode element, IHighlightingConsumer context)
        {
            DocumentRange documentRange = element.GetDocumentRange();
            var containingFile = element.GetContainingFile();
            if (containingFile != null)
                AnalyzerHelper.GenerateRangeBasedHighlightings(_spellService, context, containingFile, documentRange, range => new CommentTypoHighlighting(range, element));
        }
    }
}