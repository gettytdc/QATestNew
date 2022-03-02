using System.Drawing;

namespace AutomateControls
{
    /// <summary>
    /// A basic combo box item which contains the text and a tag.
    /// </summary>
    public class ComboBoxItem
    {
        #region - Member Variables -

        // The colour that should be used to paint this item's label (when enabled)
        private Color _color;

        private bool _enabled;

        #endregion

        #region - Constructors -

        public ComboBoxItem(string text)
            : this(text, SystemColors.WindowText, null, true) { }

        public ComboBoxItem(string text, Color color)
            : this(text, color, null, true) { }

        public ComboBoxItem(string text, object tag)
            : this(text, SystemColors.WindowText, tag, true) { }

        public ComboBoxItem(string text, Color color, object tag)
            : this(text, color, tag, true) { }

        public ComboBoxItem(string text, bool enabled)
            : this(text, SystemColors.WindowText, null, enabled) { }

        public ComboBoxItem(string text, Color color, bool enabled)
            : this(text, color, null, enabled) { }

        public ComboBoxItem(string text, object tag, bool enabled)
            : this(text, SystemColors.WindowText, tag, enabled) { }

        public ComboBoxItem(string text, Color color, object tag, bool enabled)
        {
            _color = color;
            Text = text;
            Tag = tag;
            Enabled = enabled;
        }

        #endregion

        #region - Auto Properties -

        /// <summary>
        /// Used in multi-select combo boxes to represent if this item is checked.
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        /// Used in mult-select to indicate that a checkbox may be shown
        /// </summary>
        public bool Checkable { get; set; }

        /// <summary>
        /// The text representing this item in the combo box
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The data tag associated with this combo box item
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Get/Set the enabled state of this item.
        /// </summary>
        public bool Enabled { get { return _enabled; } set { Selectable = value; _enabled = value; } }

        /// <summary>
        /// Get/Set the selectable state of this item.
        /// </summary>
        public bool Selectable { get; set; }

        /// <summary>
        /// The font style for this combo box item.
        /// </summary>
        public FontStyle Style { get; set; }

        /// <summary>
        /// An indent to use when rendering this item - this is measured in pixels
        /// </summary>
        public int Indent { get; set; }

        #endregion

        #region - Properties -

        /// <summary>
        /// The text colour for this combo box item. Ignored when the item is
        /// disabled or currently selected.
        /// </summary>
        public Color Color
        {
            get { return (_color == Color.Empty ? SystemColors.WindowText : _color); }
            set { _color = value; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Overrides the base ToString() method to return the combo box
        /// label - trying to get the base combo box to display the correct
        /// label any other way was just too painful.
        /// </summary>
        /// <returns>A string representation of this item - its label.</returns>
        public override string ToString()
        {
            return this.Text;
        }

        #endregion
    }
}
