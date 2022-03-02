using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using BluePrism.Server.Domain.Models;

namespace AutomateControls.Trees
{
    /// <summary>
    /// Enumerator over a tree node collection which can be used non-recursively
    /// </summary>
    public class TreeNodeEnumerator: IEnumerator<TreeNode>
    {
        #region - Class scope declarations -

        /// <summary>
        /// Simple state enum to hold the internal state of the enumerator
        /// </summary>
        private enum State { Before, During, After }

        #endregion

        #region - Member Variables -

        // The stack for keeping track of the treenodes to process.
        private Stack<TreeNode> _stack;

        // The collection of treenodes to iterate
        private TreeNodeCollection _nodes;

        // The current tree node - null before and after enumeration
        private TreeNode _curr;

        // The current state of the enumerator
        private State _state;

        // True to iterate depth first; False to iterate breadth first
        private bool _depthFirst;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new depth-first enumerator over all tree nodes in a collection
        /// </summary>
        /// <param name="nodes">The nodes which should be enumerated over</param>
        public TreeNodeEnumerator(TreeNodeCollection nodes) : this(nodes, true) { }

        /// <summary>
        /// Creates a new enumerator over all tree nodes in a collection
        /// </summary>
        /// <param name="nodes">The node collection to enumerate</param>
        /// <param name="depthFirst">True to enumerate the nodes in depth-first
        /// order; False to enumerate in breadth-first order</param>
        /// <exception cref="ArgumentNullException">If the given node collection was
        /// null</exception>
        public TreeNodeEnumerator(TreeNodeCollection nodes, bool depthFirst)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));

            _stack = new Stack<TreeNode>();
            _nodes = nodes;
            _state = State.Before;
            _depthFirst = depthFirst;
        }

        #endregion

        #region - IEnumerator<TreeNode> and related Members -

        /// <summary>
        /// The current treenode within this enumerator
        /// </summary>
        /// <exception cref="ObjectDisposedException">If this enumerator has been
        /// disposed of</exception>
        /// <exception cref="InvalidOperationException">If the enumerator is before
        /// the first element or after the last element.</exception>
        public TreeNode Current
        {
            get
            {
                AssertNotDisposed();
                if (_state == State.Before) throw new InvalidOperationException(
                    "Enumerator is before first element");
                if (_state == State.After) throw new InvalidOperationException(
                      "Enumerator is after last element");
                return _curr;
            }
        }

        /// <summary>
        /// Disposes of this enumerator
        /// </summary>
        public void Dispose()
        {
            _nodes = null;
        }

        /// <summary>
        /// The current treenode within this enumerator
        /// </summary>
        /// <exception cref="ObjectDisposedException">If this enumerator has been
        /// disposed of</exception>
        /// <exception cref="InvalidOperationException">If the enumerator is before
        /// the first element or after the last element.</exception>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Attempts to move onto the next tree node in the collection, returning a
        /// flag to indicate success.
        /// </summary>
        /// <returns>True if the enumerator has successfully moved onto the next
        /// element; False if there are no more elements to traverse.</returns>
        /// <exception cref="ObjectDisposedException">If this enumerator has been
        /// disposed of</exception>
        /// <exception cref="InvalidStateException">If the internal state of this
        /// enumerator was found to be an invalid value.</exception>
        public bool MoveNext()
        {
            AssertNotDisposed();
            switch (_state)
            {
                case State.After: return false;

                // Set the state and push the first node to the stack
                case State.Before:
                    _state = State.During;
                    if (_nodes.Count > 0)
                        _stack.Push(_nodes[0]);
                    goto case State.During; // drop through

                case State.During:
                    // If there's nothing left to do, retire the enumerator and
                    // return a flag indicating no more nodes
                    if (_stack.Count == 0)
                    {
                        _state = State.After;
                        return false;
                    }
                    _curr = _stack.Pop();
                    // We need to process the node's siblings and children...
                    // If we are going depth first, we need the children at the
                    // top of the stack; breadth first, we need the siblings at
                    // the top of the stack.
                    if (_depthFirst)
                    {
                        if (_curr.NextNode != null) _stack.Push(_curr.NextNode);
                        if (_curr.Nodes.Count > 0) _stack.Push(_curr.Nodes[0]);
                    }
                    else
                    {
                        // breadth first - exactly the same but in reverse order
                        if (_curr.Nodes.Count > 0) _stack.Push(_curr.Nodes[0]);
                        if (_curr.NextNode != null) _stack.Push(_curr.NextNode);
                    }
                    return true;

                default:
                    throw new InvalidStateException(
                        "Invalid internal state: {0}", _state);
            }
        }

        /// <summary>
        /// Resets this enumerator, setting its internal state to 'before the first
        /// element'.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If this enumerator has been
        /// disposed of.</exception>
        public void Reset()
        {
            AssertNotDisposed();
            _state = State.Before;
            _curr = null;
        }

        #endregion

        #region - Other methods -

        /// <summary>
        /// Helper method to ascertain that the object has not been disposed of,
        /// throwing a suitable exception if it finds that the object is disposed.
        /// </summary>
        private void AssertNotDisposed()
        {
            if (_nodes == null) throw new ObjectDisposedException(
                "TreeNodeEnumerator");
        }

        #endregion
    }
}
