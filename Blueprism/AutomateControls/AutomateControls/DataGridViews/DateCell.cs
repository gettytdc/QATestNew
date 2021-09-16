using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// A DataGridView cell which can hold dates
    /// </summary>
    public class DateCell : DataGridViewTextBoxCell
    {
        /// <summary>
        /// Gets the default new value for a date cell in a new row
        /// </summary>
        public override object DefaultNewRowValue
        {
            get { return DateTime.MinValue; }
        }

        /// <summary>
        /// Gets the formatted version of the specified value.
        /// </summary>
        /// <param name="value">The value to be formatted</param>
        /// <param name="rowIndex">The index of the row that this value will be
        /// displayed.</param>
        /// <param name="cellStyle">The style applied to this cell</param>
        /// <param name="typeConv">A type converter</param>
        /// <param name="formattedTypeConv">A formatted type converter</param>
        /// <param name="context">Some context</param>
        /// <returns>A formatted value for the given value.</returns>
        protected override object GetFormattedValue(object value, int rowIndex,
            ref DataGridViewCellStyle cellStyle, TypeConverter typeConv,
            TypeConverter formattedTypeConv, DataGridViewDataErrorContexts context)
        {
            // If this is a DateTime value, format according to the format set in
            // the owning DateColumn, or use the default if there is no owning
            // column or the owning column isn't a DateColumn or there is no format
            // set in the owning DateColumn.
            if (value is DateTime)
            {
                DateTime dt = (DateTime)value;
                DateColumn col = OwningColumn as DateColumn;

                // If this is a boundary date and we're not set to show them, just
                // return an empty string.
                if ((dt == DateTime.MinValue || dt == DateTime.MaxValue)
                    && (col == null || !col.ShowBoundaryDates))
                {
                    return "";
                }

                // Otherwise format the value according to the dateformat configured
                // in the column
                string format = (col != null ? col.DateFormat : null);
                return (format == null ? dt.ToString() : dt.ToString(format));
            }

            return base.GetFormattedValue(value, rowIndex,
                ref cellStyle, typeConv, formattedTypeConv, context);
        }

        public override object ParseFormattedValue(object fmtVal,
            DataGridViewCellStyle cellStyle, TypeConverter fmtValConverter,
            TypeConverter valConverter)
        {
            return base.ParseFormattedValue(fmtVal, cellStyle, fmtValConverter, valConverter);
        }

        /// <summary>
        ///  A <see cref="Type"/> representing the data type of the value in the cell
        /// ie. the type of <see cref="DateTime"/>
        /// </summary>
        public override Type ValueType
        {
            get { return typeof(DateTime); }
        }
    }

}
