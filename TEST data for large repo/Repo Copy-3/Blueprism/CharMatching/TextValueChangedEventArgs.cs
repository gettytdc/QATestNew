using System;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Delegate implementing a listener for TextValueChanged events
    /// </summary>
    public delegate void TextValueChangedEventHandler(
        object sender, TextValueChangedEventArgs e);

    /// <summary>
    /// Event Args detailing a text value changing, typically within a character
    /// pair or in a CharData object
    /// </summary>
    public class TextValueChangedEventArgs : EventArgs
    {
        // The new character set after the change
        private string _newVal;

        // The old character from before the change
        private string _oldVal;

        /// <summary>
        /// Creates a new TextValueChanged event args object with the before and
        /// after chars
        /// </summary>
        /// <param name="oldVal">The text value from before the change</param>
        /// <param name="newVal">The text value from after the change</param>
        public TextValueChangedEventArgs(string oldVal, string newVal)
        {
            _oldVal = oldVal;
            _newVal = newVal;
        }

        /// <summary>
        /// The text value from before the change
        /// </summary>
        public string OldValue { get { return _oldVal; } }

        /// <summary>
        /// The text value from after the change
        /// </summary>
        public string NewValue { get { return _newVal; } }

    }
}
