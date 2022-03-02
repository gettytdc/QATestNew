using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AutomateControls.DataGridViews
{
    public class DataGridViewNumericUpDownColumn : DataGridViewColumn
    {
        #region - Class-scope declarations -

        /// <summary>
        /// The default decimal places value in the up/down data grid view components
        /// </summary>
        internal const int DefaultDecimalPlaces = 0;

        /// <summary>
        /// The default maximum value in the up/down data grid view components.
        /// </summary>
        internal const decimal DefaultMaximum = 100m;

        /// <summary>
        /// The default minimum value in the up/down data grid view components.
        /// </summary>
        internal const decimal DefaultMinimum = 0m;

        /// <summary>
        /// The default increment value in the up/down data grid view components.
        /// </summary>
        internal const decimal DefaultIncrement = 1m;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty numeric up/down column
        /// </summary>
        public DataGridViewNumericUpDownColumn()
            : base(new DataGridViewNumericUpDownCell()) { }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets or sets the cell template for this column.
        /// </summary>
        /// <exception cref="CellTemplateException">If setting with a non-null cell
        /// which is not a <see cref="DataGridViewNumericUpDownCell"/> or subclass.
        /// </exception>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DataGridViewCell CellTemplate
        {
            get { return base.CellTemplate; }
            set
            {
                var cell = value as DataGridViewNumericUpDownCell;
                if (value != null && cell == null) throw new CellTemplateException(
                    typeof(DataGridViewNumericUpDownCell), value.GetType());
                base.CellTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of decimal places to use for up/down controls in
        /// this column.
        /// </summary>
        /// <exception cref="MissingTemplateException">If setting the value and the
        /// cell template is not set in this column.</exception>
        [Category("Behavior"), DefaultValue(DefaultDecimalPlaces), Description(
            "The number of decimal places to accept in cells in this column")]
        public int DecimalPlaces
        {
            get { return GetNumericUpDownCellTemplate().DecimalPlaces; }
            set
            {
                GetNumericUpDownCellTemplate().DecimalPlaces = value;
                UpdateCells((rowInd, cell) => cell.SetDecimalPlaces(rowInd, value));
            }
        }

        /// <summary>
        /// Gets or sets the minimum value to use for up/down controls in this column
        /// </summary>
        /// <exception cref="MissingTemplateException">If setting the value and the
        /// cell template is not set in this column.</exception>
        [Category("Data"), RefreshProperties(RefreshProperties.All), // <- may alter the maximum
         Description("The minimum value allowed in this column")]
        public decimal Minimum
        {
            get { return GetNumericUpDownCellTemplate().Minimum; }
            set
            {
                GetNumericUpDownCellTemplate().Minimum = value;
                UpdateCells((rowInd, cell) => cell.SetMinimum(rowInd, value));
            }
        }

        /// <summary>
        /// Gets or sets the maximum value to use for up/down controls in this column
        /// </summary>
        /// <exception cref="MissingTemplateException">If setting the value and the
        /// cell template is not set in this column.</exception>
        [Category("Data"), RefreshProperties(RefreshProperties.All), // <- may alter the minimum
         Description("The maximum value allowed in this column")]
        public decimal Maximum
        {
            get { return GetNumericUpDownCellTemplate().Maximum; }
            set
            {
                GetNumericUpDownCellTemplate().Maximum = value;
                UpdateCells((rowInd, cell) => cell.SetMaximum(rowInd, value));
            }
        }

        /// <summary>
        /// Gets or sets the increment value to use for up/down controls in this
        /// column
        /// </summary>
        /// <exception cref="MissingTemplateException">If setting the value and the
        /// cell template is not set in this column.</exception>
        [Category("Data"), Description(
            "The amount to increment or decrement by for controls in this column")]
        public decimal Increment
        {
            get { return GetNumericUpDownCellTemplate().Increment; }
            set
            {
                GetNumericUpDownCellTemplate().Increment = value;
                UpdateCells((rowInd, cell) => cell.SetIncrement(rowInd, value));
            }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Updates the numeric up/down cells in shared rows in this column using the
        /// given delegate
        /// </summary>
        /// <param name="action">The action to perform on numeric up/down cells at a
        /// specified row index in this column. The action is only called for cells
        /// which are of type: <see cref="DataGridViewNumericUpDownCell"/> found in
        /// the owning data grid view in this column. The parameters are: <list>
        /// <item>The row index as an int, and </item>
        /// <item>The cell instance being processed.</item>
        /// </list>
        /// </param>
        private void UpdateCells(Action<int, DataGridViewNumericUpDownCell> action)
        {
            var view = this.DataGridView;
            if (view == null) return;
            var rows = view.Rows;
            int count = rows.Count;
            for (int i = 0; i < count; i++)
            {
                // Use shared rows where possible for scalability
                DataGridViewRow row = rows.SharedRow(i);
                DataGridViewNumericUpDownCell cell =
                    row.Cells[Index] as DataGridViewNumericUpDownCell;
                // Call the internal method, not the property, so that each cell
                // is not invalidated separately.
                if (cell != null)
                    action(i, cell);
            }
            view.InvalidateColumn(Index);
        }

        /// <summary>
        /// Gets the cell template set in this column as a
        /// <see cref="DataGridViewNumericUpDownCell"/>.
        /// </summary>
        /// <returns>The <see cref="CellTemplate"/> set in this column, cast into a
        /// <see cref="DataGridViewNumericUpDownCell"/></returns>
        /// <exception cref="MissingTemplateException">If the cell template is not
        /// set in this column.</exception>
        private DataGridViewNumericUpDownCell GetNumericUpDownCellTemplate()
        {
            DataGridViewCell template = CellTemplate;
            if (template == null) throw new MissingTemplateException();
            return (DataGridViewNumericUpDownCell)template;
        }

        /// <summary>
        /// Returns a standard compact string representation of the column.
        /// </summary>
        public override string ToString()
        {
            return string.Format(
                "DataGridViewNumericUpDownColumn {{ Name={0}; Index={1} }}",
                Name, Index);
        }

        #endregion

        #region - ShouldSerialize / Reset Methods -

        // We use ShouldSerialize and Reset methods for the decimal properties in
        // this cell, because they cannot be used as attribute value - they're not
        // real CLR primitive types - ie. the CLR isn't aware of them as a type in
        // the same way it is for ints, doubles, strings etc.

        /// <summary>
        /// Checks if the Minimum value in this cell should be serialized by the
        /// visual designer.
        /// </summary>
        /// <returns>True if the <see cref="Minimum"/> value set in this column is
        /// not the current default value.</returns>
        private bool ShouldSerializeMinimum()
        {
            return (Minimum != DataGridViewNumericUpDownColumn.DefaultMinimum);
        }

        /// <summary>
        /// Resets the <see cref="Minimum"/> value to its default value.
        /// </summary>
        private void ResetMinimum()
        {
            Minimum = DataGridViewNumericUpDownColumn.DefaultMinimum;
        }

        /// <summary>
        /// Checks if the Maximum value in this cell should be serialized by the
        /// visual designer.
        /// </summary>
        /// <returns>True if the <see cref="Maximum"/> value set in this column is
        /// not the current default value.</returns>
        private bool ShouldSerializeMaximum()
        {
            return (Maximum != DataGridViewNumericUpDownColumn.DefaultMaximum);
        }

        /// <summary>
        /// Resets the <see cref="Maximum"/> value to its default value.
        /// </summary>
        private void ResetMaximum()
        {
            Maximum = DataGridViewNumericUpDownColumn.DefaultMaximum;
        }

        /// <summary>
        /// Checks if the Increment value in this cell should be serialized by the
        /// visual designer.
        /// </summary>
        /// <returns>True if the <see cref="Increment"/> value set in this column is
        /// not the current default value.</returns>
        private bool ShouldSerializeIncrement()
        {
            return (Increment != DataGridViewNumericUpDownColumn.DefaultIncrement);
        }

        /// <summary>
        /// Resets the <see cref="Increment"/> value to its default value.
        /// </summary>
        private void ResetIncrement()
        {
            Increment = DataGridViewNumericUpDownColumn.DefaultIncrement;
        }

        #endregion

    }
}
