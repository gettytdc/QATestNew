namespace AutomateControls.TreeList
{
    /// <summary>
    /// Collection of selected items in a TreeListView
    /// </summary>
    public class SelectedTreeListViewItemCollection : System.Windows.Forms.ListView.SelectedListViewItemCollection
    {
        #region Properties
        /// <summary>
        /// Gets a TreeListViewItem at the specified index
        /// </summary>
        new public TreeListViewItem this[int index]
        {
            get{return((TreeListViewItem) base[index]);}
        }
        #endregion
        #region Constructor
        /// <summary>
        /// Create a new instance of a SelectedTreeListViewItemCollection
        /// </summary>
        /// <param name="TreeListView"></param>
        public SelectedTreeListViewItemCollection(TreeListView TreeListView) : base((System.Windows.Forms.ListView) TreeListView)
        {
        }
        #endregion
        #region Functions
        /// <summary>
        /// Returns true if the specified item is in the collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(TreeListViewItem item)
        {
            return(base.Contains((System.Windows.Forms.ListViewItem) item));
        }
        /// <summary>
        /// Index of an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(TreeListViewItem item)
        {
            return(base.IndexOf((System.Windows.Forms.ListViewItem) item));
        }

        public TreeListViewItem FirstOrDefault()
        {
            if (Count > 0)
            {
                return (TreeListViewItem)base[0];
            }

            return new TreeListViewItem();
        }
        #endregion
    }
}
