using System.Windows.Forms;
using System.ComponentModel;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// A DataGridView column designed for displaying dates
    /// </summary>
    public class DateColumn : DataGridViewColumn
    {
        #region - Member Variables -

        // The format of the date to display
        private string _format;

        // Whether boundary dates should be shown or not
        private bool _showBoundaryDates;

        #endregion

        #region - Constructors-

        /// <summary>
        /// Creates a new DateColumn
        /// </summary>
        public DateColumn() : base(new DateCell())
        {
            SortMode = DataGridViewColumnSortMode.Automatic;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The sort mode of this date column. Recreated in this class, purely so
        /// that we can also handle the serialization of the value differently to
        /// how the base class does so (ie. making it default to automatic sorting)
        /// </summary>
        [Category("Behavior"), DefaultValue(DataGridViewColumnSortMode.Automatic),
         Description("The sort mode for the column")]
        public new DataGridViewColumnSortMode SortMode
        {
            get { return base.SortMode; }
            set { base.SortMode = value; }
        }

        /// <summary>
        /// Gets or sets whether the boundary dates should be displayed in the grid
        /// or not. Default is not to show them, but to show blanks instead.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description(
         "Show MinValue or MaxValue dates in cells?")]
        public bool ShowBoundaryDates
        {
            get { return _showBoundaryDates; }
            set { _showBoundaryDates = value; }
        }

        /// <summary>
        /// The format of the date to set in this column.
        /// </summary>
        [Category("Data")]
        public string DateFormat
        {
            get { return _format; }
            set { _format = value; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Clones this column, ensuring that the configured date format is
        /// propogated into the cloned object.
        /// This is necessary due to weirdness in how DataGridViewColumn.Clone()
        /// works and how the forms designer treats columns.
        /// </summary>
        /// <returns>A disconnected clone of this column.</returns>
        public override object Clone()
        {
            DateColumn copy = base.Clone() as DateColumn;
            copy.DateFormat = this.DateFormat;
            copy.ShowBoundaryDates = this.ShowBoundaryDates;
            return copy;
        }

        /// <summary>
        /// Resets the sort mode value to the default.
        /// </summary>
        private void ResetSortMode()
        {
            SortMode = DataGridViewColumnSortMode.Automatic;
        }

        /// <summary>
        /// Checks if the sort mode for this
        /// </summary>
        /// <returns></returns>
        private bool ShouldSerializeSortMode()
        {
            return (SortMode != DataGridViewColumnSortMode.Automatic);
        }

        #endregion

    }

}
