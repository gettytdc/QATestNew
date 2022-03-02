using System;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// Class that contains all informations on an edited item
    /// </summary>
    public struct EditItemInformations
    {
        #region Properties
        internal DateTime CreationTime;
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
        private int _colindex;
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
        /// Creates a new instance of EditItemInformations
        /// </summary>
        /// <param name="item"></param>
        /// <param name="column"></param>
        /// <param name="label"></param>
        public EditItemInformations(TreeListViewItem item, int column, string label)
        {
            _item = item; _colindex = column; _label = label; CreationTime = DateTime.Now;
        }
        #endregion
    }
}
