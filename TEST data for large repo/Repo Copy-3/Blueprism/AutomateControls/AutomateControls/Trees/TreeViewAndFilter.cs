using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;

namespace AutomateControls.Trees
{
    public partial class TreeViewAndFilter : UserControl
    {
        #region - Member Variables -

        private TreeNodeFilter _filterer;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new treeview and filter combination
        /// </summary>
        public TreeViewAndFilter()
        {
            InitializeComponent();

            // Some sensible defaults
            this.HideSelection = false;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Direct access to the treeview which is held within this panel
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FilterableTreeView Tree
        {
            get { return tvTree; }
        }

        /// <summary>
        /// The delegate used to filter the tree when a filter search term is applied
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeNodeFilter Filterer
        {
            get { return _filterer; }
            set { _filterer = value; }
        }

        /// <summary>
        /// Gets or sets the currently selected tag - ie. gets the tag of the
        /// selected node, or sets the selected node to that with the specified tag.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedTag
        {
            get { return SelectedNode?.Tag; }
            set { SelectedNode = tvTree.FindNodeWithTag(value); }
        }


        #region - Pass-thru Properties -

        /// <summary>
        /// Hides the selection on the treeview when it does not have focus
        /// </summary>
        [DefaultValue(false)]
        public bool HideSelection
        {
            get { return tvTree.HideSelection; }
            set { tvTree.HideSelection = value; }
        }

        /// <summary>
        /// Sets the tree to track the mouse movement and emphasise the node under
        /// the cursor
        /// </summary>
        [DefaultValue(false)]
        public bool HotTracking
        {
            get { return tvTree.HotTracking; }
            set { tvTree.HotTracking = value; }
        }

        /// <summary>
        /// The indent to use between nodes and their children
        /// </summary>
        public int Indent
        {
            get { return tvTree.Indent; }
            set { tvTree.Indent = value; }
        }

        /// <summary>
        /// Gets or sets the node which is currently selected in the treeview
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeNode SelectedNode
        {
            get { return tvTree.SelectedNode; }
            set { tvTree.SelectedNode = value; }
        }

        #endregion

        #endregion

        #region - Event Handlers -

        /// <summary>
        /// Handles the filter being applied
        /// </summary>
        private void HandleApplyFilter(object sender, FilterEventArgs e)
        {
            ApplyFilter(e.FilterText);
        }

        /// <summary>
        /// Handles the search button being pressed
        /// </summary>
        private void HandleSearchClick(object sender, EventArgs e)
        {
            txtFilter.SelectAll();
        }

        /// <summary>
        /// Handles the filter being cleared.
        /// </summary>
        private void HandleClearFilterClick(object sender, EventArgs e)
        {
            tvTree.ClearFilter();
        }

        #endregion

        #region - Designer Helper Methods -

        /// <summary>
        /// Resets the indent value of this treeview and filter combo (actually
        /// just passes through to the embedded treeview using reflection)
        /// </summary>
        private void ResetIndent()
        {
            MethodInfo reset = typeof(TreeView).GetMethod("ResetIndent",
                BindingFlags.NonPublic | BindingFlags.Instance);
            reset.Invoke(tvTree, null);
        }

        /// <summary>
        /// Checks if the Indent property should be serialized for this control or
        /// not (actually just passes through to the embedded treeview using
        /// reflection)
        /// </summary>
        /// <returns>true if the indent is not at the default value according to
        /// TreeView rules and thus the value should be serialized; false if the
        /// indent value is currently set to the default so it does not need to be.
        /// </returns>
        private bool ShouldSerializeIndent()
        {
            MethodInfo reset = typeof(TreeView).GetMethod("ShouldSerializeIndent",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)reset.Invoke(tvTree, null);
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Re-applies the given filter
        /// </summary>
        public void ReapplyFilter()
        {
            ApplyFilter(txtFilter.Text);
        }

        /// <summary>
        /// Applies the given text as a filter to this treeview and filter
        /// </summary>
        /// <param name="text">The text to use when applying the filter</param>
        private void ApplyFilter(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                tvTree.ClearFilter();
                return;
            }

            TreeNodeFilter filter = _filterer;
            if (filter != null)
            {
                tvTree.FilterByNode(delegate (TreeNode n) {
                    return filter(text, n);
                });
            }
            else // default to just searching the text of the node
            {
                tvTree.FilterByText(text);
            }

        }

        #endregion

    }
}
