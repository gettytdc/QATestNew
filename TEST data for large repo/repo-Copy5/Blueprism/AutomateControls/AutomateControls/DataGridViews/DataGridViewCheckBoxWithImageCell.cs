using System.Drawing;
using System.Windows.Forms;


namespace AutomateControls.DataGridViews
{
    public class DataGridViewCheckBoxWithImageCell : DataGridViewCheckBoxCell
    {
        private readonly Bitmap _cellImage; 
        public DataGridViewCheckBoxWithImageCell(Bitmap image)
        {
            _cellImage = image;
        }

        protected override void Paint
        (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
                int rowIndex, DataGridViewElementStates elementState, object value,
                object formattedValue, string errorText, DataGridViewCellStyle cellStyle,
                DataGridViewAdvancedBorderStyle advancedBorderStyle,
                    DataGridViewPaintParts paintParts)
        {
            SolidBrush cellBackground = new SolidBrush(cellStyle.BackColor);
            graphics.FillRectangle(cellBackground, cellBounds);
            cellBackground.Dispose();

            Rectangle checkBoxArea = cellBounds;
            Rectangle buttonAdjustment = this.BorderWidths(advancedBorderStyle);
            checkBoxArea.X += buttonAdjustment.X;
            checkBoxArea.Y += buttonAdjustment.Y;
            
            checkBoxArea.Height -= buttonAdjustment.Height;
            checkBoxArea.Width -= buttonAdjustment.Width;
            Point drawInPoint = new Point((cellBounds.X + cellBounds.Width/ 2 - 6) - 2,
                cellBounds.Y + cellBounds.Height / 2 - 7);

            graphics.DrawImage(_cellImage, drawInPoint);
            
            PaintBorder(graphics, clipBounds, cellBounds,
            cellStyle, advancedBorderStyle);
                                 
        }
    }
}





