using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using BluePrism.BPCoreLib;

namespace BluePrism.CharMatching.UI.Designer
{
    /// <summary>
    /// Editor control for editing grid spy region schemas
    /// </summary>
    public partial class GridSpyRegionSchemaEditor : UserControl
    {
        #region - Member Variables -

        // The schema currently represented by this control
        private GridSpyRegionSchema _schema;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty schema editor
        /// </summary>
        public GridSpyRegionSchemaEditor() : this(null) { }

        /// <summary>
        /// Creates a new schema editor for the given schema
        /// </summary>
        /// <param name="schema">The schema to edit in this editor, null to create the
        /// editor without a schema</param>
        public GridSpyRegionSchemaEditor(GridSpyRegionSchema schema)
        {
            InitializeComponent();
            cmbVectorType.SelectedIndex = 0;
            Schema = schema;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The currently selected vector list view item, or null if none is selected
        /// </summary>
        private ListViewItem SelectedItem
        {
            get
            {
                if (lstVectors.SelectedItems.Count == 0)
                    return null;
                return lstVectors.SelectedItems[0];
            }
        }

        /// <summary>
        /// The currently selected vector, or null if no vector is currently selected.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GridVector SelectedVector
        {
            get
            {
                ListViewItem item = SelectedItem;
                return (item == null ? null : (GridVector) item.Tag);
            }
        }

        /// <summary>
        /// The schema represented by this editor control
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GridSpyRegionSchema Schema
        {
            get { return _schema; }
            set
            {
                if (_schema == value)
                    return;

                _schema = value;
                lstVectors.BeginUpdate();
                try
                {
                    if (_schema == null)
                    {
                        DisplayedVectors = null;
                    }
                    else
                    {
                        DisplayedVectors = (IsShowingColumns ?
                            (ICollection)_schema.Columns :
                            (ICollection)_schema.Rows
                        );
                        gpSizeType.Enabled = false;
                    }
                }
                finally
                {
                    lstVectors.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Flag indicating if this editor is currently showing columns or rows.
        /// </summary>
        /// <returns>True if the editor is currently displaying the columns of the
        /// held schema; False if it is currently showing rows.</returns>
        private bool IsShowingColumns
        {
            get { return ((string)cmbVectorType.SelectedItem == "Columns"); }
        }

        /// <summary>
        /// The currently displayed vectors, if showing columns this will be the
        /// columns held on the schema, otherwise, this will be its rows.
        /// </summary>
        private ICollection DisplayedVectors
        {
            get
            {
                return (IsShowingColumns
                    ? (ICollection) _schema.Columns : (ICollection) _schema.Rows);
            }
            set
            {
                lstVectors.BeginUpdate();
                try
                {
                    lstVectors.Items.Clear();
                    if (value == null)
                        return;
                    int number = 0;
                    foreach (GridVector v in value)
                    {
                        ListViewItem item = lstVectors.Items.Add((++number).ToString());
                        item.SubItems.AddRange(new string[] { v.SizeTypeLabel, v.ValueString });
                        item.Tag = v;
                    }
                }
                finally
                {
                    lstVectors.EndUpdate();
                }
            }
        }

        #endregion

        #region - Event Handlers -

        /// <summary>
        /// Handles the currently selected vector being changed.
        /// </summary>
        private void HandleVectorSelectionChanged(object sender, EventArgs e)
        {
            GridVector v = SelectedVector;
            if (v == null)
            {
                gpSizeType.Enabled = false;
            }
            else
            {
                gpSizeType.Enabled = true;

                if (v.SizeType == VectorSizeType.Absolute)
                {
                    // note that it has to be this order so that the radio
                    // event handler doesn't overwrite the model's value
                    numAbsolute.Value = v.Value;
                    rbAbsolute.Checked = true;
                }
                else
                {
                    numProportion.Value = v.Value;
                    rbProportion.Checked = true;
                }
            }
        }

        /// <summary>
        /// Handles the size type being changed (either of the size type radio buttons
        /// being checked / unchecked). Note that this only deals with 'checked'
        /// events, assuming that any unchecked events can be safely ignored due to
        /// there being a corresponding 'checked' event.
        /// </summary>
        private void HandleSizeTypeChanged(object sender, EventArgs e)
        {
            if (!(sender as RadioButton).Checked)
                return;

            numProportion.Enabled = rbProportion.Checked;
            numAbsolute.Enabled = rbAbsolute.Checked;

            GridVector v = SelectedVector;
            if (v == null)
                return;

            if (rbProportion.Checked)
            {
                v.SizeType = VectorSizeType.Proportional;
                v.Value = (int) numProportion.Value;
            }
            else
            {
                v.SizeType = VectorSizeType.Absolute;
                v.Value = (int) numAbsolute.Value;
            }
            ListViewItem item = SelectedItem;
            item.SubItems[1].Text = v.SizeTypeLabel;
            item.SubItems[2].Text = v.ValueString;
        }

        /// <summary>
        /// Handles the size value being changed - ie. a spinner control value being
        /// changed.
        /// </summary>
        private void HandleSizeValueChanged(object sender, EventArgs e)
        {
            GridVector v = SelectedVector;
            if (v == null)
                return;
            NumericUpDown updn = sender as NumericUpDown;
            v.Value = (int) updn.Value;
            SelectedItem.SubItems[2].Text = v.ValueString;
        }

        /// <summary>
        /// Handles the vector type to display being changed
        /// </summary>
        private void HandleVectorTypeChanged(object sender, EventArgs e)
        {
            // No schema? Then it makes no odds
            if (_schema == null)
                return;
            // Otherwise, see what we've changed to and set the displayed
            // vectors appropriately
            switch (cmbVectorType.SelectedIndex)
            {
                case 0: DisplayedVectors = (ICollection) _schema.Columns; break;
                case 1: DisplayedVectors = (ICollection) _schema.Rows; break;
                default: DisplayedVectors = null; break;
            }
        }

        #endregion
    }
}
