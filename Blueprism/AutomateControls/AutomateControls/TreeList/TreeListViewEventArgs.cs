using System;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// TreeListViewEventHandler delegate
    /// </summary>
    public delegate void TreeListViewEventHandler(object sender, TreeListViewEventArgs e);

    /// <summary>
    /// Arguments of a TreeListViewEvent
    /// </summary>
    [Serializable]
    public class TreeListViewEventArgs : EventArgs
    {
        private TreeListViewItem _parent;
        private TreeListViewItem _item;
        private TreeListViewAction _action;

        /// <summary>
        /// Item that will be expanded
        /// </summary>
        public TreeListViewItem Item { get { return _item; } }

        /// <summary>
        /// The parent of the item (may not match Item.Parent) if the item has
        /// been removed from the treelistview.
        /// </summary>
        public TreeListViewItem Parent { get { return _parent; } }

        /// <summary>
        /// Action returned by the event
        /// </summary>
        public TreeListViewAction Action { get { return _action; } }

        /// <summary>
        /// Create a new instance of TreeListViewEvent arguments, without a parent.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="action"></param>
        public TreeListViewEventArgs(TreeListViewItem item, TreeListViewAction action)
            : this(item, null, action) { }

        /// <summary>
        /// Create a new instance of TreeListViewEvent arguments with the specified
        /// parent.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parent"></param>
        /// <param name="action"></param>
        public TreeListViewEventArgs(TreeListViewItem item, TreeListViewItem parent, TreeListViewAction action)
        {
            _item = item;
            _parent = parent;
            _action = action;
        }
    }
}
