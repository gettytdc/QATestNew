using System;
using System.Windows.Forms;
namespace AutomateControls.Forms
{
    /// <summary>
    /// Base form which can be used as a standard base class for automate forms.
    /// </summary>
    public partial class AutomateForm : Form
    {
        /// <summary>
        /// Event fired when this form is minimized
        /// </summary>
        public event EventHandler Minimized;

        /// <summary>
        /// Event fired when this form is maximized
        /// </summary>
        public event EventHandler Maximized;

        /// <summary>
        /// Event fired when this form is restored, ie. un-minimized or un-maximized
        /// </summary>
        public event EventHandler Restored;

        // The last window state recorded for this form
        private FormWindowState _lastState;

        /// <summary>
        /// Creates a new, empty automate form
        /// </summary>
        public AutomateForm()
        {
            InitializeComponent();
            _lastState = this.WindowState;
        }

        /// <summary>
        /// Triggers an OnResize event and determines whether a Minimized, Maximized
        /// or Restored event (or a combination thereof) is appropriate, firing them
        /// if so.
        /// </summary>
        /// <param name="e">The event args detailing the resize event</param>
        protected override void  OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            FormWindowState state = this.WindowState;
            FormWindowState lastState = _lastState;
            if (state == lastState)
                return;
            // Set the new state in the object now - we can test the state before
            // this method was entered with our local variable
            _lastState = state;

            switch (state)
            {
                case FormWindowState.Normal:
                    OnRestored(e);
                    break;

                case FormWindowState.Maximized:
                    if (lastState == FormWindowState.Minimized)
                        OnRestored(e);
                    if (Maximized != null)
                        OnMaximized(e);
                    break;

                case FormWindowState.Minimized:
                    if (Minimized != null)
                        OnMinimized(e);
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="Restored"/> event
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnRestored(EventArgs e)
        {
            EventHandler handle = this.Restored;
            if (handle != null)
                handle(this, e);
        }

        /// <summary>
        /// Raises the <see cref="Minimized"/> event
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnMinimized(EventArgs e)
        {
            EventHandler handle = this.Minimized;
            if (handle != null)
                handle(this, e);
        }

        /// <summary>
        /// Raises the <see cref="Maximized"/> event
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnMaximized(EventArgs e)
        {
            EventHandler handle = this.Maximized;
            if (handle != null)
                handle(this, e);
        }

    }
}
