using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using BluePrism.BPCoreLib.Collections;

namespace AutomateControls.Trees
{
    /// <summary>
    /// Tree which can be filtered on a particular predicate, or
    /// search string.
    /// The model is retained in the background, but the actual nodes
    /// in the tree - ie. that which is displayed is altered.
    /// </summary>
    public class FilterableTreeView : SearchableTreeView
    {
        #region - Class Scope Declarations -

        /// <summary>
        /// Event fired when a filter on the treeview has been applied.
        /// </summary>
        /// <remarks>This event is not fired when the filter is cleared.
        /// See the <see cref="FilterCleared"/> event.</remarks>
        public event EventHandler FilterApplied;

        /// <summary>
        /// Event fired when an active filter on the treeview has been
        /// cleared.
        /// </summary>
        public event EventHandler FilterCleared;

        /// <summary>
        /// Creates a separated copy of the TreeNodeCollection, allowing
        /// the underlying collection to be modified while the nodes are
        /// being iterated over.
        /// </summary>
        private class TreeNodeGenericCollection : ICollection<TreeNode>
        {
            // The backing list of nodes.
            private IList<TreeNode> _nodes;

            /// <summary>
            /// Create
            /// </summary>
            /// <param name="coll"></param>
            public TreeNodeGenericCollection(TreeNodeCollection coll)
            {
                _nodes = new List<TreeNode>();
                foreach (TreeNode n in coll)
                    _nodes.Add(n);
            }

            #region ICollection<TreeNode> Members

            public void Add(TreeNode item)
            {
                _nodes.Add(item);
            }

            public void Clear()
            {
                _nodes.Clear();
            }

            public bool Contains(TreeNode item)
            {
                return _nodes.Contains(item);
            }

            public void CopyTo(TreeNode[] array, int arrayIndex)
            {
                _nodes.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _nodes.Count; }
            }

            public bool IsReadOnly
            {
                get { return _nodes.IsReadOnly; }
            }

            public bool Remove(TreeNode item)
            {
                int count = _nodes.Count;
                _nodes.Remove(item);
                return (count != _nodes.Count);
            }

            #endregion

            #region IEnumerable<TreeNode> Members

            public IEnumerator<TreeNode> GetEnumerator()
            {
                return new clsTypedEnumerator<TreeNode>(_nodes.GetEnumerator());
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _nodes.GetEnumerator();
            }

            #endregion

            #region - Other Methods -

            /// <summary>
            /// Gets this collection into a TreeNode array (for passing
            /// to AddRange() or such like)
            /// </summary>
            /// <returns>The array of treenodes held in this collection</returns>
            public TreeNode[] ToArray()
            {
                TreeNode[] arr = new TreeNode[_nodes.Count];
                _nodes.CopyTo(arr, 0);
                return arr;
            }

            #endregion
        }

        /// <summary>
        /// Class to represent an index-based path to a node in a treeview.
        /// </summary>
        private class NodePath
        {
            // The list of indexes defining this path.
            private IList<int> _path;

            /// <summary>
            /// Creates a new path based on the given node.
            /// </summary>
            /// <param name="node">The node for which the path is required.</param>
            public NodePath(TreeNode node)
            {
                _path = new List<int>();
                while (node != null)
                {
                    _path.Insert(0, node.Index);
                    node = node.Parent;
                }
            }

            /// <summary>
            /// The full path described in this object as a string
            /// </summary>
            public string FullPath
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    string sep = "";
                    foreach (int index in _path)
                    {
                        sb.Append(sep).Append(index);
                        sep = "/";
                    }
                    return sb.ToString();
                }
            }

            /// <summary>
            /// Finds the node described in this path in the specified collection
            /// returning the nearest node to that described if the description
            /// did not match the structure of the given node collection.
            /// </summary>
            /// <param name="nodeColl">The node collection in which the node
            /// described in this path should be found.</param>
            /// <returns>The node described by this path, or the nearest valid
            /// ancestor of that node. eg. if the path was "0/2/5" and the node
            /// at "/0/2" only had 3 children, this method would return the
            /// node at "/0/2" - ie. the parent of the described node.
            /// If the node at "/0" only had 2 children, this method would return
            /// that node as the nearest valid ancestor of the described node.
            /// It will return null if no valid ancestor could be found (eg.
            /// if the node collection is empty, or the first index in the path
            /// is outside the range of the top set of nodes in the collection)
            /// </returns>
            public TreeNode FindNode(TreeNodeCollection nodeColl)
            {
                // If the treeview has no nodes, don't even go there.
                if (nodeColl.Count == 0)
                    return null;

                // No path? Just select the first node in the tree.
                if (_path.Count == 0)
                    return nodeColl[0];

                TreeNode curr = null;
                TreeNodeCollection nodes = nodeColl;
                foreach (int index in _path)
                {
                    // If index is invalid for this node, return the last
                    // successful node.
                    if (index < 0 || index >= nodes.Count)
                        return curr;
                    // Otherwise, get the node and continue down the path
                    curr = nodes[index];
                    nodes = curr.Nodes;
                }

                // We have our node (or the path was empty, and it's still
                // null - either way...) - return it.
                return curr;
            }

            /// <summary>
            /// Selects the node described by this path in the given treeview
            /// </summary>
            /// <param name="tv">The treeview to select the node described
            /// by this path (or the nearest valid ancestor)</param>
            public void Select(TreeView tv)
            {
                tv.SelectedNode = FindNode(tv.Nodes);
            }

            /// <summary>
            /// Gets a string representation of this path.
            /// </summary>
            /// <returns>The path as a string</returns>
            /// <seealso cref="FullPath"/>
            public override string ToString()
            {
                return FullPath;
            }
        }

        /// <summary>
        /// The state of a treeview - including its nodes and the path to
        /// the currently selected node.
        /// </summary>
        private class TreeState
        {
            // The root node into which the nodes from the treeview are stored
            private TreeNode _root;

            // The path to the selected node
            private NodePath _selectedPath;

            /// <summary>
            /// Creates a new treestate based on the given tree, clearing the
            /// given tree in the process.
            /// </summary>
            /// <param name="tree">The tree from which to save the state.</param>
            public TreeState(TreeView tree)
            {
                _root = new TreeNode();
                Update(tree);
            }

            /// <summary>
            /// Restores this state into the given tree, clearing this state
            /// in the process.
            /// </summary>
            /// <param name="tree">The tree to which state should be restored.
            /// </param>
            public void Restore(TreeView tree)
            {
                Restore(tree, true, false);
            }

            /// <summary>
            /// Restores this state into the given tree, clearing this state
            /// in the process.
            /// </summary>
            /// <param name="tree">The tree to which state should be restored.
            /// </param>
            /// <param name="restoreSelectedNode">true to restore the node
            /// which was selected when the state was saved, false to not
            /// restore that selection</param>
            public void Restore(TreeView tree, bool restoreSelectedNode)
            {
                Restore(tree, restoreSelectedNode, false);
            }

            /// <summary>
            /// Restores this state (or a clone of it) into the given tree
            /// </summary>
            /// <param name="tree">The tree to which this state should be
            /// restored.</param>
            /// <param name="withClone">true to restore a clone of the state
            /// to the tree, leaving this state unmolested in the process.
            /// </param>
            public void Restore(TreeView tree, bool restoreSelectedNode, bool withClone)
            {
                if (tree == null)
                    throw new ArgumentNullException(nameof(tree));

                tree.Nodes.Clear();
                foreach (TreeNode node in _root.Nodes)
                {
                    tree.Nodes.Add(withClone ? TreeNodeCloner.Clone(node) : node);
                }
                if (restoreSelectedNode)
                    _selectedPath.Select(tree);
                if (!withClone)
                    _selectedPath = null;
            }

            /// <summary>
            /// Updates this state from the given treeview, clearing the
            /// treeview in the process
            /// </summary>
            /// <param name="tree">The tree to get the new state from.
            /// </param>
            public void Update(TreeView tree)
            {
                Update(tree, false);
            }

            /// <summary>
            /// Updates this state from the given treeview.
            /// </summary>
            /// <param name="tree">The tree to update this state from.
            /// </param>
            /// <param name="withClone">True to clone the nodes from the
            /// given treeview, false to take them (ie. clear the treeview)
            /// </param>
            public void Update(TreeView tree, bool withClone)
            {
                _selectedPath = new NodePath(tree.SelectedNode);
                _root.Nodes.Clear();
                foreach (TreeNode n in new TreeNodeGenericCollection(tree.Nodes))
                {
                    if (withClone)
                    {
                        _root.Nodes.Add(TreeNodeCloner.Clone(n));
                    }
                    else
                    {
                        n.Remove();
                        _root.Nodes.Add(n);
                    }
                }
            }

            /// <summary>
            /// Finds the equivalent node in this saved state to the given
            /// treenode. This searches first by node path (by name rather
            /// than by index), then by tag
            /// </summary>
            /// <param name="n">The node for which the equivalent is required
            /// from this saved state.</param>
            /// <returns>A node in this saved state which has the same path
            /// as the given node. The name and tag of the node is checked
            /// against all nodes in this state with the same path and the
            /// node with the same non-empty name or reference-equal tag is
            /// returned in preference to the others. If no such node is
            /// found,the first one found with the same path is returned.
            /// If no node with the same path is found, this returns null.
            /// </returns>
            public TreeNode FindEquivalentNode(TreeNode n)
            {
                // Create a searcher
                TreeNodeSearcher seeker = new TreeNodeSearcher(_root.Nodes);

                // Search for all nodes matching the same path as the given node.
                TreeNode firstFound = null;
                foreach (TreeNode foundNode in seeker.FindNodesByPath(n.FullPath))
                {
                    // If the name is set and matches the node, return it.
                    if (!string.IsNullOrEmpty(n.Name) && n.Name == foundNode.Name)
                        return foundNode;

                    // If the tag is reference equal, return that node.
                    if (n.Tag == foundNode.Tag)
                        return foundNode;

                    // Save the first found node in case no tag matches
                    if (firstFound == null)
                        firstFound = foundNode;
                }
                // None of the names or tags matched (they may have changed while
                // the filter was working), so return the first one found - there's
                // little else we can do to differentiate it.
                return firstFound;
            }
        }

        #endregion

        #region - Member Variables -

        // Holds the state of the treeview when it is being filtered
        private TreeState _saved;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty filterable treeview.
        /// </summary>
        public FilterableTreeView()
        {
            this.DoubleBuffered = true;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Flag indicating if this treeview is currently filtered or not.
        /// </summary>
        public bool IsFiltered
        {
            get { return (_saved != null); }
        }

        /// <summary>
        /// Gets the source node related to the selected node, ie. the true node
        /// held in the saved state while the filter is in place, or the 
        /// </summary>
        public TreeNode SelectedSourceNode
        {
            get
            {
                TreeNode n = base.SelectedNode;
                if (!IsFiltered || n == null)
                    return n;
                // otherwise we need to find the equivalent node in the
                // original tree.
                return _saved.FindEquivalentNode(n);
            }
        }

        #endregion

        #region - Events -

        /// <summary>
        /// Fires the FilterApplied event.
        /// </summary>
        protected virtual void OnFilterApplied(EventArgs e)
        {
            if (FilterApplied != null)
                FilterApplied(this, e);
        }

        /// <summary>
        /// Fires the FilterCleared event.
        /// </summary>
        protected virtual void OnFilterCleared(EventArgs e)
        {
            if (FilterCleared != null)
                FilterCleared(this, e);
        }
        #endregion

        #region - FilterBy Methods -

        /// <summary>
        /// Filters this treeview based on the given text.
        /// </summary>
        /// <param name="text">The (case insensitive) text that each node
        /// must contain in its Text property in order to be retained in
        /// the tree after the filter.</param>
        public void FilterByText(string text)
        {
            FilterByText(text, true, false);
        }

        /// <summary>
        /// Filters this treeview based on the given text.
        /// </summary>
        /// <param name="text">The (case insensitive) text to search for in
        /// this treeview's nodes.</param>
        /// <param name="partialMatch">true to match any node whose Text
        /// property contains the given text, false to match only nodes
        /// whose Text property equals the given text.
        /// </param>
        public void FilterByText(string text, bool partialMatch)
        {
            FilterByText(text, partialMatch, false);
        }

        /// <summary>
        /// Filters this treeview based on the given text, or clears the
        /// filter if the text given is empty.
        /// </summary>
        /// <param name="text">The text to filter the nodes in this
        /// treeview on. Null or empty string has the effect of clearing
        /// any filter currently set in this treeview.</param>
        /// <param name="partialMatch">true to match any node whose Text
        /// property contains the given text, false to match only nodes
        /// whose Text property equals the given text.
        /// </param>
        /// <param name="caseSensitive">True to require that the case in
        /// the node matches that in the search string; False to allow
        /// for case difference in the node.</param>
        public void FilterByText(string text, bool partialMatch, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(text))
            {
                ClearFilter();
                return;
            }

            StringComparison comp = (caseSensitive
                ? StringComparison.InvariantCulture
                : StringComparison.InvariantCultureIgnoreCase);

            FilterByText(delegate(string nodeText)
            {
                if (partialMatch)
                    return (nodeText.IndexOf(text, comp) != -1);
                return nodeText.Equals(text, comp);
            });
        }

        /// <summary>
        /// Filters the treeview, retaining only those nodes whose
        /// text value satisfies the provided predicate.
        /// </summary>
        /// <param name="pred">The predicate which must be satisfied for
        /// a node to be retained in the tree - the string parameter
        /// represents the Text property of the node begin tested.</param>
        public void FilterByText(Func<string, bool> pred)
        {
            FilterByNode(delegate(TreeNode node)
            {
                return pred(node.Text);
            });
        }

        /// <summary>
        /// Filters the treeview using a predicate on the tags in the
        /// nodes of the tree. Any nodes whose tag satisfies the
        /// predicate are retained in the tree. Note that a tag which
        /// does not have the type used in this method (and the predicate)
        /// will not be tested, and will not be retained in the tree
        /// (unless a descendent has the correct type and satisfies the
        /// predicate).
        /// </summary>
        /// <typeparam name="T">The type of tag to search for.</typeparam>
        /// <param name="pred">The predicate to satisfy to retain the
        /// node in the tree (along with its ancestors). The T parameter
        /// to the predicate will be the tag of the node cast into the
        /// specified type.</param>
        public void FilterByTag<T>(Func<T, bool> pred)
        {
            FilterByNode(delegate(TreeNode node)
            {
                return (node.Tag is T && pred((T)node.Tag));
            });
        }

        /// <summary>
        /// Filters the treeview using a predicate on the treenodes in
        /// the treeview.
        /// </summary>
        /// <param name="pred">The predicate which tests each node in
        /// the treeview to retain it in the tree.</param>
        public void FilterByNode(Func<TreeNode, bool> pred)
        {
            try
            {
                BeginUpdate();

                // Save the state of the tree if we don't already have a saved state.
                // If we have a saved state already, we don't want to overwrite it.
                if (_saved == null)
                {
                    _saved = new TreeState(this);
                }

                // Restore a clone that we can safely alter without messing with
                // the originals (note that any tag reference objects in the nodes
                // will still point to the original objects)
                _saved.Restore(this, true, true);

                // Add a root node into which we can store our results
                TreeNodeSearcher seeker = new TreeNodeSearcher(this);

                // So we want to build the tree of each node found and its parent,
                // but we only want each node once, so add the nodes and all their
                // ancestors into a set.
                clsSet<TreeNode> set = new clsSet<TreeNode>();
                foreach (TreeNode n in seeker.FindNodes(pred))
                {
                    TreeNode curr = n;
                    do
                    {
                        set.Add(curr);
                        curr = curr.Parent;
                    } while (curr != null);
                }

                // We now have every treenode that we want to display, walk the
                // tree and remove any which are not in that set.
                RetainAll(set);
                ExpandAll();

                // Scroll to the top of the treeview by default.

                // Put the selected node at the top of the tree (if one is selected)
                TreeNode sel = SelectedNode;
                if (sel != null)
                    TopNode = sel;
                // otherwise, just scroll to the top of the tree
                else if (Nodes.Count > 0)
                    TopNode = Nodes[0];
            }
            finally
            {
                EndUpdate();
            }
            OnFilterApplied(EventArgs.Empty);
        }

        #endregion

        #region - Update Node methods -

        /// <summary>
        /// Updates the text on the given treenode, ensuring that, if the
        /// treeview is currently being filtered, the saved node is updated
        /// too.
        /// </summary>
        /// <param name="node">The node, within this treeview, whose text must
        /// be updated.</param>
        /// <param name="text">The text to update the node and the saved
        /// node, if this tree is currently filtered.</param>
        public void UpdateText(TreeNode node, string text)
        {
            UpdateNode(node, delegate(TreeNode n) { n.Text = text; });
        }

        /// <summary>
        /// Updates the tag on the given treenode, ensuring that, if a filter
        /// is currently in place, the saved treenode is updated with the new
        /// tag at the same time.
        /// </summary>
        /// <param name="node">The node, within this treeview, whose tag
        /// should be updated.</param>
        /// <param name="tag">The tag to set on the node.</param>
        public void UpdateTag(TreeNode node, object tag)
        {
            UpdateNode(node, delegate(TreeNode n) { n.Tag = tag; });
        }

        /// <summary>
        /// Performs the given action on the tree node and, if a filter is
        /// currently in place on the saved tree node.
        /// The initial node to update must exist in this treeview in order
        /// to work.
        /// </summary>
        /// <param name="n">The node, within this treeview, to update.
        /// </param>
        /// <param name="update">The action to perform on the given treenode
        /// and the saved one, if appropriate.</param>
        public void UpdateNode(TreeNode n, Action<TreeNode> update)
        {
            if (n == null)
                return;
            if (n.TreeView != this)
                throw new ArgumentException("The given node is not in this tree");

            if (IsFiltered)
            {
                TreeNode savedNode = _saved.FindEquivalentNode(n);
                if (savedNode != null)
                    update(savedNode);
            }
            update(n);
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Clears the filter if there is currently one in place.
        /// </summary>
        public void ClearFilter()
        {
            if (_saved != null)
            {
                // Get the node to select after the treeview has been restored
                TreeNode sel = SelectedSourceNode;
                BeginUpdate();
                // Restore the saved state, don't select a node at this stage
                _saved.Restore(this, false);
                _saved = null;
                // Select our saved node
                if (sel != null)
                {
                    SelectedNode = sel;
                    TopNode = sel;
                    // sel.EnsureVisible();
                }
                EndUpdate();
                OnFilterCleared(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Retains all the nodes in this treeview which occur in the given
        /// set of nodes.
        /// </summary>
        /// <param name="set">The nodes which should be retained in this
        /// treeview after this method has completed.</param>
        private void RetainAll(ICollection<TreeNode> set)
        {
            RetainAll(this.Nodes, set);
        }

        /// <summary>
        /// Retains only those nodes in the given set in the specified 
        /// node collection (and its descendents)
        /// </summary>
        /// <param name="nodes">The nodes to operate on</param>
        /// <param name="set">The set of nodes which should be retained.
        /// </param>
        private void RetainAll(TreeNodeCollection nodes, ICollection<TreeNode> set)
        {
            foreach (TreeNode n in new TreeNodeGenericCollection(nodes))
            {
                if (set.Contains(n))
                    RetainAll(n.Nodes, set);
                else
                    n.Remove();
            }
        }

        #endregion
    }
}
