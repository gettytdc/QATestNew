using System;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// TreeListViewCancelEventHandler delegate
    /// </summary>
    public delegate void TreeListViewCancelEventHandler(object sender, TreeListViewCancelEventArgs e);

    /// <summary>
    /// Arguments of a TreeListViewCancelEventArgs
    /// </summary>
    [Serializable]
    public class TreeListViewCancelEventArgs : TreeListViewEventArgs
    {
        private bool _cancel = false;
        /// <summary>
        /// True -> the operation is canceled
        /// </summary>
        public bool Cancel
        {
            get { return (_cancel); }
            set { _cancel = value; }
        }
        /// <summary>
        /// Create a new instance of TreeListViewCancelEvent arguments
        /// </summary>
        /// <param name="item"></param>
        /// <param name="action"></param>
        public TreeListViewCancelEventArgs(TreeListViewItem item, TreeListViewAction action) :
            base(item, action)
        { }
    }
}
