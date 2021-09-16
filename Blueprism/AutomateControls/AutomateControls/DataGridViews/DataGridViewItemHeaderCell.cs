using AutomateControls.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using BluePrism.Core.Data;

namespace AutomateControls.DataGridViews
{
    public class DataGridViewItemHeaderCell : DataGridViewTextBoxCell
    {
        #region - Class-scope Declarations -

        /// <summary>
        /// The extra vertical padding added above the title text (in addition to the
        /// padding set by the user in the cell style.
        /// </summary>
        const int PaddingAboveText = 1;

        /// <summary>
        /// The extra vertical padding added below the subtitle text (in addition to
        /// the padding set by the user in the cell style.
        /// </summary>
        const int PaddingBelowText = 1;

        /// <summary>
        /// The amount of vertical padding added between the title and subtitle text.
        /// </summary>
        const int PaddingBetweenTitles = 4;

        /// <summary>
        /// The amount of horizontal padding added between the image and text block
        /// </summary>
        const int PaddingBetweenImageAndText = 12;

        /// <summary>
        /// The size of the cell including width and height.
        /// </summary>
        private Size _cellContentSize = Size.Empty;

        #endregion

        #region - Properties -

        /// <summary>
        /// Override of the EditType of this cell. It's readonly, so inhibit any
        /// editing control.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Type EditType
        {
            get { return null; }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Type ValueType
        {
            get { return (base.ValueType ?? typeof(IItemHeader)); }
        }

        /// <summary>
        /// Gets the default new value for a date cell in a new row
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override object DefaultNewRowValue
        {
            get { return new BasicItemHeader(); }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected DataGridViewItemHeaderColumn OwningItemHeaderColumn
        {
            get { return OwningColumn as DataGridViewItemHeaderColumn; }
        }

        /// <summary>
        /// Just a nicer way of getting the DataGridView owner of this cell than
        /// 'DataGridView', which must be prefixed by 'this.' every time to indicate
        /// that you're not trying to access a static member on the class.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected DataGridView Owner
        {
            get { return base.DataGridView; }
        }

        /// <summary>
        /// The border widths in place on this cell, taking into account the
        /// advanced cell border style set in the owning data grid view
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected Rectangle StdBorderWidths
        {
            get
            {
                if (Owner == null)
                    return Rectangle.Empty;
                var borderStyle = AdjustCellBorderStyle(
                    Owner.AdvancedCellBorderStyle,
                    new DataGridViewAdvancedBorderStyle(),
                    false, false, false, false);
                return this.BorderWidths(borderStyle);
            }
        }

        /// <summary>
        /// Gets whether this cell is the currently focused cell in its owning
        /// data grid view.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFocusedCell
        {
            get
            {
                if (Owner == null)
                    return false;
                Point addr = Owner.CurrentCellAddress;
                return (addr.X == ColumnIndex && addr.Y == RowIndex);
            }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Paints this cell
        /// </summary>
        /// <param name="g">The graphics object with which to paint</param>
        /// <param name="clipBounds">The bounds of the clip region for this paint
        /// operation - typically, the clip of the owning view, not of this cell.
        /// </param>
        /// <param name="cellBounds">The bounds of this cell within its owning
        /// data grid view</param>
        /// <param name="rowIndex">The index of the row that this cell is on.</param>
        /// <param name="cellState">The state of the cell</param>
        /// <param name="value">The value associated with this cell</param>
        /// <param name="formattedVal">The formatted value associated with this cell
        /// </param>
        /// <param name="errorText">Any error text associated with this cell</param>
        /// <param name="cellStyle">The style associated with this cell</param>
        /// <param name="advancedBorderStyle">The border style associated with this
        /// cell</param>
        /// <param name="paintParts">The parts that this paint operation should
        /// paint</param>
        protected override void Paint(
            Graphics g,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates cellState,
            object value,
            object formattedVal,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {

            // Borders and background...
            if (paintParts.HasFlag(DataGridViewPaintParts.Background)
                || paintParts.HasFlag(DataGridViewPaintParts.Border))
            {
                base.Paint(g, clipBounds, cellBounds, rowIndex, cellState, value,
                    null, errorText, cellStyle, advancedBorderStyle,
                    DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
            }

            // Set the clip region for the graphics object to the bounds of the cell
            Region origClip = g.Clip;
            g.Clip = new Region(cellBounds);

            // Check if this cell is selected or not (changes bg/fg colours)
            bool selected = cellState.HasFlag(DataGridViewElementStates.Selected);

            // Get the bounds of the value, taking into account the border...
            Rectangle borderWidths = BorderWidths(advancedBorderStyle);
            Rectangle valBounds = new Rectangle(
                cellBounds.X + borderWidths.X,
                cellBounds.Y + borderWidths.Y,
                cellBounds.Width - borderWidths.Right,
                cellBounds.Height - borderWidths.Bottom);

            // ... and the padding.
            Padding padding = cellStyle.Padding;
            if (padding != Padding.Empty)
            {
                if (Owner.RightToLeft == RightToLeft.Yes)
                    valBounds.Offset(padding.Right, padding.Top);
                else
                    valBounds.Offset(padding.Left, padding.Top);
                valBounds.Width -= padding.Horizontal;
                valBounds.Height -= padding.Vertical;
            }

            GDICache cache = OwningItemHeaderColumn.Cache;

            if (paintParts.HasFlag(DataGridViewPaintParts.Background)
                && valBounds.Width > 0 && valBounds.Height > 0)
            {
                Brush b = cache.GetBrush(
                    selected && paintParts.HasFlag(DataGridViewPaintParts.SelectionBackground)
                    ? cellStyle.SelectionBackColor : cellStyle.BackColor);
                {
                    g.FillRectangle(b, valBounds);
                }
            }

            // Paint the focus rectangle if we currently have focus
            if (paintParts.HasFlag(DataGridViewPaintParts.Focus) && IsFocusedCell)
            {
                ControlPaint.DrawFocusRectangle(
                    g, valBounds, Color.Empty, cellStyle.SelectionBackColor);
            }
            // Make space for the focus rectangle (even if it's not there - we don't
            // want the content to move when the cell gains/loses focus
            valBounds.Inflate(-1, -1);

            if (paintParts.HasFlag(DataGridViewPaintParts.ContentForeground))
            {
                // So now we paint
                // We'll need the column from time to time
                var col = OwningItemHeaderColumn;

                // And the data (if it's a supported type)
                var info = value as IItemHeader;
                if (info == null)
                    return;

                Image img = (col == null ? null : col.GetImage(info.ImageKey));
                if (img != null)
                {
                    Rectangle imgBounds = valBounds;
                    imgBounds.Size = img.Size;
                    g.DrawImageUnscaledAndClipped(img, imgBounds);
                }

                Rectangle txtBounds = valBounds;
                if (img != null)
                {
                    int imgWidth = img.Width + PaddingBetweenImageAndText;
                    txtBounds.X += imgWidth;
                    txtBounds.Width -= imgWidth;
                }
                txtBounds.Y += PaddingAboveText;

                // If we're outside the clip at this point, nothing else to do
                if (!clipBounds.IntersectsWith(txtBounds))
                    return;

                Size titleSize;
                TextFormatFlags flags =
                    TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.WordEllipsis;

                if (txtBounds.Height > 0 && txtBounds.Width > 0)
                {
                    Font bold = cache.GetFont(cellStyle.Font, FontStyle.Bold);
                    {
                        TextRenderer.DrawText(
                            g, info.Title, bold, txtBounds,
                            selected ? cellStyle.SelectionForeColor : cellStyle.ForeColor,
                            flags);
                        titleSize = MeasureTextPreferredSize(
                            g, info.Title, bold, 5f, TextFormatFlags.Default);
                    }
                    txtBounds.Y += titleSize.Height + PaddingBetweenTitles;
                    txtBounds.Height -= titleSize.Height + PaddingBetweenTitles;
                    if (txtBounds.Height > 0 && txtBounds.Width > 0)
                    {
                        TextRenderer.DrawText(
                            g, info.SubTitle, cellStyle.Font, txtBounds,
                            selected ? cellStyle.SelectionForeColor : cellStyle.ForeColor,
                            flags);
                    }
                }
            }
            // Return the clip region to what it was before this method was entered
            g.Clip = origClip;
        }

        /// <summary>
        /// Gets the preferred size for this cell
        /// </summary>
        /// <param name="itemGraphic">The graphics object with which to determine the preferred
        /// size, especially with which to measure strings</param>
        /// <param name="cellStyle">The style associated with the cell</param>
        /// <param name="rowIndex">The row index within the owning data grid view on
        /// which this cell occurs</param>
        /// <param name="constraintSize">The constrained size of this cell</param>
        /// <returns>The preferred size of this cell</returns>
        protected override Size GetPreferredSize(
            Graphics itemGraphic,
            DataGridViewCellStyle cellStyle,
            int rowIndex,
            Size constraintSize)
        {
            // No owner? No size.
            if (Owner == null)
                return new Size(-1, -1);

            if (_cellContentSize.IsEmpty)
            {
                // If not there, use a placeholder
                var info = GetValue(rowIndex) as IItemHeader
                           ?? new BasicItemHeader();

                DataGridViewItemHeaderColumn col = OwningItemHeaderColumn;

                _cellContentSize = GetContentSize(itemGraphic, cellStyle, info);
            }

            return _cellContentSize;
        }

        /// <summary>
        /// Arranges the size of the cell which is determined by the cells content, graphic, 
        /// cell style and textual information.
        /// </summary>
        /// <param name="itemGraphic">The graphics object with which to determine the preferred
        /// size, especially with which to measure strings. </param>
        /// <param name="cellStyle">The style associated with the cell. </param>
        /// <param name="headerInfo">Item textual information. </param>
        /// <returns>A Size struct positioned given the cells content.</returns>
        private Size GetContentSize(
            Graphics itemGraphic,
            DataGridViewCellStyle cellStyle,
            IItemHeader headerInfo)
        {
            DataGridViewItemHeaderColumn col = OwningItemHeaderColumn;

            Size result = Size.Empty;

            // First add the image size into the content
            Image img = col?.GetImage(headerInfo.ImageKey);

            result.Width += img?.Width + PaddingBetweenImageAndText ?? 0;
            result.Height += img?.Height ?? 0;

            // Then calculate the text size (compound of title & subtitle)
            GDICache cache = OwningItemHeaderColumn.Cache;
            Font bold = cache.GetFont(cellStyle.Font, FontStyle.Bold);

            Size titleSize = MeasureTextPreferredSize(
                itemGraphic, headerInfo.Title, bold, 5f, TextFormatFlags.Default);

            Size subtitleSize = MeasureTextPreferredSize(
                itemGraphic, headerInfo.SubTitle, cellStyle.Font, 5f, TextFormatFlags.Default);

            Size textSize = new Size(
                Math.Max(titleSize.Width, subtitleSize.Width),
                titleSize.Height + subtitleSize.Height +
                PaddingAboveText + PaddingBelowText + PaddingBetweenTitles);

            var borderPadding = GetBorderPadding(cellStyle);

            // Add the text size into the content size
            result.Width += textSize.Width;
            result.Height = Math.Max(result.Height + borderPadding.Horizontal, 
                                     textSize.Height + borderPadding.Vertical);

            return result;
        }

        /// <summary>
        /// Gets the extra border and padding for the cell given the cell style and
        /// the standard border widths.
        /// </summary>
        /// <param name="cellStyle">The style associated with the cell. </param>
        /// <returns>A value tuple containing the horizontal and vertical values. </returns>
        private (int Horizontal, int Vertical) GetBorderPadding(DataGridViewCellStyle cellStyle)
        {
            Padding padding = cellStyle.Padding;

            // Add the border widths and the padding; also add 2 in each direction
            // for the focus rectangle
            Rectangle borderWidths = StdBorderWidths;
            int horizontal = borderWidths.Right + padding.Horizontal + 2;
            int vertical = borderWidths.Bottom + padding.Vertical + 2;

            return (horizontal, vertical);
        }

        /// <summary>
        /// Sets the value in this cell
        /// </summary>
        /// <param name="rowIndex">The index of the row at which this cell resides.
        /// </param>
        /// <param name="value">The value being set in this cell</param>
        /// <returns>true if the value was set; false otherwise.</returns>
        protected override bool SetValue(int rowIndex, object value)
        {
            if (!base.SetValue(rowIndex, value))
                return false;
            IItemHeader info = (
                (value as IItemHeader) ?? BasicItemHeader.Empty);
            ToolTipText = info.Title +
                (info.SubTitle.Length > 0 ? String.Format(Resources.DataGridViewItemHeaderCell_SubTitle0, info.SubTitle) : "");

            return true;
        }

        /// <summary>
        /// Gets a string representation of this cell
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "DataGridViewItemInfoCell {{ ColumnIndex={0}, RowIndex={1} }}",
                ColumnIndex, RowIndex);
        }

        #endregion
    }
}
