using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace AutomateControls
{
    /// <summary>
    /// Text box which allows a number to be entered, and sets a label
    /// with the appropriate ordinal suffix, such that (eg.) entering
    /// the number '1' will display the suffix: "st", and the number '2'
    /// will display "nd".
    /// </summary>
    public partial class OrdinalNumberTextBox : UserControl
    {
        /// <summary>
        /// The regular expression which details a valida number
        /// </summary>
        private static readonly Regex RX_VALID_NUMBER = 
            new Regex("^[0-9]*$", RegexOptions.Compiled);

        /// <summary>
        /// The number currently held by this control
        /// </summary>
        [Browsable(true),EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text
        {
            get { return mNumberBox.Text; }
            set { mNumberBox.Text = value; }
        }

        /// <summary>
        /// The integer value currently held... 0 if no valid integer 
        /// value is currently set. Note that setting to a value of 0
        /// effectively clears the text from the text box.
        /// </summary>
        public int Value
        {
            get
            {
                int val;
                if (int.TryParse(Text,out val) && val > 0) return val;
                return 0;
            }
            set
            {
                this.Text = (value > 0 ? value.ToString() : "");
            }
        }

        /// <summary>
        /// Creates a new ordinal number text box with no current value.
        /// </summary>
        public OrdinalNumberTextBox() : this(0) { }

        /// <summary>
        /// Creates a new ordinal number text box representing the given
        /// value.
        /// </summary>
        /// <param name="value">The number value to set in the text box.</param>
        public OrdinalNumberTextBox(int value)
        {
            InitializeComponent();
            if (value != 0) // don't set a value or an ordinal for 0 entries
            {
                string strValue = value.ToString();
                mNumberBox.Text = strValue;
                mOrdinalLabel.Text = getOrdinal(strValue);
            }
            mNumberBox.Enter += new EventHandler(mNumberBox_Enter);
            mNumberBox.TextChanged += new EventHandler(UpdateOrdinal);
            mNumberBox.KeyPress += new KeyPressEventHandler(mNumberBox_KeyPress);
            mNumberBox.Validating += new CancelEventHandler(mNumberBox_Validating);
        }

        /// <summary>
        /// Ensures that the text is selected when the box is entered.
        /// </summary>
        private void mNumberBox_Enter(object sender, EventArgs e)
        {
            mNumberBox.SelectAll();
        }

        /// <summary>
        /// Validates the text in the number box - arbitrary text can be 
        /// pasted in using the mouse, so we still need to check.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mNumberBox_Validating(object sender, CancelEventArgs e)
        {
            if (!RX_VALID_NUMBER.IsMatch(mNumberBox.Text))
            {
                mNumberBox.Text = "";
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Half-hearted attempts to 'mask' the text box, only allowing numbers
        /// in... Of course, this doesn't stop stuff being pasted in... for which
        /// (of course) there is no event handler. There's no 'TextChanging' or
        /// 'PreviewTextChanged' event which we could use to stop text going in,
        /// and we can't get or set the caret position within the text box... 
        /// meaning if we revert changes within the TextChanged event, the caret
        /// will end up at the end of the text rather than wherever it was
        /// beforehand. Thus, half-hearted is about all we can do.... then
        /// validate on blur.
        /// </summary>
        void mNumberBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ("0123456789\b".IndexOf(e.KeyChar) == -1)
            {
                // Ban this sick filth
                e.Handled = true;
            }
        }

        /// <summary>
        /// Gets the ordinal corresponding to the given string value
        /// This parses the given value and returns the correct ordinal
        /// suffix after converting it into an integer - if the string
        /// could not be converted, an empty string is returned.
        /// </summary>
        /// <param name="strValue">The string value to generate the
        /// ordinal suffix for.</param>
        /// <returns>The appropriate ordinal suffix for the given string
        /// value, or an empty string if the given value did not represent
        /// a valid integer number.</returns>
        private string getOrdinal(string strValue)
        {
            try
            {
                int value = int.Parse(strValue);
                // special case - if it's between 10 and 20, then the
                // suffix is always a "th" (11th, 12th, 113th etc)
                if (((value / 10) % 10) == 1)
                {
                    return "th";
                }
                switch (value % 10)
                {
                    case 1: return "st";
                    case 2: return "nd";
                    case 3: return "rd";
                    default: return "th";
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Handles the number box changing value and sets the ordinal
        /// suffix appropriately.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments detailing the event</param>
        private void UpdateOrdinal(object sender, EventArgs e)
        {
            mOrdinalLabel.Text = getOrdinal(mNumberBox.Text);
        }

    }
}
