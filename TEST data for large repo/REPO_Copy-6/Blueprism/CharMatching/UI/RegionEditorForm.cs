using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using BluePrism.BPCoreLib.Collections;
using AutomateControls.Forms;
using LocaleTools;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Form which brings together a region editor and a corresponding character list
    /// where the characters extracted from a region can be merged into the font,
    /// and where a font can be identified as a system font.
    /// </summary>
    public partial class RegionEditorForm : AutomateForm
    {
        #region - Member variables / events -

        /// <summary>
        /// Event fired when the regions are applied by the user - ie. the Apply
        /// button is pressed on the form.
        /// </summary>
        public event EventHandler RegionsApplied;

        // True if the character list has an update pending - ie. if the font
        // value or region changed while the characters were not being displayed
        private bool _charListUpdatePending;

        // The currently selected font
        private BPFont _font;

        // A cache of fonts against their names
        private IDictionary<string, BPFont> _fontCache =
            new Dictionary<string, BPFont>();

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new region editor form
        /// </summary>
        public RegionEditorForm()
        {
            InitializeComponent();
            btnMerge.Enabled = false;
            chkShowAll.Enabled = false;
            splitPane.Panel2Collapsed = true;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Flag indicating if the characters are currently visible or not
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CharactersVisible { get; set; }

        /// <summary>
        /// The font value set in this editor form - this is largely a convenience
        /// to ensure that all related controls are updated when the font value
        /// is changed.
        /// </summary>
        private BPFont FontValue
        {
            get { return _font; }
            set
            {
                if (_font == value)
                    return;
                _font = value;
                if (_font == null)
                {
                    lblFontName.Text = Resources.None;
                    lblFontCount.Text = "0";
                    chkShowAll.Enabled = false;
                    btnMerge.Enabled = false;
                }
                else
                {
                    lblFontName.Text = _font.Name;
                    lblFontCount.Text = _font.CharacterData.Count.ToString();
                    chkShowAll.Enabled = true;
                    btnMerge.Enabled = CharactersVisible;
                }
            }
        }

        #endregion

        #region - Event Handler Methods -

        /// <summary>
        /// Handles the layout of a spy region changing
        /// </summary>
        void HandleRegionLayoutChanged(object sender, SpyRegionEventArgs e)
        {
            SpyRegion reg = e.Region;

            ICollection<CharData> newChars = CharData.Extract(reg.Image);
            ICollection<CharData> currChars = e.Region.Chars;
            if (currChars == null)
            {
                // This region has no chars, so just use those generated now.
                e.Region.Chars = newChars;
            }
            else
            {
                // This region has chars - make sure that any whose masks match
                // those extracted from the region image are saved across from
                // the region (with their assigned char in place). Discard any
                // which are no longer in the region and add any new ones.
                List<CharData> chars = new List<CharData>();

                CharMap map = new CharMap(currChars);
                foreach (CharData cd in newChars)
                {
                    CharData existing;
                    if (map.TryGetValue(cd.Mask, out existing))
                    {
                        chars.Add(existing);
                    }
                    else
                    {
                        chars.Add(cd);
                    }
                }
                reg.Chars = chars;
            }

            HandleRegionSelected(sender, e);
        }

        /// <summary>
        /// Handles the char data changing on a spy region - only has any effect
        /// on this form if it is the currently selected region
        /// </summary>
        void HandleRegionCharDataChanged(object sender, SpyRegionEventArgs e)
        {
            SpyRegion reg = e.Region;

            // we only care about the currently selected region
            if (reg == null || reg != regMapper.SelectedRegion)
                return;

            // Check the font - that's the only thing we're interested in
            string currFont = null;
            if (FontValue != null)
                currFont = FontValue.Name;
            if (currFont != reg.FontName)
            {
                BPFont f;
                if (!_fontCache.TryGetValue(reg.FontName, out f))
                {
                    // Aha! A change. Let's do this thing.
                    _fontCache[reg.FontName] = (f = reg.Font);
                }
                FontValue = f;
                UpdateCharList(f, reg);
            }
        }

        /// <summary>
        /// Handles a spy region (or no spy region) being selected
        /// </summary>
        void HandleRegionSelected(object sender, SpyRegionEventArgs e)
        {
            FontValue = (e.Region == null ? null : e.Region.Font);
            UpdateCharList(FontValue, e.Region);
        }

        /// <summary>
        /// Handles the 'Show All' checkbox being changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleShowAllChanged(object sender, EventArgs e)
        {
            charList.ShowHighlightedOnly = !chkShowAll.Checked;
        }

        /// <summary>
        /// Handles the merging of the region characters into the selected font.
        /// </summary>
        private void HandleMergeClick(object sender, EventArgs e)
        {
            BPFont f = FontValue;
            if (f == null)
            {
                // it really shouldn't be - the button should be disabled if there's
                // no font set in this control
                return;
            }
            // Tell the char list to merge the characters into the current font
            charList.MergeIntoFont();
            lblFontName.Text = _font.Name;
            lblFontCount.Text = _font.CharacterData.Count.ToString();

            // Actually save the font data to the database.
            regMapper.Store.SaveFont(f);
        }

        /// <summary>
        /// Handles characters being deleted from the character list.
        /// </summary>
        private void HandleDeleteClick(object sender, EventArgs e)
        {
            // Delete any selected characters
            charList.DeleteSelected();

            // If we have a font, update it within the store.
            BPFont f = FontValue;
            if (f == null)
                return;
            regMapper.Store.SaveFont(f);
        }

        /// <summary>
        /// Handles the character selection changing, enabling or disabling the
        /// 'Delete Chars' button as appropriate.
        /// </summary>
        private void HandleCharSelectionChanged(object sender, EventArgs e)
        {
            btnDeleteChars.Enabled = (charList.SelectedCharacters.Count > 0);
        }

        /// <summary>
        /// Handles the closing of this form by checking and warning if there are
        /// any empty fonts defined in its regions
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // if we're already cancelling for whatever reason, no point in adding
            // more pain onto the user at the moment
            if (e.Cancel)
                return;

            // Check that there are no empty fonts in the regions; if there are, then
            // check if the user wants to continue or cancel
            if (DialogResult == DialogResult.OK && !ContinueAfterEmptyFontsWarning())
                e.Cancel = true;
        }

        /// <summary>
        /// Handles the OK button being clicked.
        /// </summary>
        private void HandleOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the cancel button being clicked.
        /// </summary>
        private void HandleCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Handles the Apply button being clicked.
        /// </summary>
        private void HandleApplyClick(object sender, EventArgs e)
        {
            // If there are empty fonts, warn the user about them before applying
            // the changes (and potentially cancel applying the changes if the user
            // so wishes)
            if (!ContinueAfterEmptyFontsWarning())
                return;
            EventHandler handler = RegionsApplied;
            if (handler != null)
            {
                try
                {
                    handler(this, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this,
                        Resources.AnErrorOccurredWhileApplyingChanges + ex.Message,
                        Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
        }

        /// <summary>
        /// Handles the 'Show/Hide Characters' toggle button being changed.
        /// </summary>
        private void HandleToggleCharList(object sender, EventArgs e)
        {
            CharactersVisible = !CharactersVisible;

            bool showing = CharactersVisible;
            // If we have a char list update pending, get that sorted first
            if (showing && _charListUpdatePending)
                UpdateCharList(FontValue, regMapper.SelectedRegion);

            // Then actually show the character list panel and update the
            // toggle / merge buttons
            splitPane.Panel2Collapsed = !showing;
            cbToggleChars.Text = (showing ? Resources.HideCharacters : Resources.ShowCharacters);
            btnMerge.Enabled = (showing && FontValue != null);
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Updates the character list using the given font and characters found in
        /// the specified region. Note that this does not update any of the other
        /// variables within this form - it is responsible only for the characters
        /// and highlighting status on the character list.
        /// </summary>
        /// <param name="f">The font to display in the list.</param>
        /// <param name="reg">The region whose characters should be displayed in
        /// the list. If this is null, the list is cleared</param>
        void UpdateCharList(BPFont f, SpyRegion reg)
        {
            // If the characters are not visible, queue a pending update and return.
            // When the characters become visible this method is called and the
            // work will actually be done.
            if (!CharactersVisible)
            {
                _charListUpdatePending = true;
                return;
            }

            // Reset the pending flag immediately - we're doing the actual work
            // now so any pending updates are done
            _charListUpdatePending = false;

            // No region, ergo nothing to display - there's no point in displaying
            // the chars from the font in this form - it's for region editing, not
            // for font editing, so the characters have no context with no region.
            if (reg == null)
            {
                charList.Clear();
                return;
            }

            // Extract the chars from the region
            ICollection<CharData> charsInRegion = reg.Chars;
            if (charsInRegion == null)
            {
                charsInRegion = CharData.Extract(reg.Image);
                reg.Chars = charsInRegion;
            }

            // Put them into a union with the chars in the font
            charList.CharUnion = new FontCharsUnion(f, charsInRegion);
        }

        /// <summary>
        /// Checks if any of the regions defined in the mapper refer to empty fonts,
        /// and offer a warning to the user, allowing them to continue anyway.
        /// </summary>
        /// <returns>true if the user chose to continue or there were no empty fonts,
        /// false if the user received the warning and chose to cancel the operation
        /// which was presumably saving the data.</returns>
        private bool ContinueAfterEmptyFontsWarning()
        {
            ICollection<string> emptyFonts = regMapper.EmptyFontReferences;

            // No empty fonts? No warning? Continuez tout droite.
            if (emptyFonts.Count == 0)
                return true;

            string msg = LTools.Format(
                Resources.plural_WarningTheDefinedRegionsReferToCOUNTPluralOne1FontOtherFontsWith,
                "COUNT", emptyFonts.Count,
                "FONTLIST", CollectionUtil.Join(emptyFonts, "; ")
            );

            DialogResult res = MessageBox.Show(msg, Resources.EmptyFontsDetected,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

            return (res == DialogResult.OK);
        }

        #endregion
    }
}
