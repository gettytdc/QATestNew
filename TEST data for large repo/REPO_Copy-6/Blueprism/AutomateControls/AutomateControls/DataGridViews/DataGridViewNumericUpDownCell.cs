using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BluePrism.Server.Domain.Models;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// Cell used within a DataGridView which represents a decimal number which can
    /// be modified using up and down buttons next to the text box.
    /// </summary>
    public class DataGridViewNumericUpDownCell : DataGridViewTextBoxCell
    {
        #region - Class-scope Declarations -

        // The default width of the rendering bitmap
        private const int DefaultRenderWidth = 100;

        // The default height of the rendering bitmap
        private const int DefaultRenderHeight = 22;

        /// <summary>
        /// Translates a character to the corresponding virtual-key code and shift
        /// state for the current keyboard.
        /// </summary>
        /// <param name="key">The character to be translated into a virtual-key code.
        /// </param>
        /// <returns>The virtual-key code corresponding to the given character or -1
        /// if the character could not be found.</returns>
        /// <remarks>Note that this specifies <see cref="CharSet.Unicode"/> to ensure
        /// that we can cope with all unicode characters as well as ANSI ones.
        /// </remarks>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern short VkKeyScan(char key);

        #endregion

        #region - Thread-static Variables -

        // The bitmap onto which the control is rendered for non-editing cells
        [ThreadStatic]
        private static Bitmap _render;

        // The proxy control used to render the control for non-editing cells
        [ThreadStatic]
        private static NumericUpDown _proxyCtl;

        #endregion

        #region - Member Variables -

        // The number of decimal places to support in this cell
        private int _decimalPlaces;

        // The minimum value allowed in this cell
        private decimal _min;

        // The maximum value allowed in this cell
        private decimal _max;

        // The incremental value to apply when up/down buttons are clicked
        private decimal _inc;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a an empty numeric up/down data grid view cell.
        /// </summary>
        public DataGridViewNumericUpDownCell()
        {
            _decimalPlaces = DataGridViewNumericUpDownColumn.DefaultDecimalPlaces;
            _min = DataGridViewNumericUpDownColumn.DefaultMinimum;
            _max = DataGridViewNumericUpDownColumn.DefaultMaximum;
            _inc = DataGridViewNumericUpDownColumn.DefaultIncrement;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets or sets the number of decimal places allowed in this cell.
        /// </summary>
        [DefaultValue(DataGridViewNumericUpDownColumn.DefaultDecimalPlaces)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Bug", "S4275:Getters and setters should access the expected fields", Justification = "_decimalPlaces is set in SetDecimalPlaces() method")]
        public int DecimalPlaces
        {
            get { return _decimalPlaces; }
            set
            {
                if (_decimalPlaces != value)
                {
                    SetDecimalPlaces(RowIndex, value);
                    OnConfigChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum value allows in this cell
        /// </summary>
        public decimal Minimum
        {
            get { return _min; }
            set
            {
                if (_min != value)
                {
                    SetMinimum(RowIndex, value);
                    OnConfigChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum value allows in this cell
        /// </summary>
        public decimal Maximum
        {
            get { return _max; }
            set
            {
                if (_max != value)
                {
                    SetMaximum(RowIndex, value);
                    OnConfigChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the increment value to use in this cell
        /// </summary>
        public decimal Increment
        {
            get { return _inc; }
            set
            {
                if (_inc != value)
                {
                    SetIncrement(RowIndex, value);
                    OnConfigChanged();
                }
            }
        }

        /// <summary>
        /// Gets the numeric up/down editing control currently in use in the owning
        /// data grid view, or null if there is no owning view or it has no editing
        /// control, or its editing control is not a numeric up/down control.
        /// </summary>
        private DataGridViewNumericUpDownEditingControl EditingControl
        {
            get
            {
                DataGridView view = DataGridView;
                if (view == null) return null;
                if (view.CurrentCellAddress.X != ColumnIndex) return null;
                return view.EditingControl as DataGridViewNumericUpDownEditingControl;
            }
        }

        /// <summary>
        /// Gets the type of the Value property in this cell.
        /// </summary>
        public override Type ValueType
        {
            get { return typeof(decimal); }
        }

        /// <summary>
        /// Gets the type of the editor for this cell
        /// </summary>
        public override Type EditType
        {
            get { return typeof(DataGridViewNumericUpDownEditingControl); }
        }

        #endregion

        #region - Public Methods -

        /// <summary>
        /// Clones this cell, ensuring that all the values are set on it from this
        /// cell instance.
        /// </summary>
        /// <returns>A clone of this cell with the same configuration values as this
        /// cell.</returns>
        public override object Clone()
        {
            var cell = (DataGridViewNumericUpDownCell)base.Clone();
            cell.DecimalPlaces = DecimalPlaces;
            cell.Minimum = Minimum;
            cell.Maximum = Maximum;
            cell.Increment = Increment;
            return cell;
        }

        /// <summary>
        /// Gets a string representation of this cell
        /// </summary>
        /// <returns>This cell type and its column/row indices.</returns>
        public override string ToString()
        {
            return string.Format(
                "{0} {{ ColumnIndex={1}; RowIndex={2} }}",
                GetType().Name, ColumnIndex, RowIndex);
        }

        /// <summary>
        /// Detaches the numeric up/down editing control operating on behalf of this
        /// cell from the data grid view.
        /// </summary>
        /// <exception cref="InvalidStateException">If the cell has no registered
        /// DataGridView owner, or its owner has no editing control set.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override void DetachEditingControl()
        {
            DataGridView view = this.DataGridView;
            if (view == null || view.EditingControl == null)
                throw new InvalidStateException(
                    "Cell is detached or its owner grid has no editing control.");

            var ctl = view.EditingControl as DataGridViewNumericUpDownEditingControl;
            if (ctl != null)
                ctl.Detach();

            base.DetachEditingControl();
        }

        /// <summary>
        /// Positions the editing control based on a cell's bounds and style
        /// </summary>
        /// <param name="setLocation">true to have the control placed as specified by
        /// the other arguments; false to allow the control to place itself.</param>
        /// <param name="setSize">true to specify the size; false to allow the
        /// control to size itself. </param>
        /// <param name="cellBounds">A <see cref="Rectangle "/> that defines the cell
        /// bounds. </param>
        /// <param name="cellClip">The area that will be used to paint the editing
        /// control.</param>
        /// <param name="cellStyle">A <see cref="DataGridViewCellStyle"/> that
        /// represents the style of the cell being edited.</param>
        /// <param name="singleVerticalBorderAdded">true to add a vertical border to
        /// the cell; otherwise, false.</param>
        /// <param name="singleHorizontalBorderAdded">true to add a horizontal border
        /// to the cell; otherwise, false.</param>
        /// <param name="isFirstDisplayedColumn">true if the hosting cell is in the
        /// first visible column; otherwise, false.</param>
        /// <param name="isFirstDisplayedRow">true if the hosting cell is in the
        /// first visible row; otherwise, false.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override void PositionEditingControl(
            bool setLocation,
            bool setSize,
            Rectangle cellBounds,
            Rectangle cellClip,
            DataGridViewCellStyle cellStyle,
            bool singleVerticalBorderAdded,
            bool singleHorizontalBorderAdded,
            bool isFirstDisplayedColumn,
            bool isFirstDisplayedRow)
        {
            Rectangle bounds = PositionEditingPanel(
                cellBounds, cellClip, cellStyle,
                singleVerticalBorderAdded, singleHorizontalBorderAdded,
                isFirstDisplayedColumn, isFirstDisplayedRow);

            this.DataGridView.EditingControl.Bounds =
                AdjustEditingControlBounds(bounds, cellStyle);
        }

        /// <summary>
        /// Adjust the bounds of the editing control to honour the alignment set in
        /// the active cell style.
        /// </summary>
        /// <param name="bounds">The bounds reserved for the editing control.</param>
        /// <param name="cellStyle">The style applied to the cell</param>
        /// <returns>A rectangle describing the bounds within which the edit control
        /// should fit, taking into account a small amount of padding and the cell
        /// style's configured alignment. </returns>
        private Rectangle AdjustEditingControlBounds(
            Rectangle bounds, DataGridViewCellStyle cellStyle)
        {
            // Add a 1 pixel padding on the left and right of the editing control
            bounds.X++;
            bounds.Width = Math.Max(0, bounds.Width - 2);
            // And top and bottom
            bounds.Y++;
            bounds.Height = Math.Max(0, bounds.Height - 2);

            // Adjust the vertical location of the editing control:
            int preferredHeight = cellStyle.Font.Height + 3;
            if (preferredHeight < bounds.Height)
            {
                switch (cellStyle.Alignment)
                {
                    // Top aligned - no further adjustment needed
                    case DataGridViewContentAlignment.TopLeft:
                    case DataGridViewContentAlignment.TopCenter:
                    case DataGridViewContentAlignment.TopRight:
                        break;

                    // Bottom aligned - push to bottom of bounds
                    case DataGridViewContentAlignment.BottomLeft:
                    case DataGridViewContentAlignment.BottomCenter:
                    case DataGridViewContentAlignment.BottomRight:
                        bounds.Y += bounds.Height - preferredHeight;
                        break;

                    // Middle aligned - add half available space to y-coord
                    default:
                        bounds.Y += (bounds.Height - preferredHeight) / 2;
                        break;
                }
            }

            return bounds;
        }

        /// <summary>
        /// Paints this cell.
        /// </summary>
        /// <param name="g">The graphics context to use to draw the cell.</param>
        /// <param name="clipBounds">The clipping region for the paint operation.
        /// </param>
        /// <param name="cellBounds">The bounds of the cell within the host.</param>
        /// <param name="rowInd">The index of the row whose cell is being painted
        /// (<see cref="RowIndex"/> may be -1 if this cell is within a shared row).
        /// </param>
        /// <param name="cellState">The state of the cell to draw.</param>
        /// <param name="val">The value of the cell.</param>
        /// <param name="fmtVal">The formatted value of the cell.</param>
        /// <param name="errorText">The error message, if there is one.</param>
        /// <param name="style">The style to apply to the cell.</param>
        /// <param name="borderStyle">The border style of the cell.</param>
        /// <param name="paintParts">A bitflag value indicating which parts of the
        /// cell should be painted by this method.</param>
        protected override void Paint(
            Graphics g,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowInd,
            DataGridViewElementStates cellState,
            object val,
            object fmtVal,
            string errorText,
            DataGridViewCellStyle style,
            DataGridViewAdvancedBorderStyle borderStyle,
            DataGridViewPaintParts paintParts)
        {
            DataGridView view = this.DataGridView;

            // If we don't have a host grid view, there's nowhere to paint... exit
            if (view == null)
                return;

            // Create the thread-local objects, if they aren't available to use
            if (_render == null)
            {
                _render = new Bitmap(
                    Math.Max(DefaultRenderWidth, cellBounds.Width),
                    Math.Max(DefaultRenderHeight, cellBounds.Height)
                );
            }

            if (_proxyCtl == null || _proxyCtl.IsDisposed)
            {
                // We create a control with fairly liberal min/max levels that
                // become our effective limits.
                _proxyCtl = new NumericUpDown()
                {
                    BorderStyle = BorderStyle.None,
                    Maximum = Decimal.MaxValue / 10,
                    Minimum = Decimal.MinValue / 10
                };
                // Remove the proxy if it's disposed of - it's no use to us at that
                // point
                _proxyCtl.Disposed += (s, e) => _proxyCtl = null;
            }

            // First paint the borders and background of the cell.
            // ie. everything that we've been told to paint other than the foreground
            // content and the error icon, which we'll get to later
            base.Paint(
                g, clipBounds, cellBounds, rowInd, cellState, val, fmtVal,
                errorText, style, borderStyle,
                paintParts & ~(DataGridViewPaintParts.ErrorIcon | DataGridViewPaintParts.ContentForeground));

            Point addr = view.CurrentCellAddress;

            // If the cell is in editing mode, there is nothing else to paint
            // So check if the current address matches this cell and the editing
            // control is visible in the datagrid view
            if (addr.X == ColumnIndex && addr.Y == rowInd &&
                view.EditingControl != null)
                return;

            // Paint the foreground - ie. paint the numeric up/down control
            if (paintParts.HasFlag(DataGridViewPaintParts.ContentForeground))
            {
                // Reduce our bounds in to allow for the borders
                Rectangle bw = BorderWidths(borderStyle);
                // Operate on a local value - we need cellBounds later for the call
                // to draw the error icon
                Rectangle bounds = cellBounds;
                bounds.Offset(bw.X, bw.Y);
                bounds.Width -= bw.Width;
                bounds.Height -= bw.Height;

                // Also take the padding into account
                Padding pad = style.Padding;
                if (pad != Padding.Empty)
                {
                    bounds.Offset(
                        view.RightToLeft == RightToLeft.Yes ? pad.Right : pad.Left,
                        pad.Top);
                    bounds.Width -= pad.Horizontal;
                    bounds.Height -= pad.Vertical;
                }

                // Determine the NumericUpDown control location within the bounds
                // of the cell
                bounds = AdjustEditingControlBounds(bounds, style);

                // If our rendering bitmap is too small, create a new one to
                // draw our control to
                if (_render.Width < bounds.Width || _render.Height < bounds.Height)
                {
                    _render.Dispose();
                    _render = new Bitmap(bounds.Width, bounds.Height);
                }

                // The proxy control needs to be parented to a visible control,
                // so given it the owning data grid view (if it doesn't already
                // have a visible parent)
                if (_proxyCtl.Parent == null || !_proxyCtl.Parent.Visible)
                    _proxyCtl.Parent = view;

                // Set all the relevant properties
                _proxyCtl.TextAlign = style.Alignment.TranslateToHorizontal();
                _proxyCtl.DecimalPlaces = DecimalPlaces;
                _proxyCtl.Font = style.Font;
                _proxyCtl.Width = bounds.Width;
                _proxyCtl.Height = bounds.Height;
                _proxyCtl.RightToLeft = view.RightToLeft;
                _proxyCtl.Text = fmtVal as string;
                _proxyCtl.ForeColor = style.ForeColor;

                // Hide the proxy control off the container so that it doesn't
                // actually get rendered to the screen (TabStop is off, so there
                // should be no way to get focus on it when it's not clickable).
                _proxyCtl.Location = new Point(0, -_proxyCtl.Height - 100);

                Color backColor;
                if (paintParts.HasFlag(DataGridViewPaintParts.SelectionBackground)
                    && cellState.HasFlag(DataGridViewElementStates.Selected))
                {
                    backColor = style.SelectionBackColor;
                }
                else
                {
                    backColor = style.BackColor;
                }
                if (paintParts.HasFlag(DataGridViewPaintParts.Background))
                {
                    // The NumericUpDown control does not support transparency
                    if (backColor.A < 255)
                        backColor = Color.FromArgb(255, backColor);
                    _proxyCtl.BackColor = backColor;
                }
                // Finally paint the NumericUpDown control
                Rectangle srcRect = new Rectangle(0, 0, bounds.Width, bounds.Height);
                if (srcRect.Width > 0 && srcRect.Height > 0)
                {
                    _proxyCtl.DrawToBitmap(_render, srcRect);
                    g.DrawImage(_render,
                        new Rectangle(bounds.Location, bounds.Size),
                        srcRect, GraphicsUnit.Pixel);
                }
            }
            if (paintParts.HasFlag(DataGridViewPaintParts.ErrorIcon))
            {
                // Paint the error icon on top of the NumericUpDown control
                base.Paint(
                    g, clipBounds, cellBounds, rowInd, cellState, val, fmtVal,
                    errorText, style, borderStyle,
                    DataGridViewPaintParts.ErrorIcon);
            }
        }

        /// <summary>
        /// Intitialises the editing control with the property values from this cell
        /// and the formatted value provided.
        /// </summary>
        /// <param name="rowInd">The row in which the editing control is being
        /// initialised</param>
        /// <param name="formatVal">The formatted value to set in the editing control
        /// </param>
        /// <param name="style">The cell style to apply to the editing control.
        /// </param>
        public override void InitializeEditingControl(
            int rowInd, object formatVal, DataGridViewCellStyle style)
        {
            base.InitializeEditingControl(rowInd, formatVal, style);
            NumericUpDown ctl = DataGridView.EditingControl as NumericUpDown;
            if (ctl != null)
            {
                ctl.BorderStyle = BorderStyle.None;
                ctl.DecimalPlaces = DecimalPlaces;
                ctl.Increment = Increment;
                ctl.Maximum = Maximum;
                ctl.Minimum = Minimum;
                ctl.Text = (formatVal as string ?? "");
            }
        }

        /// <summary>
        /// Checks if the given key event args describe a key which entered editing
        /// mode in this cell.
        /// For a numeric up/down cell, a digit or '-' key should enter edit mode.
        /// </summary>
        /// <param name="e">The key event args to test</param>
        /// <returns>True if the key event should cause edit mode to be entered in
        /// this cell.</returns>
        public override bool KeyEntersEditMode(KeyEventArgs e)
        {
            // Any modifier keys - not a digit
            if (e.Shift || e.Alt || e.Control)
                return false;

            // If the keycode represents a digit, straight off: edit mode
            if (char.IsDigit((char)e.KeyCode))
                return true;

            // If it's a numpad digit: edit mode
            if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
                return true;

            // Subtract or numpad - ? edit mode
            if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
                return true;

            // Left the most awkward one until last - check for the negative sign
            // according to the current culture. For Latin cultures, digits will have
            // been catered for above; for others, the negative sign may differ from
            // 'subtract'/'minus' sign.

            // Get the (first char of) the negative sign. If there is no -ve sign in
            // the current culture, this will be null - ie. char.MinValue.
            char sign =
                CultureInfo.CurrentCulture.NumberFormat.NegativeSign.FirstOrDefault();
            if (sign != char.MinValue && e.KeyCode == (Keys)VkKeyScan(sign))
                return true;

            // Finally - not a digit, not a negative sign in any form. Not edit mode
            return false;
        }

        #endregion

        #region - Protected Methods -

        /// <summary>
        /// Gets the value for this cell at the given row index. If there is a value
        /// at the given index, this ensures that it is converted to a decimal before
        /// being used.
        /// </summary>
        /// <param name="rowIndex">The index at which the value is required.</param>
        /// <returns>The numeric up/down value in the datagridview at the specified
        /// row index, converted into a decimal; or null if there is no value at the
        /// given row index.</returns>
        protected override object GetValue(int rowIndex)
        {
            object val = base.GetValue(rowIndex);
            return (val == null ? null : (object)Convert.ToDecimal(val));
        }

        /// <summary>
        /// Gets the preferred size of this cell, taking into account the up/down
        /// buttons to the right of the textbox.
        /// </summary>
        /// <param name="g">The graphics context with which to render any content
        /// to determine its size.</param>
        /// <param name="style">The style to apply to the cell when determining its
        /// size.</param>
        /// <param name="rowInd">The index of the row which this cell is being
        /// rendered within</param>
        /// <param name="constraintSize">The constrained size of the cell. Generally
        /// Width or Height is 0 indicating freedom of movement in that direction.
        /// </param>
        /// <returns>The preferred size of this cell, or (-1,-1) if this cell does
        /// not have an owning data grid view</returns>
        protected override Size GetPreferredSize(
            Graphics g, DataGridViewCellStyle style, int rowInd, Size constraintSize)
        {
            if (this.DataGridView == null)
                return new Size(-1, -1);

            Size sz = base.GetPreferredSize(g, style, rowInd, constraintSize);
            // If we have freedom of size in the horizontal, add space for the
            // up/down buttons and a little padding
            if (constraintSize.Width == 0)
            {
                const int ButtonsWidth = 16; // Width of the buttons.
                const int ButtonMargin = 8;  // Padding between text and buttons.
                sz.Width += ButtonsWidth + ButtonMargin;
            }
            return sz;
        }

        /// <summary>
        /// Gets the bounds in which the error icon should display. This shifts the
        /// bounds from the default for a textbox to ensure that the icon is shown
        /// on the inside of the spin buttons, not over the top of them.
        /// </summary>
        protected override Rectangle GetErrorIconBounds(
            Graphics g, DataGridViewCellStyle style, int rowInd)
        {
            const int ButtonWidth = 16;

            Rectangle r = base.GetErrorIconBounds(g, style, rowInd);
            // Subtract 'ButtonWidth' from the bounds X locn (add if going r-to-l)
            // such that the error icon appears on the inside of the up/down buttons
            r.X +=
                (DataGridView.RightToLeft == RightToLeft.Yes ? 1 : -1) * ButtonWidth;
            return r;
        }

        /// <summary>
        /// Gets a display value formatted as required by this cell, ensuring that
        /// the display properties (ie. <see cref="DecimalPlaces"/>) are honoured
        /// in the formatted output.
        /// </summary>
        /// <param name="value">The value to be formatted</param>
        /// <param name="rowInd">The index of the cell's parent row (may be different
        /// from <see cref="RowIndex"/> if this is in a shared row (in which case
        /// <see cref="RowIndex"/> will be -1.</param>
        /// <param name="style">The style in effect for this cell</param>
        /// <param name="valConv">A type converter associated with the value type
        /// that provides custom conversion to the formatted value type, or null if
        /// no such custom conversion is needed.</param>
        /// <param name="fmtValConv">A TypeConverter associated with the formatted
        /// value type that provides custom conversion from the value type, or null
        /// if no such custom conversion is needed.</param>
        /// <param name="ctx">A bitwise combination of
        /// <see cref="DataGridViewDataErrorContexts"/> values describing the context
        /// in which the formatted value is needed.</param>
        /// <returns>The formatted value of the cell or null if the cell does not
        /// belong to a DataGridView control.</returns>
        protected override object GetFormattedValue(
            object value,
            int rowInd,
            ref DataGridViewCellStyle style,
            TypeConverter valConv,
            TypeConverter fmtValConv,
            DataGridViewDataErrorContexts ctx)
        {
            object val = base.GetFormattedValue(
                value, rowInd, ref style, valConv, fmtValConv, ctx) as string;

            // If we get nothing at all, there's nothing we can do
            if (val == null) return null;

            // It shouldn't ever be anything other than a string
            Debug.Assert(val.GetType() == typeof(string),
                "Unexpected formatted value found", "Found base value: {0}", val + " of type " + val.GetType()
            );

            string fmtVal = val as string;

            // If we have a string format value and the (param) value is present
            if (!string.IsNullOrEmpty(fmtVal))
            {
                decimal unformattedDecVal = Convert.ToDecimal(value);
                decimal formattedDecVal = Convert.ToDecimal(fmtVal);
                if (formattedDecVal == unformattedDecVal)
                {
                    // The base implementation did nothing other than a straight
                    // ToString() call on the value. We need to use the configured
                    // DecimalPlaces value to format it the way the designer intended
                    return formattedDecVal.ToString("F" + DecimalPlaces);
                }
            }

            return fmtVal;
        }

        #endregion

        #region - Internal Methods -

        /// <summary>
        /// Sets the decimal places configured on this cell.
        /// </summary>
        /// <param name="rowInd">The index of the row affected by this change.
        /// </param>
        /// <param name="value">The new decimal places value to use for the cell.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="value"/>
        /// is less than zero or greater than 99</exception>
        /// <remarks>
        /// Also note that this method does not invalidate (ie. redraw) or alter the
        /// autosizing properties of the cell.
        /// </remarks>
        internal void SetDecimalPlaces(int rowInd, int value)
        {
            if (value < 0 || value > 99) throw new ArgumentOutOfRangeException(
                "DecimalPlaces must be between 0 and 99; Found: " + value);
            _decimalPlaces = value;
            if (OwnsEditingNumericUpDown(rowInd))
                EditingControl.DecimalPlaces = value;
        }

        /// <summary>
        /// Sets the increment configured on this cell
        /// </summary>
        /// <param name="rowInd">The index of the row affected by this change.
        /// </param>
        /// <param name="value">The new increment value to use for the cell.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="value"/>
        /// is less then zero.</exception>
        internal void SetIncrement(int rowInd, decimal value)
        {
            if (value < 0m) throw new ArgumentOutOfRangeException(
                "Increment cannot be less than zero; Found: " + value);
            _inc = value;
            if (OwnsEditingNumericUpDown(rowInd))
                EditingControl.Increment = value;
        }

        /// <summary>
        /// Sets the minimum constraint on a numeric up down cell, ensuring that the
        /// value in the cell is reduced to match the new constraint if necessary.
        /// </summary>
        /// <param name="rowInd">The index of the row affected by this change.
        /// </param>
        /// <param name="value">The new minimum value to use for the cell</param>
        /// <remarks>If the new minimum value is greater than the current maximum set
        /// in this cell, the maximum is reduced to match the minimum value.
        /// Also note that this method does not invalidate (ie. redraw) or alter the
        /// autosizing properties of the cell.
        /// </remarks>
        internal void SetMinimum(int rowInd, decimal value)
        {
            _min = value;
            // If the new minimum is larger than the the max, up the max to match it
            if (_min > _max)
                _max = _min;
            UpdateValueWithNewConstraints(rowInd);
            if (OwnsEditingNumericUpDown(rowInd))
                EditingControl.Minimum = value;
        }

        /// <summary>
        /// Sets the maximum constraint on a numeric up down cell, ensuring that the
        /// value in the cell is reduced to match the new constraint if necessary.
        /// </summary>
        /// <param name="rowInd">The index of the row affected by this change.
        /// </param>
        /// <param name="value">The new maximum value to use for the cell</param>
        /// <remarks>If the new maximum value is lower than the current minimum set
        /// in this cell, the minimum is reduced to match the maximum value.
        /// Also note that this method does not invalidate (ie. redraw) or alter the
        /// autosizing properties of the cell.
        /// </remarks>
        internal void SetMaximum(int rowInd, decimal value)
        {
            _max = value;
            // If the new maximum is smaller than the min, down the min to match it
            if (_min > _max)
                _min = _max;
            UpdateValueWithNewConstraints(rowInd);
            if (OwnsEditingNumericUpDown(rowInd))
                EditingControl.Maximum = value;
        }

        /// <summary>
        /// Updates the value at the given row index to fit within a new set of
        /// constraints (ie. new <see cref="Minimum"/> and/or <see cref="Maximum"/>.
        /// </summary>
        /// <param name="rowInd">The row index to constrain the value at.</param>
        void UpdateValueWithNewConstraints(int rowInd)
        {
            object cellValue = GetValue(rowInd);
            if (cellValue != null)
            {
                decimal currVal = Convert.ToDecimal(cellValue);
                decimal constrained = currVal;
                if (constrained < _min) constrained = _min;
                if (constrained > _max) constrained = _max;

                if (currVal != constrained)
                    SetValue(rowInd, constrained);
            }
        }

        /// <summary>
        /// Method called when any configuration value has changed on this cell
        /// (eg. DecimalPlaces, Maximum) which affects the rendering / size.
        /// Called when a cell characteristic that affects its rendering and/or preferred size has changed.
        /// This implementation only takes care of repainting the cells. The DataGridView's autosizing methods
        /// also need to be called in cases where some grid elements autosize.
        /// </summary>
        void OnConfigChanged()
        {
            var view = this.DataGridView;
            // No point invalidating if the view is not there or on its way out
            if (view == null || view.IsDisposed || view.Disposing)
                return;

            // If this cell has no owning row, ie. it is shared amongst rows
            if (RowIndex == -1)
            {
                // Invalidate the whole column
                view.InvalidateColumn(ColumnIndex);

                //FIXME: Unlike when the cell has an owning row, this doesn't autosize columns/rows...
            }
            else
            {
                // The DataGridView control exposes a public method called UpdateCellValue
                // that invalidates the cell so that it gets repainted and also triggers all
                // the necessary autosizing: the cell's column and/or row, the column headers
                // and the row headers are autosized depending on their autosize settings.
                view.UpdateCellValue(ColumnIndex, RowIndex);
            }
        }

        /// <summary>
        /// Checks whether this cell owns the currently editing numeric up down
        /// control or not. Note that the row index is needed, because this cell may
        /// be shared amongst multiple rows (whereas a cell is fixed on a column, so
        /// the column is not variable at this level).
        /// </summary>
        /// <param name="rowInd">The row index to check to see if this cell is the
        /// current owner of the editing numeric up down control.</param>
        bool OwnsEditingNumericUpDown(int rowInd)
        {
            DataGridView view;
            if (rowInd == -1 || (view = this.DataGridView) == null)
            {
                return false;
            }
            var ctl = view.EditingControl as DataGridViewNumericUpDownEditingControl;
            return (ctl != null
                && ((IDataGridViewEditingControl)ctl).EditingControlRowIndex == rowInd
                && view.CurrentCellAddress.X == ColumnIndex);
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
        /// <returns>True if the <see cref="Minimum"/> value set in this cell is not
        /// the current default value.</returns>
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
        /// <returns>True if the <see cref="Maximum"/> value set in this cell is not
        /// the current default value.</returns>
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
        /// <returns>True if the <see cref="Increment"/> value set in this cell is not
        /// the current default value.</returns>
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
