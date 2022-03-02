using System;
using System.Windows.Forms;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// TreeListViewBeforeLabelEditEventHandler delegate
    /// </summary>
    public delegate void TreeListViewBeforeLabelEditEventHandler(object sender, TreeListViewBeforeLabelEditEventArgs e);

    /// <summary>
    /// Arguments of a TreeListViewBeforeLabelEdit event.
    /// </summary>
    [Serializable]
    public class TreeListViewBeforeLabelEditEventArgs : TreeListViewLabelEditEventArgs
    {
        #region Properties
        /// <summary>
        /// Gets or sets the index of the subitem
        /// </summary>
        new public int ColumnIndex
        {
            get { return _colindex; }
            set { _colindex = value; }
        }
        private Control _editor;
        /// <summary>
        /// Gets or sets the editor (a TextBox will be displayed if null)
        /// </summary>
        public Control Editor
        {
            get { return _editor; }
            set { _editor = value; }
        }
        #endregion
        #region Constructor
        /// <summary>
        /// Creates a new instance of TreeListViewBeforeLabelEditEventArgs
        /// </summary>
        /// <param name="item"></param>
        /// <param name="column"></param>
        /// <param name="label"></param>
        public TreeListViewBeforeLabelEditEventArgs(TreeListViewItem item, int column, string label)
            : base(item, column, label)
        { }
        #endregion
    }
}
