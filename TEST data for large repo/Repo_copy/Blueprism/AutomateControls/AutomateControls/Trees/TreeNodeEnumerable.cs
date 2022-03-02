using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

namespace AutomateControls.Trees
{
    /// <summary>
    /// Enumerable over a collection of treenodes which can be used iteratively, i.e.
    /// without resorting to recursion.
    /// </summary>
    public class TreeNodeEnumerable: IEnumerable<TreeNode>
    {
        #region - Member Variables -

        // Flag indicating if the enumerating should go depth-first or breadth-first
        private bool _depthFirst;

        // The collection of nodes to enumerate
        private TreeNodeCollection _nodes;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new depth-first enumerable over the nodes in the given treeview
        /// </summary>
        /// <param name="tv">The TreeView containing the nodes to enumerate</param>
        public TreeNodeEnumerable(TreeView tv) : this(tv.Nodes, true) { }

        /// <summary>
        /// Creates a new enumerable over the nodes in the given treeview.
        /// </summary>
        /// <param name="tv">The treeview containing the nodes to enumerate</param>
        /// <param name="depthFirst">True to enumerate the nodes in depth-first
        /// order; False to enumerate in breadth-first order</param>
        public TreeNodeEnumerable(TreeView tv, bool depthFirst)
            : this(tv.Nodes, depthFirst) { }

        /// <summary>
        /// Creates a new depth-first enumerable over a node collection
        /// </summary>
        /// <param name="nodes">The collection of nodes to enumerate</param>
        public TreeNodeEnumerable(TreeNodeCollection nodes) : this(nodes, true) { }

        /// <summary>
        /// Creates a new enumerable over a node collection
        /// </summary>
        /// <param name="nodes">The collection of nodes to enumerate</param>
        /// <param name="depthFirst">True to enumerate the nodes in depth-first
        /// order; False to enumerate in breadth-first order</param>
        public TreeNodeEnumerable(TreeNodeCollection nodes, bool depthFirst)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            _nodes = nodes;
            _depthFirst = depthFirst;
        }

        #endregion

        #region - IEnumerable<TreeNode> and related Members -

        /// <summary>
        /// Gets an enumerator over the treenodes set within this enumerable
        /// </summary>
        /// <returns>An initialised enumerator over the treenodes</returns>
        public IEnumerator<TreeNode> GetEnumerator()
        {
            return new TreeNodeEnumerator(_nodes, _depthFirst);
        }

        /// <summary>
        /// Gets an enumerator over the treenodes set within this enumerable
        /// </summary>
        /// <returns>An initialised enumerator over the treenodes</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
