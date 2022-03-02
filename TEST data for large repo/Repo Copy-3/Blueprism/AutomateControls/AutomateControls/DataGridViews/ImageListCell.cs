using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// DataGridView image cell which uses an imagelist for its images and can
    /// be set using an image key, index or an independent image.
    /// </summary>
    public class ImageListCell : DataGridViewImageCell
    {
        /// <summary>
        /// Creates a new blank image list cell.
        /// </summary>
        public ImageListCell()
        {
            this.ImageLayout = DataGridViewImageCellLayout.Normal;
            this.Style.SelectionBackColor = Color.FloralWhite;
        }

        /// <summary>
        /// Gets the formatted value for this image list cell.
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <param name="rowIndex">The index of the row to format</param>
        /// <param name="cellStyle">The cell style for the cell in question</param>
        /// <param name="valueTypeConverter">I don't know what this is.</param>
        /// <param name="formattedValueTypeConverter">Nope. Not a clue.</param>
        /// <param name="context">Did you see Corrie last night?</param>
        /// <returns>The formatted value</returns>
        protected override object GetFormattedValue(
            object value, int rowIndex, ref DataGridViewCellStyle cellStyle,
            TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context)
        {
            if (this.DataGridView == null)
                return null;

            ImageListColumn col = OwningColumn as ImageListColumn;
            if (col == null)
                return null;

            return col.GetImage(value);
        }
    }
}
