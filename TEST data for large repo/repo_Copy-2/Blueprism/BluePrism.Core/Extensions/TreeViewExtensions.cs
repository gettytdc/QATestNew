using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BluePrism.Core.Extensions
{
    public static class TreeViewExtensions
    {
        public static void ExpandAll(this TreeView treeView, ICollection<TreeNode> exceptions) => ExpandAllNodes(treeView.Nodes, exceptions);

        private static void ExpandAllNodes(IEnumerable nodes, ICollection<TreeNode> exceptions)
        {
            foreach (TreeNode node in nodes)
            {
                if (exceptions.Contains(node))
                {
                    continue;
                }

                node.Expand();
                if (node.Nodes.Count > 0)
                {
                    ExpandAllNodes(node.Nodes, exceptions);
                }
            }
        }
    }
}
