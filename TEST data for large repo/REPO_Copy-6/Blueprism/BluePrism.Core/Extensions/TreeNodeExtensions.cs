using System.Collections.Generic;
using System.Windows.Forms;

namespace BluePrism.Core.Extensions
{
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// Recursive find all nodes from a given node.
        /// </summary>
        public static List<TreeNode> GetAllNodes(this TreeNode node)
        {
            var nodes = new List<TreeNode>(node.GetNodeCount(true))
            {
                node
            };

            foreach (TreeNode n in node.Nodes)
            {
                nodes.AddRange(n.GetAllNodes());
            }
            return nodes;
        }
    }
}
