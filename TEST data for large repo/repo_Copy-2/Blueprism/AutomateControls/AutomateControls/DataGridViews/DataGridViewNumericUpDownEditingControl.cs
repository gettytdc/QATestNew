using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// Editing control used in NumericUpDown cells within a DataGridView, which is
    /// used to capture numeric values.
    /// </summary>
    public class DataGridViewNumericUpDownEditingControl
        : NumericUpDown, IDataGridViewEditingControl
    {
        #region - Auto-Properties -

        /// <summary>
        /// Gets or sets the data grid view that this editing control belongs to
        /// </summary>
        public DataGridView EditingControlDataGridView { get; set; }

        /// <summary>
        /// Gets or sets the row index that this control is operating on
        /// </summary>
        public int EditingControlRowIndex { get; set; }

        /// <summary>
        /// Gets or sets whether the control value has changed in this edit control
        /// </summary>
        public bool EditingControlValueChanged { get; set; }

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new editing control for numeric up/down datagridview cells
        /// </summary>
        public DataGridViewNumericUpDownEditingControl()
        {
            // Apparently, this must not be involved in the tab order
            TabStop = false;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets the text box subcontrol of this control, if one is present; null
        /// otherwise.
        /// </summary>
        TextBox TextBoxSubControl
        {
            get
            {
                if (Controls.Count < 2) return null;
                return Controls[1] as Textboxes.StyledTextBox;
            }
        }

        #endregion

        #region IDataGridViewEditingControl Members

        /// <summary>
        /// Applies the given cell style to this control, taking from it the font,
        /// backcolour (making it opaque if it has any transparency), forecolour and
        /// alignment.
        /// </summary>
        /// <param name="style">The style to apply to this control</param>
        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle style)
        {
            Font = style.Font;
            // If there's transparency, we have to remove it
            if (style.BackColor.A < 255)
            {
                // The NumericUpDown control does not support transparent back colors
                Color opaque = Color.FromArgb(255, style.BackColor);
                BackColor = opaque;
                EditingControlDataGridView.EditingPanel.BackColor = opaque;
            }
            else
            {
                BackColor = style.BackColor;
            }
            ForeColor = style.ForeColor;
            TextAlign = style.Alignment.TranslateToHorizontal();
        }

        /// <summary>
        /// Checks if the control wants the given input key. This depends on the
        /// current state of the control and whether the grid view itself has any
        /// use for the key (if it doesn't, the key is passed to the control anyway).
        /// </summary>
        /// <param name="keyData">The key data to test to see if it is of use to
        /// the control</param>
        /// <param name="viewWantsKey">Whether the host data grid view wants to use
        /// the key if the control does not</param>
        /// <returns>true if the editing control should be passed the input key;
        /// false if it is not interested in it.</returns>
        public bool EditingControlWantsInputKey(Keys keyData, bool viewWantsKey)
        {
            // Most of these keys require the textbox is some form
            TextBox box = TextBoxSubControl;
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Right:
                    if (box != null && box.IsCaretAtEndOfText(this.RightToLeft))
                        return true;
                    break;

                case Keys.Left:
                    if (box != null && box.IsCaretAtStartOfText(this.RightToLeft))
                        return true;
                    break;

                case Keys.Down:
                    if (Value > Minimum)
                        return true;
                    break;

                case Keys.Up:
                    if (Value < Maximum)
                        return true;
                    break;

                case Keys.Home: case Keys.End:
                    // give to the control if text is not fully selected (?)
                    if (box != null && box.SelectionLength != box.Text.Length)
                        return true;
                    break;

                case Keys.Delete:
                    // give to the control if anything is selected or the caret is
                    // not at the end of the text (ie. there is something to delete)
                    if (box != null &&
                        (box.SelectionLength > 0 || !box.IsCaretAtEndOfText(this.RightToLeft)))
                    {
                        return true;
                    }
                    break;
            }

            // Otherwise, give it to the control if the view doesn't want it
            return !viewWantsKey;
        }

        /// <summary>
        /// Gets or sets the formatted value from this edit control
        /// </summary>
        public object EditingControlFormattedValue
        {
            get
            {
                return GetEditingControlFormattedValue(
                    DataGridViewDataErrorContexts.Formatting);
            }
            set { this.Text = (string)value; }
        }

        /// <summary>
        /// The cursor to use for the editing panel which holds this control
        /// </summary>
        public Cursor EditingPanelCursor
        {
            get { return Cursors.Default; }
        }

        /// <summary>
        /// Gets the formatted current value of the editing control
        /// </summary>
        /// <param name="context">The context in which the formatted value is
        /// required.</param>
        /// <returns>The current value of the editing control</returns>
        public object GetEditingControlFormattedValue(
            DataGridViewDataErrorContexts context)
        {
            // Note: the MSDN example I learned all this from temporarily set the UserEdit
            // property to true if the context didn't have the 'Display' flag set;
            // false otherwise. At the moment I'm not sure why, but the comment says:
            // "Prevent the Value from being set to Maximum or Minimum when the cell is being painted."
            // It also formatted the string more - ie. thousands sep (which I'm not adding)
            return Value.ToString("F" + DecimalPlaces);
        }

        /// <summary>
        /// Prepares the editing control for entering edit mode
        /// </summary>
        /// <param name="selectAll">True to select all contents; False otherwise.
        /// </param>
        public void PrepareEditingControlForEdit(bool selectAll)
        {
            TextBox box = TextBoxSubControl;
            if (box == null) return;

            if (selectAll)
                box.SelectAll();
            else
                box.SelectionStart = box.Text.Length;
        }

        /// <summary>
        /// Gets whether the editing control needs reposition on a value change.
        /// It does not.
        /// </summary>
        public bool RepositionEditingControlOnValueChange
        {
            get { return false; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Performs any processing required within this control to prepare it for
        /// detachment from the host data grid view. This doesn't actually perform
        /// the attachment (see
        /// <see cref="DataGridViewNumericUpDownCell.DetachEditingControl"/>); it
        /// just ensures that the same control can be used with different cells
        /// without confusion occurring between them by clearing the undo buffer of
        /// the associated text box.
        /// </summary>
        internal void Detach()
        {
            TextBox box = TextBoxSubControl;
            if (box != null)
                box.ClearUndo();
        }

        /// <summary>
        /// The key event which propogates this edit control being entered needs to
        /// be sent to the text box sub control
        /// </summary>
        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            TextBox box = TextBoxSubControl;
            if (box != null)
            {
                WinUtil.SendMessage(box.Handle, m.Msg, m.WParam, m.LParam);
                return true;
            }
            else
            {
                return base.ProcessKeyEventArgs(ref m);
            }
        }

        /// <summary>
        /// Handles the value changing in this control, ensuring that the owning
        /// view is notified of the value change.
        /// </summary>
        protected override void OnValueChanged(EventArgs e)
        {
            base.OnValueChanged(e);
            if (Focused)
                NotifyValueChanged();
        }

        /// <summary>
        /// Marks this editing control as having had its value changed, and notifies
        /// the owning view that the current cell (ie. the one that this control is
        /// modifying) is 'dirty', ie. has a new value.
        /// </summary>
        void NotifyValueChanged()
        {
            if (!EditingControlValueChanged)
            {
                EditingControlValueChanged = true;
                EditingControlDataGridView.NotifyCurrentCellDirty(true);
            }
        }

        /// <summary>
        /// Listen to the KeyPress notification to know when the value changed, and
        /// notify the grid of the change.
        /// </summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            // The value changes when a digit, the decimal separator, the group
            // separator or the negative sign is pressed.
            bool notify = false;
            if (char.IsDigit(e.KeyChar))
            {
                notify = true;
            }
            else
            {
                // Gather the symbols for this culture. If the key char is in any
                // of them, then treat it as a value change
                NumberFormatInfo numFmt = CultureInfo.CurrentCulture.NumberFormat;
                string symbols = numFmt.NumberDecimalSeparator +
                    numFmt.NumberGroupSeparator + numFmt.NegativeSign;
                notify = (symbols.IndexOf(e.KeyChar) != -1);
            }

            // Let the DataGridView know about the value change (if there is one)
            if (notify)
                NotifyValueChanged();
        }

        #endregion
    }
}
