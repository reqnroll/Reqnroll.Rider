using System;
using System.Collections.Generic;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public static class CompositeElementExtensions
{
    public static T FindChild<T>(this CompositeElement element, Predicate<T> predicate)
        where T: TreeElement
    {
        var firstChild = (TreeElement) element.FirstChild;
        for (var treeElement1 = firstChild; treeElement1 != null; treeElement1 = treeElement1.nextSibling)
        {
            if (treeElement1 is T treeElement2 && predicate(treeElement2))
                return treeElement2;
        }

        return null;
    }
        
    public static IEnumerable<T> FindChildren<T>(this CompositeElement element, Predicate<T> predicate)
        where T: TreeElement
    {
        var firstChild = (TreeElement) element.FirstChild;
        for (var treeElement1 = firstChild; treeElement1 != null; treeElement1 = treeElement1.nextSibling)
        {
            if (treeElement1 is T treeElement2 && predicate(treeElement2))
                yield return treeElement2;
        }
    }
}