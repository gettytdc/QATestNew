using System;
using System.Windows.Forms;
using AutomateControls.WindowsSupport;

namespace AutomateControls
{
    /// <summary>
    /// ComboBox which can be set to be readonly - this only supports drop-down-list
    /// combo boxes and it will resist any attempt to set the box to any other style
    /// </summary>
    public class ReadOnlyComboBox : ComboBox
    {
        #region - Member Variables -

        // Indicates if this combo box is currently set as readonly
        private bool _readOnly;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new drop down list combo box which supports a readonly mode.
        /// </summary>
        public ReadOnlyComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets or sets the read-only status of this combo box.
        /// </summary>
        public bool ReadOnly
        {
            get { return _readOnly; }
            set { _readOnly = value; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Handles the drop down style changing, in effect disabling the style
        /// changing to anything other than drop down list.
        /// </summary>
        protected override void OnDropDownStyleChanged(EventArgs e)
        {
            // Ignore non-dropdownlists, that's all we support.
            if (DropDownStyle != ComboBoxStyle.DropDownList)
                DropDownStyle = ComboBoxStyle.DropDownList;
        }

        /// <summary>
        /// Handles the pre-processing for the given windows message. If the combo
        /// box is not set to readonly, this just uses the default handling.
        /// Otherwise, it ensures that any keypress messages are inhibited other than
        /// those used to transfer focus from the control.
        /// </summary>
        /// <param name="msg">The windows message to perform preprocessing for.
        /// </param>
        /// <returns>True if the message has been handled by this method; False
        /// otherwise.</returns>
        public override bool PreProcessMessage(ref Message msg)
        {
            if (!_readOnly)
                return base.PreProcessMessage(ref msg);

            // We want to ignore (ie. mark as handled) any non-focus shifting presses
            bool handled = false;

            if (msg.Msg == WindowsMessage.WM_KEYDOWN)
            {
                Keys keyCode = (Keys)msg.WParam & Keys.KeyCode;
                if (keyCode != Keys.Tab)
                    handled = true;
            }

            if (!handled)
                handled = base.PreProcessMessage(ref msg);

            return handled;
        }

        /// <summary>
        /// Handles the processing for the given windows message. If this combo box
        /// is not currently set to be readonly, this just uses the default handling.
        /// Otherwise, it ensures that left mouse button clicks are ignored for the
        /// purposes of dropping down the list.
        /// </summary>
        /// <param name="m">The windows message to process</param>
        protected override void WndProc(ref Message m)
        {
            if (!_readOnly)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case WindowsMessage.WM_LBUTTONDOWN:
                case WindowsMessage.WM_LBUTTONDBLCLK:
                    // We still want the control to be focused if it isn't currently
                    // focused and the user clicked it, but that's all, so do it
                    // programmatically.
                    if (!Focused)
                        Focus();
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #endregion

    }
}
