using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BluePrism.BPCoreLib.Collections;

namespace AutomateControls.Trees
{
    /// <summary>
    /// A TreeView which provides a few extended mechanisms with which the nodes
    /// within it can be searched.
    /// </summary>
    public class SearchableTreeView : FlickerFreeTreeView
    {
        // The searcher used to search this treeview
        private TreeNodeSearcher _seeker;

        /// <summary>
        /// Creates a new empty treeview
        /// </summary>
        public SearchableTreeView()
        {
            _seeker = new TreeNodeSearcher(this.Nodes);
        }

        #region - The Seekers -

        /// <summary>
        /// Finds the TreeNode within this tree with the given path.
        /// </summary>
        /// <param name="path">The path of the required treenode</param>
        /// <returns>The first node encountered which is associated with the given
        /// path, null if no such node was found.</returns>
        public TreeNode FindNodeByPath(string path)
        {
            return CollectionUtil.First(_seeker.FindNodesByPath(path));
        }

        /// <summary>
        /// Finds all of the TreeNodes within this tree with the given path
        /// </summary>
        /// <param name="path">The path to search for treenodes for</param>
        /// <returns>The collection of nodes which correspond to the given path
        /// </returns>
        public ICollection<TreeNode> FindNodesByPath(string path)
        {
            return _seeker.FindNodesByPath(path);
        }

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
            return _seeker.FindNodeByTag<T>(pred);
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
            return _seeker.FindNodeWithTag(tag);
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
            return _seeker.FindNodesByTag<T>(pred);
        }

        /// <summary>
        /// Finds the first node in the tree which satisfies a predicate
        /// </summary>
        /// <param name="pred">The predicate determining which node to return.
        /// </param>
        /// <returns>The first node found in a depth-first search of the tree that
        /// satisfies the provided predicate.</returns>
        public TreeNode FindNode(Func<TreeNode, bool> pred)
        {
            return _seeker.FindNode(pred);
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
            return _seeker.FindNodes(pred);
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
            return _seeker.FindNodesByText(text);
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
            return _seeker.FindNodesByText(text, partialMatch);
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
            return _seeker.FindNodesByText(text, partialMatch, caseSensitive);
        }

        #endregion

    }
}
