using System;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Delegate for handling events fired when a CharData instance is modified.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The args detailing the old and new char</param>
    public delegate void CharDataEventHandler(object sender, CharDataEventArgs e);

    /// <summary>
    /// Arguments detailing the event fired when a CharData object is changed
    /// </summary>
    public class CharDataEventArgs : EventArgs
    {
        // The data object in question
        private CharData _data;

        // The old value set in the char data object before the change occurred
        private string _oldVal;

        /// <summary>
        /// Creates a new event args object with no old character based on the
        /// given CharData object.
        /// </summary>
        /// <param name="data">The CharData object which is the subject of the change
        /// </param>
        public CharDataEventArgs(CharData data) : this(data, CharData.NullValue) { }

        /// <summary>
        /// Creates a new event args object based on the given CharData object, with
        /// the specified old character
        /// </summary>
        /// <param name="oldVal">The old text value in the CharData object before it
        /// was changed. <see cref="CharData.NullValue"/> indicates that there was
        /// no text value in the CharData, or no old character is to be recorded in
        /// this event args.</param>
        /// <param name="data">The CharData object which is the subject of the change
        /// </param>
        public CharDataEventArgs(CharData data, string oldVal)
        {
            _data = data;
            _oldVal = oldVal;
        }

        /// <summary>
        /// The data object which has changed.
        /// </summary>
        public CharData Data { get { return _data; } }

        /// <summary>
        /// The old text value in the data object before the change. If this is
        /// <see cref="CharData.NullValue"/> that means either that there was no char
        /// previously set in the data object, or it was not recorded in this event.
        /// </summary>
        public string OldValue { get { return _oldVal; } }

        /// <summary>
        /// The new text value set in the data object. This is equivalent to getting
        /// <see cref="Data"/>.<see cref="CharData.Value">Value</see>.
        /// </summary>
        public string NewValue { get { return _data.Value; } }

    }
}
