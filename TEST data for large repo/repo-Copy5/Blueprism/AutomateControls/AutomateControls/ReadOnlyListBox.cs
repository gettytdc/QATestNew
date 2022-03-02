using System;
using System.Windows.Forms;

namespace AutomateControls
{
    /// <summary>
    /// ListBox which can be set to be read only
    /// </summary>
    public class ReadOnlyListBox : ListBox
    {
        public event EventHandler ReadOnlyChanged;

        // Flag indicating if the box is read only
        private bool _readOnly;

        /// <summary>
        /// Sets the listbox to be read only - ie. to not allow selection from the
        /// mouse or keyboard
        /// </summary>
        public bool ReadOnly
        {
            get { return _readOnly; }
            set
            {
                if (_readOnly != value)
                {
                    _readOnly = value;
                    OnReadOnlyChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ReadOnlyChanged"/> event
        /// </summary>
        /// <param name="e">The arguments detailing the event</param>
        protected virtual void OnReadOnlyChanged(EventArgs e)
        {
            EventHandler handler = this.ReadOnlyChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Handles the message, ensuring it reaches the default windows procedure,
        /// or is filtered out as appropriate.
        /// </summary>
        /// <param name="m">The windows message in question</param>
        protected override void DefWndProc(ref Message m)
        {
            // (Originally from http://ajeethtechnotes.blogspot.co.uk/2009/02/readonly-listbox.html)
            // If ReadOnly is set to true, then block any messages
            // to the selection area from the mouse or keyboard.
            // Let all other messages pass through to the
            // Windows default implementation of DefWndProc.
            if (!_readOnly || (
                (m.Msg <= 0x0200 || m.Msg >= 0x020E)
                && (m.Msg <= 0x0100 || m.Msg >= 0x0109)
                && m.Msg != 0x2111
                && m.Msg != 0x87))
            {
                base.DefWndProc(ref m);
            }
        }
    }
}
