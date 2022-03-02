using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace AutomateControls
{
    /// <summary>
    /// Textbox which fires an Activating event when the enter key is pressed within it.
    /// Note that this event is <em>not</em> fired if the text box is configured to
    /// accept the return key within it - often the case for multiline text boxes.
    /// </summary>
    public class ActivatingTextBox : Textboxes.StyledTextBox
    {
        /// <summary>
        /// Event fired when the text box is 'activated' by pressing
        /// the enter key within it.
        /// </summary>
        [Category("Behavior"), Description(
         "Fired when the textbox is activated by pressing the Enter key")]
        public event EventHandler Activated;

        /// <summary>
        /// Event fired when the text box is 'escaped' by pressing the escape key
        /// within it
        /// </summary>
        [Category("Behavior"), Description(
         "Fired when the textbox is escaped by pressing the Escape key")]
        public event EventHandler Escaped;

        // Flag to indicate if handling the escape key down event is enabled
        private bool _trapEscape;

        // Flag to indicate if handling the enter key down event is enabled
        private bool _trapEnter;

        /// <summary>
        /// Creates a new textbox which can be <see cref="Activated"/> or
        /// <see cref="Escaped"/>
        /// </summary>
        public ActivatingTextBox()
        {
            _trapEnter = true;
            _trapEscape = false;
        }

        [Browsable(true), Category("Behavior"), DefaultValue(false), Description(
         "Traps the escape key as well as the enter key")]
        public bool TrapEscape
        {
            get { return _trapEscape; }
            set { _trapEscape = value; }
        }

        [Browsable(true), Category("Behavior"), DefaultValue(true), Description(
         "Traps the escape key as well as the enter key")]
        public bool TrapEnter
        {
            get { return _trapEnter; }
            set { _trapEnter = value; }
        }

        /// <summary>
        /// Tests if the given key is an input key for this textbox.
        /// </summary>
        /// <param name="key">The key to test</param>
        /// <returns>True if this textbox is an input key - ie. a key that this text
        /// box should handle; False otherwise.</returns>
        protected override bool IsInputKey(Keys key)
        {
            // Need to use the same tests as in the OnKeyDown method
            return (_trapEnter && !AcceptsReturn && key == Keys.Enter)
                || (_trapEscape && key == Keys.Escape)
                || base.IsInputKey(key);
        }

        /// <summary>
        /// Handles a keydown event, ensuring that the <see cref="Activated"/> or
        /// <see cref="Escaped"/> event is fired as appropriate
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // NOTE: If the following tests change, the corresponding tests in
            // IsInputKey() should be modified too.

            // if it's accepting return we don't want to interfere, even if our
            // 'trapEnter' is set.
            if (_trapEnter && !this.AcceptsReturn && e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // inhibits the bell sound on keypress
                OnActivated(EventArgs.Empty);
            }
            if (_trapEscape && e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OnEscaped(EventArgs.Empty);
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Fires the <see cref="Activated"/> event
        /// </summary>
        protected virtual void OnActivated(EventArgs e)
        {
            EventHandler handler = this.Activated;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Fires the <see cref="Escaped"/> event
        /// </summary>
        protected virtual void OnEscaped(EventArgs e)
        {
            EventHandler handler = this.Escaped;
            if (handler != null)
                handler(this, e);
        }
    }
}
