using System;
using System.Windows.Forms;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Control which encapsulates the 'shifting' functions, allowing characters to
    /// be edited
    /// </summary>
    public partial class Shifter : UserControl
    {
        /// <summary>
        /// Event fired when a shift operation button is clicked
        /// </summary>
        public event ShiftOperationEventHandler ShiftOperationClick;

        /// <summary>
        /// Creates a new initialized shifter control
        /// </summary>
        public Shifter()
        {
            InitializeComponent();
            foreach (Control ctl in this.Controls)
            {
                ShifterButton btn = ctl as ShifterButton;
                if (btn != null)
                {
                    ttShifter.SetToolTip(btn, btn.TooltipText);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ShiftOperationClick"/> event
        /// </summary>
        protected virtual void OnShiftOperationClick(ShiftOperationEventArgs e)
        {
            ShiftOperationEventHandler handler = ShiftOperationClick;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Handles a shift button being clicked. This extracts the operation that
        /// the clicked button represents and propogates the event into a
        /// <see cref="ShiftOperationClick"/> event fired by this control
        /// </summary>
        private void HandleShiftButtonClick(object sender, EventArgs e)
        {
            ShifterButton btn = sender as ShifterButton;
            OnShiftOperationClick(new ShiftOperationEventArgs(btn.Operation));
        }

    }
}
