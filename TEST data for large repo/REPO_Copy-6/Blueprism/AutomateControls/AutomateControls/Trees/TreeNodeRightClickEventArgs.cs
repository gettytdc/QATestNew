using System.Windows.Forms;

namespace AutomateControls.Trees
{
    /// <summary>
    /// Delegate used to handle right clicked tree node
    /// </summary>
    public delegate void TreeNodeRightClickEventHandler(
        object sender, TreeNodeRightClickEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="FlickerFreeTreeView.RightClickNode"/> event
    /// </summary>
    public class TreeNodeRightClickEventArgs
    {
        /// <summary>
        /// The node that was right clicked
        /// </summary>
        public readonly TreeNode Node;

        /// <summary>
        /// Create a new instance of the event args that provide data when a tree
        /// node is right clicked
        /// </summary>
        /// <param name="node">The node that was right clicked</param>
        public TreeNodeRightClickEventArgs(TreeNode node)
        {
            Node = node;
        }
    }
}
