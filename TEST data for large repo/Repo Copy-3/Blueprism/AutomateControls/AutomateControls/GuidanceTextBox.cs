using System;
using System.Drawing;
using System.ComponentModel;


namespace AutomateControls
{
    /// <summary>
    /// Activating TextBox which displays guidance text when it does not have
    /// focus and it is currently empty.
    /// </summary>
    public class GuidanceTextBox : ActivatingTextBox
    {
        #region - Class scope declarations -

        /// <summary>
        /// The modes available for this textbox.
        /// </summary>
        protected enum Mode { Normal, ShowGuidance }
        
        #endregion

        #region - Member variables -

        // The mode that this textbox is currently in.
        private Mode _mode;

        // The forecolor of the guidance textbox, saved at the point that
        // ShowGuidance mode was entered
        private Color _savedForeColor;

        // The color to use for the guidance text when this textbox is in
        // ShowGuidance mode
        private Color _guidanceColor;

        // The guidance text to display when this textbox is in
        // ShowGuidance mode
        private string _guidanceText;

        // Flag which indicates that the text should be selected when this
        // box gains focus.
        private bool _selectOnFocus;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new guidance text box with the default guidance text
        /// displayed in the colour defined in <see cref="SystemColors.GrayText"/>
        /// </summary>
        public GuidanceTextBox()
            : this("", SystemColors.GrayText) { }

        /// <summary>
        /// Creates a new guidance text box with the given text, displayed in
        /// the colour defined in <see cref="SystemColors.GrayText"/>
        /// </summary>
        /// <param name="guidanceText">The text that this textbox should display
        /// when it is empty and does not currently have focus.</param>
        public GuidanceTextBox(string guidanceText)
            : this(guidanceText, SystemColors.GrayText) { }

        /// <summary>
        /// Creates a new guidance text box with the given text, displayed as
        /// guidance in the specified colour.
        /// </summary>
        /// <param name="guidanceText">The text to display as guidance to the
        /// user when the textbox is empty and does not have focus.</param>
        /// <param name="guidanceColor">The colour to use to paint the guidance
        /// text displayed in this box.</param>
        public GuidanceTextBox(string guidanceText, Color guidanceColor)
        {
            _guidanceText = guidanceText;
            _guidanceColor = guidanceColor;

            // Set the mode to be 'GuidanceShown' on creation.
            CurrentMode = Mode.ShowGuidance;
            // On creation, the colour gets set to WindowText, and not the default
            // (which is ControlText). No idea why, but it's causing a non-default
            // value to appear in the designer properties when the control is added
            _savedForeColor = SystemColors.ControlText;
        }

        #endregion

        #region - Overridden / overloaded properties -

        /// <summary>
        /// Overrides the forecolor for this text box to ensure that
        /// setting the forecolor while the textbox is in ShowGuidance
        /// mode is not ignored when the box returns to normal mode
        /// </summary>
        [Category("Appearance"), Description(
          "The foreground color of this component, which is used to display text")]
        public new Color ForeColor
        {
            get
            {
                if (_mode == Mode.ShowGuidance)
                    return _savedForeColor;
                return base.ForeColor;
            }
            set
            {
                if (_mode == Mode.ShowGuidance)
                    _savedForeColor = value;
                else
                    base.ForeColor = value;
            }
        }

        /// <summary>
        /// Subtly changes the text property to deal with the guidance
        /// mode of this textbox
        /// </summary>
        [Category("Appearance"), DefaultValue(""),
         Description("The text associated with the control")]
        public new string Text
        {
            get
            {
                if (_mode == Mode.ShowGuidance)
                    return "";
                return base.Text;
            }
            set { base.Text = value; }
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The colour of the guidance text - the text which is shown in
        /// the textbox
        /// </summary>
        [Category("Appearance"),
         Description("The colour to use to display the guidance text")]
        public Color GuidanceColor
        {
            get { return _guidanceColor; }
            set
            {
                _guidanceColor = value;
                if (_mode == Mode.ShowGuidance)
                    base.ForeColor = value;
            }
        }

        /// <summary>
        /// The text to display as guidance when the text box is left empty.
        /// </summary>
        [Category("Appearance"), DefaultValue(""),
         Description("The guidance text to use when the textbox is empty")]
        public string GuidanceText
        {
            get { return _guidanceText; }
            set
            {
                _guidanceText = value;
                if (_mode == Mode.ShowGuidance)
                    base.Text = value;
            }
        }

        /// <summary>
        /// The current mode that the text box is in
        /// </summary>
        protected Mode CurrentMode
        {
            get { return _mode; }
            set
            {
                if (value == _mode)
                    return;
                _mode = value;
                if (value == Mode.Normal)
                {
                    if (base.Text == _guidanceText)
                        base.Text = "";
                    base.ForeColor = _savedForeColor;
                }
                else
                {
                    base.Text = _guidanceText;
                    _savedForeColor = base.ForeColor;
                    base.ForeColor = _guidanceColor;
                }
            }
        }

        #endregion

        #region - ShouldSerialize/Reset Methods -

        /// <summary>
        /// Resets the forecolor of this textbox to the default value
        /// </summary>
        private new void ResetForeColor()
        {
            ForeColor = DefaultForeColor;
        }

        /// <summary>
        /// Checks if the forecolor of this textbox should be serialized by the
        /// forms designer or not
        /// </summary>
        /// <returns>True if the forecolor of this textbox does not match the
        /// default</returns>
        private bool ShouldSerializeForeColor()
        {
            return (ForeColor != DefaultForeColor);
        }

        /// <summary>
        /// Checks if the guidance color of this textbox should be serialized by the
        /// forms designer or not
        /// </summary>
        /// <returns>True if the guidance color of this textbox does not match the
        /// default</returns>
        private bool ShouldSerializeGuidanceColor()
        {
            return (GuidanceColor != SystemColors.GrayText);
        }

        /// <summary>
        /// Resets the guidance colour of this textbox to the default value
        /// </summary>
        private void ResetGuidanceColor()
        {
            GuidanceColor = SystemColors.GrayText;
        }

        #endregion

        #region - Event overrides -

        /// <summary>
        /// Handles this textbox losing focus by checking the current
        /// <see cref="Text"/> value to see if it's empty and showing
        /// the guidance (and setting guidance mode) if that is the case.
        /// </summary>
        protected override void OnLostFocus(EventArgs e)
        {
            if (base.Text.Trim() == "")
                CurrentMode = Mode.ShowGuidance;
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Handles this textbox getting the focus by clearing the text
        /// and restoring the normal forecolor if the box is currently
        /// in 'ShowGuidance' mode.
        /// This will select the text if it is currently flagged to do so
        /// </summary>
        protected override void OnGotFocus(EventArgs e)
        {
            if (_mode == Mode.ShowGuidance)
                CurrentMode = Mode.Normal;

            base.OnGotFocus(e);

            if (_selectOnFocus)
                SelectAll();
            // Assume we don't want to select on focus (this flag is set after
            // the box is validated, implying that we are in the same window)
            _selectOnFocus = false;
        }

        /// <summary>
        /// Handles this textbox being validated, implying that it is losing
        /// focus to another control within the same window (rather than
        /// losing focus due to another window being activated).
        /// In such a case we set the box to select all text when it is
        /// next focused (so that tabbing between fields automatically
        /// selects all the text)
        /// </summary>
        protected override void OnValidated(EventArgs e)
        {
            _selectOnFocus = true;
            base.OnValidated(e);
        }

        /// <summary>
        /// Handles this textbox having its text changed, either through user
        /// interaction or programmatically. If the change is by user interaction,
        /// this method does nothing beyond the usual actions when a textbox's
        /// text is changed. If programmatically set, this will set the mode to
        /// guidance if an empty string is set as the text, and set it to normal if
        /// a non-empty string which doesn't match the guidance text is set.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            // we deal with mode changes separately if user activated
            if (Focused)
                return;

            if (CurrentMode == Mode.Normal)
            {
                if (base.Text.Trim() == "")
                    CurrentMode = Mode.ShowGuidance;
            }
            else // ie. CurrentMode == Mode.ShowGuidance
            {
                string txt = base.Text.Trim();
                if (txt != _guidanceText && txt != "")
                    CurrentMode = Mode.Normal;
            }
        }

        #endregion

    }
}

