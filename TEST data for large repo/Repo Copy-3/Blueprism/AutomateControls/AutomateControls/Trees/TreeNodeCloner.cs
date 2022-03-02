using System.Windows.Forms;

namespace AutomateControls.Trees
{
    /// <summary>
    /// Utility class to clone a tree node, ensuring that the expanded
    /// state of the node is cloned too (it is not in the standard
    /// <see cref="TreeNode.Clone"/> implementation.
    /// </summary>
    public class TreeNodeCloner
    {
        /// <summary>
        /// Clones the given treenode, giving the clone the same
        /// expanded state as the given node.
        /// </summary>
        /// <param name="n">The node to clone</param>
        /// <returns>A deep(ish) clone of the treenode. Note that any
        /// associated data in the node, such as the Tag value will be
        /// memberwise cloned, meaning that it if it is an instance
        /// of a reference type, the original and the clone will be
        /// pointing to the same object.</returns>
        public static TreeNode Clone(TreeNode n)
        {
            TreeNode clone = (TreeNode)n.Clone();

            if (n.IsExpanded)
                clone.Expand();
            ExpandNodes(n.Nodes, clone.Nodes);
            return clone;
        }

        /// <summary>
        /// Expands the nodes in the cloned collection to match the
        /// expansion states in the original collection - this
        /// descends the entire tree to ensure that the nodes are
        /// correctly matched in expansion state.
        /// </summary>
        /// <param name="orig">The original treenode collection</param>
        /// <param name="clone">The cloned treenode collection into
        /// which the expanded state from the original should be
        /// copied.</param>
        /// <remarks>The cloned collection is expected to be
        /// structurally identical to the original collection - if
        /// it is not, undefined behaviour will be invoked.
        /// That's bad, by the way.</remarks>
        private static void ExpandNodes(TreeNodeCollection orig, TreeNodeCollection clone)
        {
            for (int i = 0; i < orig.Count; i++)
            {
                TreeNode origNode = orig[i];
                TreeNode cloneNode = clone[i];
                if (origNode.IsExpanded)
                    cloneNode.Expand();
                ExpandNodes(origNode.Nodes, cloneNode.Nodes);
            }
        }
    }
}
