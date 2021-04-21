using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Helpers
{
    public static class TreeNodeHelper
    {
        [CanBeNull]
        public static ITreeNode GetPreviousNodeOfType<T>(ITreeNode node)
            where T : ITreeNode
        {
            var nodeIterator = node.PrevSibling;
            while (nodeIterator != null)
            {
                if (nodeIterator is T expectedNode)
                    return expectedNode;
                else
                    nodeIterator = nodeIterator.PrevSibling;
            }
            return null;
        }


        [CanBeNull]
        public static ITreeNode GetNextNodeOfType<T>(ITreeNode node)
            where T : ITreeNode
        {
            var nodeIterator = node.NextSibling;
            while (nodeIterator != null)
            {
                if (nodeIterator is T expectedNode)
                    return expectedNode;
                else
                    nodeIterator = nodeIterator.NextSibling;
            }
            return null;
        }
    }
}