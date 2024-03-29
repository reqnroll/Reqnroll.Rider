using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.SelectEmbracingConstruct;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ExtendSelection
{
    public class GherkinNodeSelection : TreeNodeSelection<GherkinFile>
    {
        public override ISelectedRange Parent
        {
            get
            {
                var parentNode = TreeNode.Parent;
                return parentNode == null ? null : new GherkinNodeSelection(FileNode, parentNode);
            }
        }

        public GherkinNodeSelection([NotNull] GherkinFile fileNode, [NotNull] ITreeNode node)
            : base(fileNode, node)
        {
        }
    }
}