using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using AutomateControls;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// A user control which represents a character pair - ie. a char value along
    /// with the glyph that represents it in a particular font.
    /// </summary>
    internal partial class CharacterPair : UserControl
    {
        #region - Class-scope declarations -

        /// <summary>
        /// The default highlight color for character pairs - typically used for
        /// characters which exist outside of a set of characters
        /// </summary>
        public static readonly Color PrimaryHighlight = Color.AliceBlue;

        /// <summary>
        /// The secondray highlight color for character pairs - typically used for
        /// characters which exist in two sets of characters
        /// </summary>
        public static readonly Color SecondaryHighlight = Color.LemonChiffon;

        /// <summary>
        /// The padding to apply to a textbox when dynamically calculating its
        /// preferred size from its current contents
        /// </summary>
        private const int TextBoxPadding = 8;

        #endregion

        #region - Events -

        /// <summary>
        /// Event indicating that the CharData model that this control represents
        /// has changed.
        /// </summary>
        public event CharDataEventHandler CharDataChanged;

        /// <summary>
        /// Event indicating that the character in this control has changed.
        /// </summary>
        public event TextValueChangedEventHandler TextValueChanged;

        /// <summary>
        /// Event indicating that the character in this control has been selected
        /// </summary>
        public event EventHandler CharSelected;

        /// <summary>
        /// Event indicating that the character in this control has been de-selected
        /// </summary>
        public event EventHandler CharDeSelected;

        #endregion

        #region - Member Variables -

        // The model being represented in this control
        private CharData _chr;

        // The bitmap representing the character
        private Bitmap _bmp;

        // Flag indicating if this pair is highlighted
        private bool _highlighted;

        // The back colour to use for highlighted pairs
        private Color _highlightColor = PrimaryHighlight;

        // Flag indicating if this pair is currently selected
        private bool _selected;

        // The detector of joined chars in all of its char pairs (incl. this one)
        private IJoinedCharDetector _detector;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty character pair
        /// </summary>
        public CharacterPair() : this(null, false) { }

        /// <summary>
        /// Creates a new character pair control for the given char data.
        /// </summary>
        /// <param name="c">The data representing the model that the control should
        /// draw from. Null creates an empty disassociated character pair control.
        /// </param>
        public CharacterPair(CharData c) : this(c, false) { }

        /// <summary>
        /// Creates a new character pair control for the given char data,
        /// highlighted as appropriate
        /// </summary>
        /// <param name="c">The data representing the model that the control should
        /// draw from. Null creates an empty disassociated character pair control.
        /// </param>
        /// <param name="highlighted">True to set this pair is highlighted - this
        /// means that the background colour used will be the 'HighlightedBackColor'
        /// rather than the default</param>
        public CharacterPair(CharData c, bool highlighted)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
            if (c != null)
                Character = c;
            Highlighted = highlighted;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The character data object being represented by this control
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CharData Character
        {
            get { return _chr; }
            set
            {
                if (_chr != value)
                {
                    if (_bmp != null)
                    {
                        _bmp.Dispose();
                        _bmp = null;
                    }
                    if (_chr != null)
                    {
                        _chr.CharChanged -= HandleModelCharChanged;
                        _chr.MaskChanged -= HandleCharMaskChanged;
                    }
                }
                
                _chr = value;
                if (_chr == null)
                {
                    txtLetter.Text = "";
                }
                else
                {
                    _bmp = _chr.ToBitmap(4);
                    TextValue = _chr.Value;
                    _chr.CharChanged += HandleModelCharChanged;
                    _chr.MaskChanged += HandleCharMaskChanged;
                }
                UpdateSize();
            }
        }

        /// <summary>
        /// Checks if this character pair is highlighted or not
        /// </summary>
        [Category("Appearance"),
         DefaultValue(false)]
        public bool Highlighted
        {
            get { return _highlighted; }
            set
            {
                if (_highlighted != value)
                {
                    _highlighted = value;
                    // Change the background colour if we're not selected.
                    if (!_selected)
                    {
                        if (value)
                        {
                            BackColor = HighlightColor;
                        }
                        else
                        {
                            BackColor = SystemColors.Control;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The backcolor to use for highlighted character pairs.
        /// </summary>
        [Browsable(true), Category("Appearance"),
         DefaultValue(typeof(Color), "AliceBlue"),
         Description("The colour to use for this CharacterPair if it is highlighted")]
        public Color HighlightColor
        {
            get { return _highlightColor; }
            set {
                if (_highlightColor != value)
                {
                    _highlightColor = value;
                    if (Highlighted && !_selected)
                    {
                        BackColor = _highlightColor;
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether this control should auto-blur (ie. shift focus) when a
        /// single char is entered for it or not.
        /// </summary>
        [Browsable(false)]
        private bool AutoBlur
        {
            get
            {
                return (_detector != null && !_detector.JoinedCharactersDetected);
            }
        }

        /// <summary>
        /// The text value represented by this character pair, or
        /// <see cref="CharData.NullValue"/> if no text value is set in it
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TextValue
        {
            get
            {
                string txt = txtLetter.Text.Trim();
                return (txt == "" ? CharData.NullValue : txt);
            }
            set
            {
                string oldValue = TextValue;
                if (value != oldValue)
                {
                    txtLetter.Text = (value == CharData.NullValue ? "" : value);
                    if (_chr != null)
                        _chr.Value = TextValue; // use prop to trim it consistently
                    UpdateSize();
                    OnTextValueChanged(new TextValueChangedEventArgs(oldValue, value));
                }
            }
        }

        /// <summary>
        /// Flag indicating if this char pair control has an image registered within
        /// it or not.
        /// </summary>
        [Browsable(false)]
        private bool HasImage
        {
            get { return (_bmp != null && _bmp.Size != Size.Empty); }
        }

        /// <summary>
        /// Gets the rectangle into which the image will be drawn. An empty rectangle
        /// if there is no image to draw
        /// </summary>
        [Browsable(false)]
        private Rectangle ImageRectangle
        {
            get
            {
                if (!HasImage)
                    return Rectangle.Empty;

                // We want to centre a smaller bitmap horizontally within the total
                // width of this control
                Padding m = txtLetter.Margin;
                return new Rectangle(new Point(
                    (Width - _bmp.Width) / 2, m.Vertical + txtLetter.Height),
                    _bmp.Size);
            }
        }

        /// <summary>
        /// Flag indicating whether this character pair is selected or not.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    if (value)
                        OnSelected(EventArgs.Empty);
                    else
                        OnDeSelected(EventArgs.Empty);
                }
            }
        }

        #endregion

        #region - Internal Event Methods -

        /// <summary>
        /// Handles the parent of this control changing, ensuring that the
        /// <see cref="IJoinedCharDetector"/> in the ancestry path is recorded in
        /// this control.
        /// </summary>
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            _detector = UIUtil.GetAncestor<IJoinedCharDetector>(this);
        }

        /// <summary>
        /// Handles the painting of this control, ensuring that the image is drawn
        /// when the rest of the control is drawn.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle r = ClientRectangle;
            r.Width--;
            r.Height--;
            e.Graphics.DrawRectangle(Pens.LightGray, r);
            if (HasImage)
                e.Graphics.DrawImage(_bmp, ImageRectangle);
        }

        /// <summary>
        /// Raises the CharChanged event indicating that the character has changed
        /// within this control. The default behaviour is to update the model and
        /// then propogate the event to registered listeners.
        /// </summary>
        protected virtual void OnTextValueChanged(TextValueChangedEventArgs e)
        {
            TextValueChangedEventHandler h = TextValueChanged;
            if (h != null)
                h(this, e);
        }

        /// <summary>
        /// Raises the CharDataChanged event with the given args.
        /// </summary>
        protected virtual void OnCharDataChanged(CharDataEventArgs e)
        {
            TextValue = e.NewValue;
            CharDataEventHandler h = CharDataChanged;
            if (h != null)
                h(this, e);
        }

        /// <summary>
        /// Raises the <see cref="DeSelected"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDeSelected(EventArgs e)
        {
            // Setting the memvar may be redundant if coming from the property,
            // but if called from a subclass it would be expected to work thus
            _selected = false;

            if (Highlighted)
                BackColor = HighlightColor;
            else
                BackColor = DefaultBackColor;

            EventHandler handler = CharDeSelected;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="Selected"/> event.
        /// </summary>
        protected virtual void OnSelected(EventArgs e)
        {
            // Setting the memvar may be redundant if coming from the property,
            // but if called from a subclass it would be expected to work thus
            _selected = true;

            BackColor = SystemColors.Highlight;
            EventHandler handler = CharSelected;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region - Event Listeners -

        /// <summary>
        /// Handle the mast of the char data changing, ensuring that the view is kept
        /// up to date with the new glyph.
        /// </summary>
        void HandleCharMaskChanged(object sender, EventArgs e)
        {
            _bmp = _chr.ToBitmap(4);
            this.Size = GetPreferredSize(Size.Empty);
            Invalidate(true);
        }

        /// <summary>
        /// Handles the character changing on the contained CharData object.
        /// </summary>
        /// <param name="sender">The CharData object being changed</param>
        /// <param name="e">The args detailing the change</param>
        void HandleModelCharChanged(object sender, TextValueChangedEventArgs e)
        {
            // Propogate the model changing to interested parties outside this class
            OnCharDataChanged(new CharDataEventArgs((CharData)sender, e.OldValue));
        }

        /// <summary>
        /// Handles the text being validated - this updates the model directly and
        /// raises the <see cref="TextValueChanged"/> event in this control if the
        /// value set by the user differs from that set in the model.
        /// </summary>
        void HandleTextValidated(object sender, EventArgs e)
        {
            string oldVal = _chr.Value;
            string newVal = TextValue;
            if (oldVal != newVal)
            {
                _chr.Value = newVal;
                OnTextValueChanged(new TextValueChangedEventArgs(oldVal, newVal));
            }
        }

        /// <summary>
        /// Handles the text being changed - this moves onto the next available
        /// control, which should be the next character (unless it's the last
        /// character being displayed).
        /// </summary>
        void HandleTextChanged(object sender, EventArgs e)
        {
            UpdateSize();
            // If the text changes while we have focus, and there's only 1 char
            // in there and we don't have to deal with joined chars, auto-focus
            // onto the next control (typically the next CharPair in a list).
            if (txtLetter.Focused && txtLetter.Text.Length == 1 && AutoBlur)
            {
                Control top = TopLevelControl;
                Debug.Assert(top != null);
                if (top != null)
                    top.SelectNextControl(this, true, true, true, true);
            }
        }

        /// <summary>
        /// Ensures that the text is selected when the text box gains focus.
        /// </summary>
        void HandleLetterEntered(object sender, EventArgs e)
        {
            txtLetter.SelectAll();
            Selected = true;
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Resets the highlight colour of this control to the default.
        /// </summary>
        public void ResetHighlightColor()
        {
            HighlightColor = PrimaryHighlight;
        }

        /// <summary>
        /// Gets the preferred size of this character pair. It depends on whether a
        /// bitmap is available to display to determine what the preferred size is.
        /// </summary>
        /// <param name="proposedSize">The proposed size for this control - actually
        /// ignored for the purposes of this method.</param>
        /// <returns>The preferred size of this control in its current state.
        /// </returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            // We use the margin of the textbox as the margin for everything
            Padding txtMargin = txtLetter.Margin;

            Size txtSize = txtLetter.Size;
            // Pad out the text size with its margin
            txtSize.Width += txtMargin.Horizontal;
            txtSize.Height += txtMargin.Vertical;

            // If no bitmap, that's enough.
            if (!HasImage)
                return txtSize;

            // If there's a bitmap, we want to cover that, plus one more
            // margin (txtLetter's bottom margin) to provide a little
            // space between it and the edge of the component.
            // And the horizontal margin to ensure padding on left and right sides.
            Size bmpSize = _bmp.Size;
            bmpSize.Height += txtMargin.Bottom;
            bmpSize.Width += txtMargin.Horizontal;

            // So now we want a width big enough for whichever of the textbox or
            // picture is wider, and a height big enough to encompass both the
            // textbox and the picture.
            return new Size(
                Math.Max(txtSize.Width, bmpSize.Width),
                txtSize.Height + bmpSize.Height
            );
        }

        /// <summary>
        /// Updates the preferred (ie. actual) size of this control, taking into
        /// account the length of the string in the textbox and the size of the
        /// glyph being displayed.
        /// </summary>
        private void UpdateSize()
        {
            Size textSize = TextRenderer.MeasureText(txtLetter.Text, txtLetter.Font);
            int currWidth = txtLetter.Width;
            txtLetter.Width = textSize.Width + TextBoxPadding;
            txtLetter.Left = (ClientSize.Width - txtLetter.Width) / 2;
            this.Size = GetPreferredSize(Size.Empty);
            Invalidate();
        }

        #endregion
    }

}
