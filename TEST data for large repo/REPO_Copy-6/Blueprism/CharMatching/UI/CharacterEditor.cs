using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using BluePrism.Server.Domain.Models;
using BluePrism.BPCoreLib.Collections;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Character editor control which contains a character list and a 'shifter'
    /// control with which selected characters can be edited.
    /// </summary>
    public partial class CharacterEditor : UserControl
    {
        #region - Events -

        /// <summary>
        /// Event fired when the selection state of any characters in the list held
        /// in this editor has changed. Note that any multiple-selection events,
        /// for instance a char being selected causing a different char to be
        /// unselected, will result in a single event.
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new character editor
        /// </summary>
        public CharacterEditor()
        {
            InitializeComponent();
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The union of font and free characters to be set into this control with a
        /// view to being edited.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FontCharsUnion CharUnion
        {
            get { return charList.CharUnion; }
            set { charList.CharUnion = value; }
        }

        /// <summary>
        /// Gets the selected chars from the character list held in this editor.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<CharData> SelectedChars
        {
            get { return charList.SelectedCharacters; }
        }

        /// <summary>
        /// Indicates whether the character shifter control should be visible on this
        /// control or not; Default is true
        /// </summary>
        [Browsable(true), Category("Behaviour"), DefaultValue(true),
         Description("Shows or hides the character shifter panel")]
        public bool CharacterShifterVisible
        {
            get { return shifter.Visible; }
            set { shifter.Visible = value; }
        }

        /// <summary>
        /// Sets this editor to show only the highlighted characters
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(false),
         Description("Sets the editor to display highlighted chars only")]
        public bool ShowHighlightedOnly
        {
            get { return charList.ShowHighlightedOnly; }
            set { charList.ShowHighlightedOnly = value; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Raises the <see cref="SelectionChanged"/> event
        /// </summary>
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            EventHandler handler = SelectionChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Handles the selection changing in the character list panel, chaining the
        /// event to any other interested parties.
        /// </summary>
        private void HandleSelectionChanged(object sender, EventArgs e)
        {
            OnSelectionChanged(e);
        }

        /// <summary>
        /// Handles a shift operation being requested on the selected characters
        /// </summary>
        private void HandleShiftOperation(object sender, ShiftOperationEventArgs e)
        {
            Action<CharData> action = null;
            switch (e.Operation)
            {
                case ShiftOperation.PadTop:
                case ShiftOperation.PadRight:
                case ShiftOperation.PadBottom:
                case ShiftOperation.PadLeft:
                    action = delegate(CharData cd) { cd.Pad(e.Direction, 1); };
                    break;

                case ShiftOperation.ShiftUp:
                case ShiftOperation.ShiftRight:
                case ShiftOperation.ShiftDown:
                case ShiftOperation.ShiftLeft:
                    action = delegate(CharData cd) { cd.Shift(e.Direction, 1); };
                    break;

                case ShiftOperation.TrimTop:
                case ShiftOperation.TrimRight:
                case ShiftOperation.TrimBottom:
                case ShiftOperation.TrimLeft:
                    action = delegate(CharData cd) { cd.Strip(e.Direction, 1); };
                    break;

                default: // unrecognised shift operation - do nothing
                    return;
            }

            ICollection<CharData> selected = charList.SelectedCharacters;
            if (selected.Count == 0)
                return;

            foreach (CharData cd in selected)
            {
                try
                {
                    action(cd);
                }
                catch (LimitReachedException) { } // Ignore any overruns
            }
            // We now need to check if the characters exist in the font, and
            // update the list accordingly.
            charList.RebuildCharacterPairs();

            // Finally, we want to make sure that the previously selected chars
            // are selected again.
            // We need to ensure that only the glyph is taken into account when
            // selecting the appropriate chars, since the assigned char may have
            // changed in the char list if a glyph was found in the font, or if
            // a previously found glyph was no longer there after the shift operation
            charList.SelectedCharacters = new clsSet<CharData>(
                CharData.CharlessEqualityComparer, selected);
        }

        /// <summary>
        /// Clears the list of characters in the embedded character list
        /// </summary>
        public void Clear()
        {
            charList.Clear();
        }

        /// <summary>
        /// Merges the chars in the current union into the font set within the union.
        /// </summary>
        public void MergeCharsIntoFont()
        {
            charList.MergeIntoFont();
        }

        /// <summary>
        /// Deletes the seelcted chars in this character editor, both from the font
        /// and from any free chars.
        /// </summary>
        public void DeleteSelectedChars()
        {
            charList.DeleteSelected();
        }


        #endregion

    }
}
