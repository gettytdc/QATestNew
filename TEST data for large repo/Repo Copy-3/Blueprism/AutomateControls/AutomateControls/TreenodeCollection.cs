// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//  Jackson Harper (jackson@ximian.com)

// TODO: Sorting

using System;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace AutomateControls
{

    //PJW
    //  [Editor("MonoSWF.Design.TreeNodeCollectionEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
    public class AutomateTreeNodeCollection : IList, ICollection, IEnumerable 
    {

        private static readonly int OrigSize = 50;

        private AutomateTreeNode owner;
        private int count;
        private AutomateTreeNode [] nodes;

        private AutomateTreeNodeCollection ()
        {
        }

        internal AutomateTreeNodeCollection (AutomateTreeNode owner)
        {
            this.owner = owner;
            nodes = new AutomateTreeNode [OrigSize];
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public int Count 
        {
            get { return count; }
        }

        public bool IsReadOnly 
        {
            get { return false; }
        }

        bool ICollection.IsSynchronized 
        {
            get { return false; }
        }

        object ICollection.SyncRoot 
        {
            get { return this; }
        }

        bool IList.IsFixedSize 
        {
            get { return false; }
        }

        object IList.this [int index] 
        {
            get 
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException ("index");
                return nodes [index];
            }
            set 
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException ("index");
                AutomateTreeNode node = (AutomateTreeNode) value;
                SetupNode (node);
                nodes [index] = node;
            }
        }

        public virtual AutomateTreeNode this [int index] 
        {
            get 
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException ("index");
                return nodes [index];
            }
            set 
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException ("index");
                SetupNode (value);
                nodes [index] = value;
            }
        }

        public virtual AutomateTreeNode Add (string text)
        {
            AutomateTreeNode res = new AutomateTreeNode (text);
            Add (res);
            return res;
        }

        public virtual int Add (AutomateTreeNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            int res;
            AutomateTreeView tree_view = null;

            if (tree_view != null && tree_view.Sorted) 
            {
                res = AddSorted (node);
            } 
            else 
            {
                if (count >= nodes.Length)
                    Grow ();
                nodes [count++] = node;
                res = count;
            }

            SetupNode (node);

            return res;
        }

        public virtual void AddRange (AutomateTreeNode [] nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("node");

            // We can't just use Array.Copy because the nodes also
            // need to have some properties set when they are added.
            for (int i = 0; i < nodes.Length; i++)
                Add (nodes [i]);
        }

        public virtual void Clear ()
        {
            for (int i = 0; i < count; i++)
                RemoveAt (i, false);
            
            Array.Clear (nodes, 0, count);
            count = 0;

            AutomateTreeView tree_view = null;
            if (owner != null) 
            {
                tree_view = owner.TreeView;
                if (owner.IsRoot)
                    tree_view.top_node = null;
                if (tree_view != null) 
                {
                    tree_view.UpdateBelow (owner);
                    tree_view.RecalculateVisibleOrder (owner);
                }
            }
        }

        public bool Contains (AutomateTreeNode node)
        {
            return (Array.BinarySearch (nodes, node) > 0);
        }

        public void CopyTo (Array dest, int index)
        {
            nodes.CopyTo (dest, index);
        }

        public IEnumerator GetEnumerator ()
        {
            return new TreeNodeEnumerator (this);
        }

        public int IndexOf (AutomateTreeNode node)
        {
            return Array.IndexOf (nodes, node);
        }

        public virtual void Insert (int index, AutomateTreeNode node)
        {
            if (count >= nodes.Length)
                Grow ();

            Array.Copy (nodes, index, nodes, index + 1, count - index);
            nodes [index] = node;
            count++;

            SetupNode (node);
        }

        public void Remove (AutomateTreeNode node)
        {
            int index = IndexOf (node);
            if (index > 0)
                RemoveAt (index);
        }

        public virtual void RemoveAt (int index)
        {
            RemoveAt (index, true);
        }

        private void RemoveAt (int index, bool update)
        {
            AutomateTreeNode removed = nodes [index];
            AutomateTreeNode prev = GetPrevNode (removed);
            AutomateTreeNode new_selected = null;
            bool visible = removed.IsVisible;

            AutomateTreeView  tree_view = null;
            if (owner != null)
                tree_view = owner.TreeView;

            if (tree_view != null) 
            {
                tree_view.RecalculateVisibleOrder (prev);
                if (removed == tree_view.top_node) 
                {

                    if (removed.IsRoot) 
                    {
                        tree_view.top_node = null;
                    } 
                    else 
                    {
                        OpenTreeNodeEnumerator oe = new OpenTreeNodeEnumerator (removed);
                        if (oe.MovePrevious () && oe.MovePrevious ()) 
                        {
                            tree_view.top_node = oe.CurrentNode;
                        } 
                        else 
                        {
                            removed.is_expanded = false;
                            oe = new OpenTreeNodeEnumerator (removed);
                            if (oe.MoveNext () && oe.MoveNext ()) 
                            {
                                tree_view.top_node = oe.CurrentNode;
                            } 
                            else 
                            {
                                tree_view.top_node = null;
                            }
                        }
                    }
                }
                if (removed == tree_view.selected_node) 
                {
                    OpenTreeNodeEnumerator oe = new OpenTreeNodeEnumerator (removed);
                    if (oe.MoveNext () && oe.MoveNext ()) 
                    {
                        new_selected = oe.CurrentNode;
                    } 
                    else 
                    {
                        oe = new OpenTreeNodeEnumerator (removed);
                        oe.MovePrevious ();
                        new_selected = oe.CurrentNode;
                    }
                }
            }

            
            Array.Copy (nodes, index + 1, nodes, index, count - index);
            count--;
            if (nodes.Length > OrigSize && nodes.Length > (count * 2))
                Shrink ();

            if (tree_view != null && new_selected != null) 
            {
                tree_view.SelectedNode = new_selected;
            }

            AutomateTreeNode parent = removed.parent;
            removed.parent = null;

            if (tree_view != null && visible) 
            {
                tree_view.RecalculateVisibleOrder (prev);
                tree_view.UpdateScrollBars ();
                tree_view.UpdateBelow (parent);
            }
        }

        private AutomateTreeNode GetPrevNode (AutomateTreeNode node)
        {
            OpenTreeNodeEnumerator one = new OpenTreeNodeEnumerator (node);

            if (one.MovePrevious () && one.MovePrevious ())
                return one.CurrentNode;
            return null;
        }

        private void SetupNode (AutomateTreeNode node)
        {
            // Remove it from any old parents
            node.Remove ();

            node.parent = owner;

            AutomateTreeView  tree_view = null;
            if (owner != null)
                tree_view = owner.TreeView;

            if (tree_view != null) 
            {
                AutomateTreeNode prev = GetPrevNode (node);

                if (tree_view.top_node == null)
                    tree_view.top_node = node;

                if (node.IsVisible)
                    tree_view.RecalculateVisibleOrder (prev);
                tree_view.UpdateScrollBars ();
            }

            if (owner != null && tree_view != null && (owner.IsExpanded || owner.IsRoot)) 
            {
                // tree_view.UpdateBelow (owner);
                tree_view.UpdateNode (owner);
                tree_view.UpdateNode (node);
            } 
            else if (owner != null && tree_view != null) 
            {
                tree_view.UpdateBelow (owner);
            }
        }

        int IList.Add (object node)
        {
            return Add ((AutomateTreeNode) node);
        }

        bool IList.Contains (object node)
        {
            return Contains ((AutomateTreeNode) node);
        }
        
        int IList.IndexOf (object node)
        {
            return IndexOf ((AutomateTreeNode) node);
        }

        void IList.Insert (int index, object node)
        {
            Insert (index, (AutomateTreeNode) node);
        }

        void IList.Remove (object node)
        {
            Remove ((AutomateTreeNode) node);
        }

        private int AddSorted (AutomateTreeNode node)
        {
            if (count >= nodes.Length)
                Grow ();

            CompareInfo compare = Application.CurrentCulture.CompareInfo;
            int pos = 0;
            bool found = false;
            for (int i = 0; i < count; i++) 
            {
                pos = i;
                int comp = compare.Compare (node.Text, nodes [i].Text);
                if (comp < 0) 
                {
                    found = true;
                    break;
                }
            }

            // Stick it at the end
            if (!found)
                pos = count;

            // Move the nodes up and adjust their indices
            for (int i = count - 1; i >= pos; i--) 
            {
                nodes [i + 1] = nodes [i];
            }
            count++;
            nodes [pos] = node;

            return count;
        }

        // Would be nice to do this without running through the collection twice
        internal void Sort () 
        {

            Array.Sort (nodes, 0, count, new TreeNodeComparer (Application.CurrentCulture.CompareInfo));

            for (int i = 0; i < count; i++) 
            {
                nodes [i].Nodes.Sort ();
            }

            // No null checks since sort can only be called from the treeviews root node collection
            owner.TreeView.RecalculateVisibleOrder (owner);
            owner.TreeView.UpdateScrollBars ();
        }

        private void Grow ()
        {
            AutomateTreeNode [] nn = new AutomateTreeNode [nodes.Length + 50];
            Array.Copy (nodes, nn, nodes.Length);
            nodes = nn;
        }

        private void Shrink ()
        {
            int len = (count > OrigSize ? count : OrigSize);
            AutomateTreeNode [] nn = new AutomateTreeNode [len];
            Array.Copy (nodes, nn, count);
            nodes = nn;
        }

        
        internal class TreeNodeEnumerator : IEnumerator 
        {

            private AutomateTreeNodeCollection collection;
            private int index = -1;

            public TreeNodeEnumerator (AutomateTreeNodeCollection collection)
            {
                this.collection = collection;
            }

            public object Current 
            {
                get { return collection [index]; }
            }

            public bool MoveNext ()
            {
                if (index + 1 >= collection.Count)
                    return false;
                index++;
                return true;
            }

            public void Reset ()
            {
                index = 0;
            }
        }

        private class TreeNodeComparer : IComparer 
        {

            private CompareInfo compare;
        
            public TreeNodeComparer (CompareInfo compare)
            {
                this.compare = compare;
            }
        
            public int Compare (object x, object y)
            {
                AutomateTreeNode l = (AutomateTreeNode) x;
                AutomateTreeNode r = (AutomateTreeNode) y;
                int res = compare.Compare (l.Text, r.Text);

                return (res == 0 ? l.Index - r.Index : res);
            }
        }
    }
}

