using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Treeview extenstion methods.
    /// </summary>
    public static class TreeviewExtensionMethods
    {
        /// <summary>
        /// Flatten all the nodes in the treenode collection.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IEnumerable<TreeNode> Flatten(this TreeNodeCollection e) => e.Cast<TreeNode>().SelectMany(c => c.Nodes.Flatten()).Concat(e.Cast<TreeNode>());

        /// <summary>
        /// Flatten all the nodes in the treenode collection if they are expanded.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IEnumerable<TreeNode> FlattenExpanded(this TreeNodeCollection e) => e.Cast<TreeNode>().Where(x=>x.IsExpanded && x.Nodes.Count > 0).SelectMany(c => c.Nodes.Flatten()).Concat(e.Cast<TreeNode>());


        /// <summary>
        /// perform a tree traverse using the stack method.  Needed when doing recursion within linq
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> selector)
        {
            var stack = new Stack<T>(items);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                foreach (var child in selector(next))
                {
                    stack.Push(child);
                }
            }
        }
    }
}
