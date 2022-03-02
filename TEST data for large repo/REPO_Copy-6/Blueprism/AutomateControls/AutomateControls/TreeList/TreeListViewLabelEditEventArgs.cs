using System;
using System.ComponentModel;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// TreeListViewItemLabelEditHandler delegate
    /// </summary>
    public delegate void TreeListViewLabelEditEventHandler(object sender, TreeListViewLabelEditEventArgs e);

    /// <summary>
    /// Arguments of a TreeListViewLabelEdit event.
    /// </summary>
    [Serializable]
    public class TreeListViewLabelEditEventArgs : CancelEventArgs
    {
        #region Properties
        private string _label;
        /// <summary>
        /// Gets the label of the subitem
        /// </summary>
        public string Label
        {
            get { return _label; }
        }
        private TreeListViewItem _item;
        /// <summary>
        /// Gets the item being edited
        /// </summary>
        public TreeListViewItem Item
        {
            get { return _item; }
        }
        internal int _colindex;
        /// <summary>
        /// Gets the number of the subitem
        /// </summary>
        public int ColumnIndex
        {
            get { return _colindex; }
        }
        #endregion
        #region Constructor
        /// <summary>
        /// Creates a new instance of TreeListViewLabelEditEventArgs
        /// </summary>
        /// <param name="item"></param>
        /// <param name="column"></param>
        /// <param name="label"></param>
        public TreeListViewLabelEditEventArgs(TreeListViewItem item, int column, string label)
            : base()
        {
            _item = item; _colindex = column; _label = label;
        }
        #endregion
    }
}
