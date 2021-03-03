using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon.SyntaxHighlighting;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.ParameterNameHints.ManagedLanguage;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.References;
using ReSharperPlugin.SpecflowRiderPlugin.SyntaxHighlighting;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.ParameterHighlighting
{
    public class ParameterHighlightingProcessor : TreeNodeVisitor<IHighlightingConsumer>,
        IRecursiveElementProcessor<IHighlightingConsumer>
    {

        public ParameterHighlightingProcessor(IDaemonProcess daemonProcess, ResolveHighlighterRegistrar resolveHighlighterRegistrar)
        {
        }

        public bool InteriorShouldBeProcessed(ITreeNode element, IHighlightingConsumer context) => true;
 

        public bool IsProcessingFinished(IHighlightingConsumer context) => false;

        public void ProcessBeforeInterior(ITreeNode element, IHighlightingConsumer consumer)
        {
            if (!(element is GherkinStep gherkinStep))
                return;
            gherkinStep.Accept(this, consumer);
        }

        public void ProcessAfterInterior(ITreeNode element, IHighlightingConsumer context)
        {
        }

        public override void VisitNode(ITreeNode node, IHighlightingConsumer consumer)
        {
            if (node is GherkinStep step)
            {
                var references = step.GetFirstClassReferences();
                if (references.Count != 1 || !(references[0] is SpecflowStepDeclarationReference)) return;

                SpecflowStepDeclarationReference reference = (SpecflowStepDeclarationReference) references[0];
                
                var document = step.GetDocumentRange().Document;
                var parameterRanges = GherkinPsiUtil.BuildParameterRanges(step, reference, reference.GetDocumentRange());

                foreach (var range in parameterRanges)
                {
                    var documentRange = new DocumentRange(document, range);
                    consumer.AddHighlighting(new ReSharperSyntaxHighlighting(GherkinHighlightingAttributeIds.REGEXP_PARAMETER, null, documentRange));
                }
                base.VisitNode(node, consumer);
            }
        }


    }
}