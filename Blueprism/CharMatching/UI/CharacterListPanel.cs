using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BluePrism.BPCoreLib.Collections;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// User control which hosts characters in a left to right scrolling list.
    /// </summary>
    public partial class CharacterListPanel : UserControl, IJoinedCharDetector
    {
        #region - Published Events -

        /// <summary>
        /// Event fired whenever the selection of the character pairs within this
        /// panel has changed. Note that this is done en masse - ie. if a pair is
        /// selected and another pair is deselected as a result, only one event will
        /// be fired.
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region - Member Variables -

        // The union of font characters and free characters represented in this panel
        private FontCharsUnion _union;

        // Flag indicating if only highlighted chars should be shown
        private bool _showHighlightedOnly;

        // Flag indicating if multiple selections are allowed
        private bool _multiSelect;

        // Flag indicating that the pairs are being selected within this class
        private bool _settingSelected;

        // Flag indicating if joined chars were found in the CharPairs in this list
        private bool _joinedCharsDetected;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty CharacterList control
        /// </summary>
        public CharacterListPanel()
        {
            InitializeComponent();
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Indicates whether multiple characters are allowed to be selected in this
        /// control. Default is false
        /// </summary>
        [Browsable(true), Category("Behaviour"), DefaultValue(false),
         Description("Allows or disallows the selecting of multiple characters")]
        public bool MultiSelect
        {
            get { return _multiSelect; }
            set { _multiSelect = value; }
        }

        /// <summary>
        /// Indicates whether only highlighted characters are displayed in this
        /// list control; Default is false.
        /// </summary>
        [Browsable(true), Category("Behaviour"), DefaultValue(false),
         Description("True to show only highlighted characters in the list; " +
             "False to show all characters")]
        public bool ShowHighlightedOnly
        {
            get { return _showHighlightedOnly; }
            set
            {
                if (_showHighlightedOnly != value)
                {
                    _showHighlightedOnly = value;
                    flowChars.SuspendLayout();
                    try
                    {
                        foreach (CharacterPair cp in Pairs)
                        {
                            cp.Visible = (!value || cp.Highlighted);
                        }
                    }
                    finally
                    {
                        flowChars.ResumeLayout();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the union of characters and fonts in this panel
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FontCharsUnion CharUnion
        {
            get { return _union; }
            set
            {
                _union = value;
                RebuildCharacterPairs();
            }
        }

        /// <summary>
        /// Gets the collection CharacterPair controls handled by this list
        /// </summary>
        [Browsable(false)]
        ControlCollection Pairs
        {
            get { return flowChars.Controls; }
        }

        /// <summary>
        /// The currently selected character pairs in this character list.
        /// </summary>
        [Browsable(false)]
        ICollection<CharacterPair> SelectedPairs
        {
            get
            {
                List<CharacterPair> pairs = new List<CharacterPair>();
                foreach (CharacterPair cp in Pairs)
                {
                    if (cp.Selected)
                        pairs.Add(cp);
                }
                return pairs;
            }
        }

        /// <summary>
        /// Gets whether any of the CharacterPairs in this list panel are selected
        /// or not.
        /// </summary>
        [Browsable(false)]
        bool AnySelected
        {
            get
            {
                foreach (CharacterPair cp in Pairs)
                    if (cp.Selected)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the currently selected characters in this character list. An
        /// empty collection indicates that none are selected.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<CharData> SelectedCharacters
        {
            get
            {
                List<CharData> chars = new List<CharData>();
                foreach (CharacterPair cp in SelectedPairs)
                    chars.Add(cp.Character);
                return chars;
            }
            set
            {
                if (value == null) // sanity check
                    value = GetEmpty.ICollection<CharData>();

                // Indicate that we're setting selected chars - we keep track of
                // selections to deal with multiple selections. This ensures that
                // the method dealing with that knows what we're doing.
                _settingSelected = true;
                try
                {
                    foreach (CharacterPair cp in Pairs)
                        cp.Selected = (value.Contains(cp.Character));
                }
                finally
                {
                    _settingSelected = false;
                }
                OnSelectionChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// The currently active character pair in this list - ie. the character
        /// pair which has keyboard focus
        /// </summary>
        [Browsable(false)]
        CharacterPair ActivePair
        {
            get { return ActiveControl as CharacterPair; }
        }

        /// <summary>
        /// Gets the count of characters represented on this panel
        /// </summary>
        [Browsable(false)]
        public int CharacterCount
        {
            get { return Pairs.Count; }
        }

        /// <summary>
        /// Gets or sets whether joined characters have been detected in this list
        /// panel. Note that once joined characters have been detected once, they
        /// remain so until the characters are cleared using <see cref="ClearPairs"/>
        /// </summary>
        [Browsable(false)]
        bool IJoinedCharDetector.JoinedCharactersDetected
        {
            // We allow joined chars 
            get { return _joinedCharsDetected; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Adds an unhighlighted character pair to this panel.
        /// This also ensures that the pair is tied to the selection event handler,
        /// and that the pair is rendered invisible if this panel is set to show only
        /// highlighted characters.
        /// </summary>
        /// <param name="chr">The char data for which a character pair control is
        /// required to be added.</param>
        /// <returns>The CharacterPair control representing the given CharData.
        /// </returns>
        CharacterPair AddCharPair(CharData chr)
        {
            return AddCharPair(chr, null);
        }

        /// <summary>
        /// Adds a character pair to this panel, ensuring that it is highlighted if
        /// a colour is given to highlight it with.
        /// This also ensures that the pair is tied to the selection event handler,
        /// and that the pair is rendered invisible if it is not highlighted and this
        /// panel is set to show only highlighted characters.
        /// </summary>
        /// <param name="chr">The char data for which a character pair control is
        /// required to be added.</param>
        /// <param name="col">The colour to highlight the character pair control with
        /// or null if it should not be highlighted.</param>
        /// <returns>The CharacterPair control representing the given CharData.
        /// </returns>
        CharacterPair AddCharPair(CharData chr, Color? col)
        {
            // Create a character pair, if we have a colour, ensure it is highlighted
            // and the given colour is set as the highlight colour
            CharacterPair cp = new CharacterPair(chr, col.HasValue);
            if (col.HasValue)
                cp.HighlightColor = col.Value;
            _joinedCharsDetected = (_joinedCharsDetected || chr.Value.Length > 1);

            // Add it to the flow panel and ensure the event is handled for it
            Pairs.Add(cp);
            cp.CharSelected += HandlePairSelected;
            cp.CharDeSelected += HandlePairDeselected;
            cp.TextValueChanged += HandlePairValueChanged;

            // Set invisible if we're only showing highlighted and the pair is
            // not actually highlighted
            if (_showHighlightedOnly)
                cp.Visible = cp.Highlighted;

            return cp;
        }

        /// <summary>
        /// Rebuilds the character pairs held in this panel using the currently
        /// set font/chars union.
        /// </summary>
        internal void RebuildCharacterPairs()
        {
            flowChars.SuspendLayout();
            try
            {
                ClearPairs();
                // If there is no character collection, nothing else to do
                if (_union == null)
                    return;

                // Add in order, namely
                // 1) chars only in region
                // 2) chars in region and font
                // 3) chars only in font
                // 1 & 2 are 'highlighted' with the primary and secondary
                //    highlight colours respectively. 3 is not highlighted.
                foreach (CharData c in _union.CharsInFreeOnly)
                    AddCharPair(c, CharacterPair.PrimaryHighlight);

                foreach (CharData c in _union.CharsInFontAndFree)
                    AddCharPair(c, CharacterPair.SecondaryHighlight);

                foreach (CharData c in _union.CharsInFontOnly)
                    AddCharPair(c);
            }
            finally
            {
                flowChars.ResumeLayout();
            }

        }

        /// <summary>
        /// Merges the free characters held in the union represented by this panel
        /// into the font in the same union and updates this panel with the new
        /// values.
        /// </summary>
        /// <exception cref="NoSuchElementException">If no <see cref="CharUnion"/>
        /// is set in this panel and thus there are no characters to merge into
        /// no font.</exception>
        public void MergeIntoFont()
        {
            if (_union == null)
                throw new NoSuchElementException(
                    Resources.NoCharUnionSetCannotMergeCharacters);
            _union.MergeFreeCharsIntoFont();
            RebuildCharacterPairs();
        }

        /// <summary>
        /// Deletes the selected characters in this list panel.
        /// </summary>
        public void DeleteSelected()
        {
            if (!AnySelected) // nothing to do
                return;

            _union.RemoveChars(SelectedCharacters);
            foreach (CharacterPair cp in SelectedPairs)
            {
                Pairs.Remove(cp);
            }
            // There were selections before there are none now...
            OnSelectionChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Handles a CharacterPair control being deselected
        /// </summary>
        void HandlePairDeselected(object sender, EventArgs e)
        {
            if (!_settingSelected)
            {
                OnSelectionChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles a CharacterPair control being selected.
        /// </summary>
        void HandlePairSelected(object sender, EventArgs e)
        {
            // If this panel is not currently setting selected pairs
            if (!_settingSelected)
            {
                // ...and multi-select is enabled and Ctrl was not pressed
                // when a pair was selected
                if (_multiSelect && (Control.ModifierKeys & Keys.Control) == 0)
                {
                    // Deselect all except the active CharPair
                    CharacterPair active = ActivePair;
                    // Make sure we don't get a whole load of selection changed
                    // events out of doing this
                    _settingSelected = true;
                    foreach (CharacterPair cp in SelectedPairs)
                    {
                        if (cp != active)
                            cp.Selected = false;
                    }
                    _settingSelected = false;
                }
                OnSelectionChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the text value of a CharacterPair changing. This ensures that any
        /// values with more than one char are detected, changing the way that the
        /// auto-focusing works if they are.
        /// </summary>
        void HandlePairValueChanged(object sender, TextValueChangedEventArgs e)
        {
            _joinedCharsDetected = (_joinedCharsDetected || e.NewValue.Length > 1);
        }

        /// <summary>
        /// Raises the <see cref="SelectionChanged"/> event
        /// </summary>
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            EventHandler handler = this.SelectionChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Gets the maximum size described by the given sizes.
        /// </summary>
        /// <param name="sz1">The first size to check</param>
        /// <param name="sz2">The second size to check</param>
        /// <returns>The size containing the largest width and height of the two
        /// given sizes.</returns>
        private Size Max(Size sz1, Size sz2)
        {
            return new Size(
                sz1.Width > sz2.Width ? sz1.Width : sz2.Width,
                sz1.Height > sz2.Height ? sz1.Height : sz2.Height
            );
        }

        /// <summary>
        /// Gets the preferred size of this panel
        /// </summary>
        /// <param name="proposedSize">The proposed constraints for this control.
        /// </param>
        /// <returns>The preferred size of this character list</returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            if (CharacterCount == 0)
                return base.GetPreferredSize(proposedSize);

            // Get the largest size of any of the character pairs.
            Size sz = Size.Empty;
            // default the margin to 3, but use the CharacterPairs to be certain
            Padding margin = new Padding(3);
            foreach (CharacterPair cp in Pairs)
            {
                sz = Max(sz, cp.GetPreferredSize(Size.Empty));
                margin = cp.Margin;
            }
            
            // Now the size we want has the largest width (including the default
            // margin of a character pair) multiplied by the number of
            // characters, and the largest height plus space for a scrollbar and
            // the 'Characters' label.
            sz.Width = (sz.Width + margin.Horizontal) * Pairs.Count;
            sz.Height += SystemInformation.HorizontalScrollBarHeight;

            return sz;
        }

        /// <summary>
        /// Clears the characters in this list control
        /// </summary>
        public virtual void Clear()
        {
            _union = null;
            ClearPairs();
        }

        /// <summary>
        /// Clears the character pair controls from this panel. It does not alter
        /// the model held in this panel (ie. the <see cref="CharUnion"/>)
        /// </summary>
        void ClearPairs()
        {
            Pairs.Clear();
            _joinedCharsDetected = false;
        }

        #endregion

    }
}
