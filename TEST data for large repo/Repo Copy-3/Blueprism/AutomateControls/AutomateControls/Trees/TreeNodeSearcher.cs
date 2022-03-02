using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BluePrism.BPCoreLib.Collections;

namespace AutomateControls.Trees
{
    public class TreeNodeSearcher
    {
        private TreeNodeCollection _nodes;

        public TreeNodeSearcher(TreeView tree) : this(tree.Nodes) { }

        public TreeNodeSearcher(TreeNodeCollection nodes)
        {
            _nodes = nodes;
        }

        public TreeNodeCollection Nodes
        {
            get { return _nodes; }
        }

        #region - Find Methods -

        /// <summary>
        /// Finds the first node in this tree whose tag satisfies the given
        /// predicate. Note that any nodes with tags which cannot be cast
        /// into the specified type will be ignored for the purposes of
        /// this search.
        /// </summary>
        /// <typeparam name="T">The type of tag expected in the nodes to be
        /// searched.</typeparam>
        /// <param name="pred">The predicate delegate which will test each
        /// node's tag for a match</param>
        /// <returns>The first node which satisfies the given predicate in
        /// a depth first search of this tree, or null if no nodes in the
        /// tree satisfied the predicate.</returns>
        public TreeNode FindNodeByTag<T>(Func<T, bool> pred)
        {
            return FindNodeByTag(this.Nodes, pred);
        }

        /// <summary>
        /// Finds the first node in a depth first search of the given node
        /// collection whose tag satisfies the given predicate. Note that
        /// any nodes with tags which cannot be cast into the specified type
        /// will be ignored for the purposes of this search.
        /// </summary>
        /// <typeparam name="T">The type of tag expected in the nodes to be
        /// searched.</typeparam>
        /// <param name="pred">The predicate delegate which will test each
        /// node's tag for a match</param>
        /// <returns>The first node which satisfies the given predicate in
        /// a depth first search of this tree, or null if no nodes in the
        /// tree satisfied the predicate.</returns>
        private TreeNode FindNodeByTag<T>(TreeNodeCollection nodes, Func<T, bool> pred)
        {
            foreach (TreeNode n in nodes)
            {
                if (n.Tag is T && pred((T)n.Tag))
                    return n;
                if (n.Nodes.Count > 0)
                {
                    TreeNode found = FindNodeByTag(n.Nodes, pred);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the first node encountered in a depth first search of this
        /// tree which has a tag which matches the given tag according to the
        /// rules of <see cref="Object.Equals"/>.
        /// </summary>
        /// <param name="tag">The tag to search this treeview for.</param>
        /// <returns>The first treenode found in this treeview whose tag was
        /// equal to the given object.</returns>
        public TreeNode FindNodeWithTag(object tag)
        {
            return FindNodeByTag<object>(delegate(object o)
            {
                return object.Equals(tag, o);
            });
        }

        /// <summary>
        /// Finds all the nodes in the tree whose tags satisfy the given
        /// predicate.
        /// </summary>
        /// <typeparam name="T">The type of tag to check in the nodes
        /// </typeparam>
        /// <param name="pred">The predicate which a node's tag must
        /// satisfy to be included in the result.</param>
        /// <returns></returns>
        public ICollection<TreeNode> FindNodesByTag<T>(Func<T, bool> pred)
        {
            return FindNodes(delegate(TreeNode node)
            {
                if (node.Tag is T)
                    return pred((T)node.Tag);
                return false;
            });
        }

        /// <summary>
        /// Finds the first node in a depth-first search which matches a given
        /// predicate
        /// </summary>
        /// <param name="pred">The predicate which determines which node to return.
        /// </param>
        /// <returns>The first node found in a depth first search of the tree which
        /// matches the given predicate</returns>
        public TreeNode FindNode(Func<TreeNode, bool> pred)
        {
            return FindNode(this.Nodes, pred);
        }

        /// <summary>
        /// Finds the first node in a depth-first search of a node collection, which
        /// matches a given predicate
        /// </summary>
        /// <param name="nodes">The node collection to search in</param>
        /// <param name="pred">The predicate which determines which node to return.
        /// </param>
        /// <returns>The first node found in a depth first search of the node
        /// collection which matches the given predicate</returns>
        private TreeNode FindNode(TreeNodeCollection nodes, Func<TreeNode, bool> pred)
        {
            foreach (TreeNode n in nodes)
            {
                if (pred(n))
                    return n;
                if (n.Nodes.Count > 0)
                {
                    TreeNode found = FindNode(n.Nodes, pred);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        /// <summary>
        /// Filters all the treenodes into a collection, retaining those which
        /// satisfy a given predicate.
        /// </summary>
        /// <param name="pred">The predicate which a tree node must satisfy to
        /// be included in the resultant collection.</param>
        /// <returns>A collection of treenodes which </returns>
        public ICollection<TreeNode> FindNodes(Func<TreeNode, bool> pred)
        {
            return FindNodesInto(this.Nodes, pred, new List<TreeNode>());
        }

        /// <summary>
        /// Filters all the treenodes, within a tree node collection, which
        /// satisfy a given predicate into the supplied collection.
        /// </summary>
        /// <param name="nodes">The node collection to recursively search for
        /// matching nodes.</param>
        /// <param name="pred">The predicate which each node must satisfy to
        /// be included in the resultant collection.</param>
        /// <param name="into">The collection into which nodes which satisfy
        /// the predicate should be added.</param>
        /// <returns>The collection of treenodes that this method has added
        /// to in its traversal of the node collection.</returns>
        private ICollection<TreeNode> FindNodesInto(
            TreeNodeCollection nodes, Func<TreeNode, bool> pred, ICollection<TreeNode> into)
        {
            foreach (TreeNode n in nodes)
            {
                if (pred(n))
                    into.Add(n);
                if (n.Nodes.Count > 0)
                    FindNodesInto(n.Nodes, pred, into);
            }
            return into;
        }

        /// <summary>
        /// Finds all the nodes whose text value contains the given text
        /// (case insensitive)
        /// </summary>
        /// <param name="text">The text to search for in this tree.</param>
        /// <returns>Any node whose text value contains the given text.
        /// </returns>
        public ICollection<TreeNode> FindNodesByText(string text)
        {
            return FindNodesByText(text, true, false);
        }

        /// <summary>
        /// Finds all the nodes whose text value contains the given text
        /// (case insensitive)
        /// </summary>
        /// <param name="text">The text to search for in this tree.</param>
        /// <returns>Any node whose text value contains the given text.
        /// </returns>
        public ICollection<TreeNode> FindNodesByText(string text, bool partialMatch)
        {
            return FindNodesByText(text, partialMatch, false);
        }

        /// <summary>
        /// Finds all the nodes whose text value contains the given text
        /// (case insensitive)
        /// </summary>
        /// <param name="text">The text to search for in this tree.</param>
        /// <returns>Any node whose text value contains the given text.
        /// </returns>
        public ICollection<TreeNode> FindNodesByText(
            string text, bool partialMatch, bool caseSensitive)
        {
            StringComparison comp = (caseSensitive
                ? StringComparison.InvariantCulture
                : StringComparison.InvariantCultureIgnoreCase);

            return FindNodes(delegate(TreeNode n)
            {
                if (partialMatch)
                    return (n.Text.IndexOf(text, comp) != -1);
                return n.Text.Equals(text, comp);
            });
        }

        /// <summary>
        /// Finds the collection of nodes which are represented by the
        /// given path. Note that there may be more than one since a node's
        /// name does not have to be unique at its level.
        /// </summary>
        /// <param name="path">The path to search for nodes with</param>
        /// <returns>The collection of nodes corresponding to the supplied
        /// path.</returns>
        public ICollection<TreeNode> FindNodesByPath(string path)
        {
            if (_nodes.Count == 0)
                return GetEmpty.ICollection<TreeNode>();
            TreeView tv = _nodes[0].TreeView;
            return FindNodesByPath(path, tv == null ? "\\" : tv.PathSeparator);
        }

        /// <summary>
        /// Finds the nodes in the contained treenodecollection which match the
        /// given path.
        /// </summary>
        /// <param name="path">The path to search in the held treenodecollection
        /// for.</param>
        /// <param name="pathSeparator">The path separator to use in the path to
        /// separate different path elements.</param>
        /// <returns>The collection of treenodes whose path matched the given
        /// path.</returns>
        public ICollection<TreeNode> FindNodesByPath(string path, string pathSeparator)
        {
            string[] pathElements =
                path.Split(new string[] { pathSeparator }, StringSplitOptions.None);

            IBPSet<TreeNode> currLevelNodes = null;
            foreach (string pathEntry in pathElements)
            {
                // First time in, set the current level nodes to all those that
                // match the first entry in the path
                if (currLevelNodes == null)
                {
                    currLevelNodes = new clsSet<TreeNode>(FindNodesWithText(_nodes, pathEntry));
                    // Move straight to the next entry.
                    continue;
                }
                // after that, go through each of the matching nodes from the
                // previous iteration and get all the nodes from then which match
                // the subsequent entry in the path
                ICollection<TreeNode> prevLevelNodes = currLevelNodes;
                currLevelNodes = new clsSet<TreeNode>();
                foreach (TreeNode n in prevLevelNodes)
                {
                    currLevelNodes.Union(FindNodesWithText(n.Nodes, pathEntry));
                }
            }
            // At this point we should have either a set of nodes which matched the
            // path, or null - null only if there were no path elements.
            return (currLevelNodes ?? GetEmpty.ICollection<TreeNode>());

        }

        /// <summary>
        /// Finds the nodes immediately in the given treenode collection which have
        /// the supplied text.
        /// </summary>
        /// <param name="nodes">The node collection to search</param>
        /// <param name="text">The text to search for.</param>
        /// <returns>The nodes in the node collection with the same text as that
        /// provided (exactly the same - case sensitive, no partial matches).</returns>
        private ICollection<TreeNode> FindNodesWithText(TreeNodeCollection nodes, string text)
        {
            List<TreeNode> list = new List<TreeNode>();
            foreach (TreeNode n in nodes)
            {
                if (n.Text == text)
                    list.Add(n);
            }
            return list;
        }
    
        #endregion
    }
}
