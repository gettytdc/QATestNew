using System.Windows.Forms;

namespace BluePrism.CharMatching.UI.Designer
{
    /// <summary>
    /// Drop down control for a grid spy region schema
    /// </summary>
    public partial class GridSpyRegionSchemaEditorDropDown : UserControl
    {
        #region - Member Variables -

        // The schema being edited
        private GridSpyRegionSchema _schema;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new, empty schema editor drop down
        /// </summary>
        public GridSpyRegionSchemaEditorDropDown() : this(null) { }

        /// <summary>
        /// Creates a new schema editor drop down for the given schema
        /// </summary>
        /// <param name="schema">The schema to be edited.</param>
        public GridSpyRegionSchemaEditorDropDown(GridSpyRegionSchema schema)
        {
            _schema = schema;
            InitializeComponent();
        }

        #endregion

        #region - Command Event Handlers -

        /// <summary>
        /// Handles the 'Add Row' link being invoked
        /// </summary>
        private void HandleAddRow(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            _schema.AddRow();
            CheckEnabled();
        }

        /// <summary>
        /// Handles the 'Add Column' link being invoked
        /// </summary>
        private void HandleAddColumn(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            _schema.AddColumn();
            CheckEnabled();
        }

        /// <summary>
        /// Handles the 'Delete Row' link being invoked
        /// </summary>
        private void HandleDeleteRow(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            _schema.DeleteLastRow();
            CheckEnabled();
        }

        /// <summary>
        /// Handles the 'Delete Column' link being invoked
        /// </summary>
        private void HandleDeleteColumn(
            object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            _schema.DeleteLastColumn();
            CheckEnabled();
        }

        /// <summary>
        /// Handles the 'Add Row' link being invoked
        /// </summary>
        private void HandleEdit(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            using (GridSpyRegionSchemaEditorForm f =
                new GridSpyRegionSchemaEditorForm())
            {
                f.Schema = _schema.Clone();
                if (f.ShowDialog() == DialogResult.OK)
                    _schema.CopySchemaFrom(f.Schema);
            }
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Ensures that the 'delete' links are disabled if there is only one row
        /// or column left.
        /// </summary>
        private void CheckEnabled()
        {
            llDelRow.Enabled = (_schema.RowCount > 1);
            llDelCol.Enabled = (_schema.ColumnCount > 1);
        }

        #endregion
    }
}
