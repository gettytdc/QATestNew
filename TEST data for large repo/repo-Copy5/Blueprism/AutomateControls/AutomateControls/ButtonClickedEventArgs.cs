using System.Windows.Forms;

namespace AutomateControls
{
    /// <summary>
    /// Delegate describing an event handler for button clicks
    /// </summary>
    /// <param name="sender">The source of the event - typically this is the form
    /// or control which is passing on the event rather than the precise button,
    /// which is identifiable from the args.</param>
    /// <param name="e">The args detailing the button which was clicked</param>
    public delegate void ButtonClickedEventHandler(
        object sender, ButtonClickedEventArgs e);

    /// <summary>
    /// Args object detailing that a button has been clicked
    /// </summary>
    public class ButtonClickedEventArgs
    {
        // The button text
        private string _text;

        // The button which was clicked
        private Button _btn;

        /// <summary>
        /// Creates a new ButtonClickedEventArgs indicating that the given button
        /// has been clicked
        /// </summary>
        /// <param name="btn">The button which was clicked</param>
        public ButtonClickedEventArgs(Button btn)
        {
            _btn = btn;
            _text = btn.Text;
        }

        /// <summary>
        /// The text on the button which was pressed
        /// </summary>
        public string ButtonText { get { return _text; } }

        /// <summary>
        /// The button itself
        /// </summary>
        public Button Button { get { return _btn; } }
    }
}
