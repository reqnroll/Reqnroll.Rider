using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Extensions
{
    public static class TreeNodeExtensions
    {
        public static IEnumerable<T> GetChildrenInSubtrees<T>([NotNull] this ITreeNode node) where T : class, ITreeNode
        {
            if (node.FirstChild != null)
            {
                ITreeNode child;
                for (child = node.FirstChild; child != null; child = child.NextSibling)
                {
                    if (child is T obj)
                        yield return obj;
                    foreach (var childrenInSubtree in child.GetChildrenInSubtrees<T>())
                        yield return childrenInSubtree;
                }
            }
        }

        public static T GetParentOfType<T>([NotNull] this ITreeNode node) where T : class, ITreeNode
        {
            for (; node != null; node = node.Parent)
            {
                if (node is T obj)
                    return obj;
            }
            return default(T);
        }
    }
}