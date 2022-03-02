using System.Collections.Generic;
using System.Windows.Forms;

namespace AutomateControls.Trees
{
    /// <summary>
    /// Extension methods for <see cref="TreeNode"/> objects
    /// </summary>
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// Gets a sequence containing the current TreeNode's ancestors from the 
        /// immediate parent upwards
        /// </summary>
        /// <param name="node">The TreeNode object</param>
        /// <returns>A sequence of ancestor TreeNode objects</returns>
        public static IEnumerable<TreeNode> GetAncestors(this TreeNode node)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        /// <summary>
        /// Gets a sequence containing the current TreeNode and its ancestors from 
        /// the immediate parent upwards
        /// </summary>
        /// <param name="node">The TreeNode object</param>
        /// <returns>A sequence of ancestor TreeNode objects</returns>
        public static IEnumerable<TreeNode> GetAncestorsAndSelf(this TreeNode node)
        {
            yield return node;
            foreach (var ancestor in node.GetAncestors())
            {
                yield return ancestor;
            }
        }

        /// <summary>
        /// Gets a sequence containing the current TreeNode's descendants
        /// </summary>
        /// <param name="node">The TreeNode object</param>
        /// <returns>A sequence of descendant TreeNode objects</returns>
        public static IEnumerable<TreeNode> GetDescendants(this TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
            {
                yield return child;
                foreach (var descendant in child.GetDescendants())
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// Gets a sequence containing the current TreeNode and its descendants
        /// </summary>
        /// <param name="node">The TreeNode object</param>
        /// <returns>A sequence of descendant TreeNode objects</returns>
        public static IEnumerable<TreeNode> GetDescendantsAndSelf(this TreeNode node)
        {
            yield return node;
            foreach (var descendant in node.GetDescendants())
            {
                yield return descendant;
            }
        }

    }
}