using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

using BluePrism.BPCoreLib;

using AutomateControls.Properties;
using BluePrism.Server.Domain.Models;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// Cell for a DataGridView which displays an icon corresponding to an <see cref="ItemStatus"/>
    /// value. This can be used in a DataGridView inside the designer by choosing a column type of
    /// <see cref="ItemStatusColumn"/>.
    /// </summary>
    /// <remarks>
    /// The idea and basis for this class was brought about by an article on InformIT : 
    /// http://www.informit.com/articles/article.aspx?p=446453&seqNum=14
    /// </remarks>
    public class ItemStatusCell: DataGridViewImageCell
    {

        #region - Constructors -

        /// <summary>
        /// Creates a new blank ItemStatusCell.
        /// </summary>
        public ItemStatusCell()
        {
            this.ImageLayout = DataGridViewImageCellLayout.Normal;
            this.Style.SelectionBackColor = Color.FloralWhite;
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Method to get the image which corresponds to the given status.
        /// </summary>
        /// <param name="stat">The status for which the corresponding display image is required.
        /// </param>
        /// <param name="col">The item status column which contains this cell, or null if this
        /// cell is not currently contained on an ItemStatusColumn.</param>
        /// <returns>An Image object which can be used to represent the given item status.
        /// </returns>
        protected virtual Image GetImageFor(ItemStatus stat, ItemStatusColumn col)
        {
            switch (stat)
            {
                case ItemStatus.Completed:
                    return Resources.tick_12x12;

                case ItemStatus.Stopped:
                case ItemStatus.Failed:
                    if (col != null && col.UsePersonIconForException)
                        return Resources.person_12x12;
                    return Resources.cross_16x16;

                case ItemStatus.Pending:
                case ItemStatus.Deferred:
                    return Resources.ellipsis_12x12;

                case ItemStatus.Running:
                case ItemStatus.Debugging:
                    return Resources.next_16x16;

                case ItemStatus.Locked:
                    return Resources.padlock_16x16;

                case ItemStatus.Queried:
                    return Resources.help_16x16;
                case ItemStatus.PartExceptioned:
                    return Resources.Flag_Purple_16x16;
                // can't decide if Unknown should show something like a question mark...
                // settling for 'no' for right now.
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the formatted value for the cell in the owning column for
        /// the specified row.
        /// </summary>
        /// <param name="value">The value to be formatted. </param>
        /// <param name="rowIndex">The index of the cell's parent row. </param>
        /// <param name="cellStyle">The <see cref="DataGridViewCellStyle"/> in effect for the cell.
        /// </param>
        /// <param name="valueTypeConverter">A <see cref="TypeConverter"/> associated with the 
        /// value type that provides custom conversion to the formatted value type, or null if no 
        /// such custom conversion is needed.</param>
        /// <param name="formattedValueTypeConverter">A <see cref="TypeConverter"/> associated with
        /// the formatted value type that provides custom conversion to the formatted value type,
        /// or null if no such custom conversion is needed.</param>
        /// <param name="context">A bitwise combination of 
        /// <see cref="DataGridViewDataErrorContexts"/>  values describing the context in which the
        /// formatted value is needed.</param>
        /// <returns>The formatted value of the cell or null if the cell does not belong to a
        /// DataGridView control</returns>
        protected override object GetFormattedValue(
            object value, int rowIndex, ref DataGridViewCellStyle cellStyle,
            TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context)
        {
            if (this.DataGridView == null)
                return null;

            ItemStatus status = ItemStatus.Unknown;
            ItemStatusColumn col = OwningColumn as ItemStatusColumn;
            if (col != null)
                status = col.DefaultItem;

            if (value is ItemStatus || value is int)
                status = (ItemStatus)value;
            cellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            return GetImageFor(status, col);
        }

        #endregion

    }
}
