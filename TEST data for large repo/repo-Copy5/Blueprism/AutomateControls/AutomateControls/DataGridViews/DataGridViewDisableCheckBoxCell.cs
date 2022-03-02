using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace AutomateControls.DataGridViews
{
    public class DataGridViewDisableCheckBoxCell : DataGridViewCheckBoxCell
    {
        /// <summary>
        /// Value for whether the disabled checkbox it ticked or not.
        /// </summary>
        private bool _enabledValue;

        /// <summary>
        /// This property decides whether the checkbox should be shown 
        /// checked or unchecked.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabledValue;
            }
            set
            {
                _enabledValue = value;
            }
        }

        /// Override the Clone method so that the Enabled property is copied.
        public override object Clone()
        {
            DataGridViewDisableCheckBoxCell cell =
                (DataGridViewDisableCheckBoxCell)base.Clone();
            cell.Enabled = this.Enabled;
            return cell;
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
            PaintBorder(graphics, clipBounds, cellBounds,
            cellStyle, advancedBorderStyle);
            Rectangle checkBoxArea = cellBounds;
            Rectangle buttonAdjustment = this.BorderWidths(advancedBorderStyle);
            checkBoxArea.X += buttonAdjustment.X;
            checkBoxArea.Y += buttonAdjustment.Y;

            checkBoxArea.Height -= buttonAdjustment.Height;
            checkBoxArea.Width -= buttonAdjustment.Width;
            Point drawInPoint = new Point((cellBounds.X + cellBounds.Width / 2 - 6) - 1,
                cellBounds.Y + cellBounds.Height / 2 - 7);

            if (this._enabledValue)
                CheckBoxRenderer.DrawCheckBox(graphics, drawInPoint, CheckBoxState.CheckedDisabled);
            else
                CheckBoxRenderer.DrawCheckBox(graphics, drawInPoint, CheckBoxState.UncheckedDisabled);
        }
    }
}
