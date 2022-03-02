using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AutomateControls;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// The full font editor control allowing all aspects of a BP font to be edited
    /// </summary>
    public partial class FontEditor : UserControl
    {
        #region - Published Events -

        /// <summary>
        /// Event indicating that a spy operation has been requested
        /// </summary>
        public event SpyRequestEventHandler SpyRequested;

        #endregion

        #region - Member Variables -

        // The font store used by this control
        private IFontStore _store;

        // The font being edited
        private BPFont _font;

        // The height of the top panel in the split pane before being toggled closed
        private int _savedHeight;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new font editor control
        /// </summary>
        public FontEditor()
        {
            InitializeComponent();
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The image being displayed on the region mapper embedded within this font
        /// editor
        /// </summary>
        public Image Image
        {
            get { return regMapper.Image; }
            set { regMapper.Image = value; }
        }

        /// <summary>
        /// The font value being edited by this control
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BPFont FontValue
        {
            get { return _font; }
            set
            {
                if (value == _font)
                    return;

                _font = value;

                if (value == null)
                {
                    txtName.Text = "";
                    txtVer.Text = "";
                    numSpaceWidth.Value = 0;
                    charEditor.Clear();
                    regMapper.ClearRegions();
                }
                else
                {
                    txtName.Text = value.Name;
                    txtVer.Text = value.Version;
                    numSpaceWidth.Value = value.Data.SpaceWidth;
                    charEditor.CharUnion = new FontCharsUnion(value, null);
                }
            }
        }

        /// <summary>
        /// The font store from which fonts can be loaded and to which fonts can
        /// be saved
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFontStore Store
        {
            get { return _store; }
            set { _store = value; }
        }

        /// <summary>
        /// Gets or sets whether the region editor is visible or not.
        /// </summary>
        public bool RegionEditorVisible { get; set; }

        #endregion

        #region - Methods -

        /// <summary>
        /// Handles the region editor visibility being toggled.
        /// </summary>
        private void HandleToggleRegionEditor(object sender, EventArgs e)
        {
            RegionEditorVisible = !RegionEditorVisible;
            bool visible = !splitPanel.Panel1Collapsed;
            bool shouldBeVisible = RegionEditorVisible;
            if (visible == shouldBeVisible)
                return;
            splitPanel.SuspendLayout();
            if (visible)
            {
                _savedHeight = splitPanel.Panel1.Height;
                splitPanel.Panel1Collapsed = true;
                TopLevelControl.Height -= _savedHeight;
            }
            else
            {
                TopLevelControl.Height += Math.Min(_savedHeight, 250);
                splitPanel.Panel1Collapsed = false;
            }
            btnToggleRegionEditor.Text = (visible ? Resources.ShowRegionEditor : Resources.HideRegionEditor);
            splitPanel.ResumeLayout();
        }

        /// <summary>
        /// Handles a region on the region mapper changing
        /// </summary>
        private void HandleRegionChanged(object sender, SpyRegionEventArgs e)
        {
            SpyRegion reg = e.Region;
            Image img = (reg == null ? null : reg.Image);
            btnExtract.Enabled = (img != null);
            if (img != null)
            {
                Color currSel = cmbForeground.SelectedColor;
                cmbForeground.BeginUpdate();
                try
                {
                    cmbForeground.Items.Clear();
                    IDictionary<Color, int> colorMap =
                        ImageUtil.GetColourFrequencies(img);
                    foreach (Color col in colorMap.Keys)
                    {
                        cmbForeground.Items.Add(col);
                    }
                    // Select the last selected colour, or the second most dominant
                    // colour if there was no last selected colour
                    if (currSel != Color.Empty)
                        cmbForeground.SelectedItem = currSel;
                    else if (cmbForeground.Items.Count > 1)
                        cmbForeground.SelectedIndex = 1;
                }
                finally
                {
                    cmbForeground.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Updates the character editor with the combination of the current region
        /// and the font.
        /// </summary>
        private void UpdateCharEditor()
        {
            SpyRegion reg = regMapper.SelectedRegion;

            // Get the characters from the region, if a region is selected
            ICollection<CharData> charsInRegion = null;
            if (reg != null)
            {
                charsInRegion = CharData.Extract(reg.Image,
                    cmbForeground.SelectedColor, cbAutoTrim.Checked);
            }

            // Set the chars in the editor
            charEditor.CharUnion = new FontCharsUnion(_font, charsInRegion);
        }

        /// <summary>
        /// Raises the <see cref="SpyRequested"/> event
        /// </summary>
        protected virtual void OnSpyRequested(SpyRequestEventArgs e)
        {
            SpyRequestEventHandler handler = this.SpyRequested;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Handles the SpyRequested event coming from the region mapper. This just
        /// propogates the event such that any listeners to this editor control can
        /// handle the request.
        /// </summary>
        private void HandleSpyRequested(object sender, SpyRequestEventArgs e)
        {
            OnSpyRequested(e);
        }

        /// <summary>
        /// Handles the Extract button being clicked by extracting the chars from
        /// the region (if there is one selected).
        /// </summary>
        private void HandleExtractClick(object sender, EventArgs e)
        {
            UpdateCharEditor();
        }

        /// <summary>
        /// Handles the merging of characters (with their char values set) into the
        /// font.
        /// </summary>
        private void HandleMergeClick(object sender, EventArgs e)
        {
            charEditor.MergeCharsIntoFont();
            UpdateCharEditor();
        }

        /// <summary>
        /// Handles the 'Delete' of characters from the list (and font, if the chars
        /// are in the font).
        /// </summary>
        private void HandleDeleteClick(object sender, EventArgs e)
        {
            charEditor.DeleteSelectedChars();
        }

        /// <summary>
        /// Updates the font name with the user specified text
        /// </summary>
        private void HandleNameValidated(object sender, EventArgs e)
        {
            _font.Name = txtName.Text;
        }

        /// <summary>
        /// Handles the validation of the font name, ensuring that it is not empty
        /// </summary>
        private void HandleNameTextValidating(object sender, CancelEventArgs e)
        {
            if (txtName.Text.Trim() == "")
            {
                ErrorBox.Show(this, Resources.TheFontNameCannotBeEmpty);
                e.Cancel = true;
            }
            else if (_font.Name != txtName.Text.Trim())
            {
                foreach (string font in _store.AvailableFontNames)
                {
                    if (txtName.Text.Trim().Equals(font, StringComparison.CurrentCultureIgnoreCase))
                    {
                        ErrorBox.Show(this, string.Format(Resources.TheName0IsAlreadyInUsePleaseChooseAnother, font));
                        e.Cancel = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the font with the new version value.
        /// </summary>
        private void HandleVerTextValidated(object sender, EventArgs e)
        {
            _font.Version = txtVer.Text;
        }

        /// <summary>
        /// Updates the font with the new space width value.
        /// </summary>
        private void numSpaceWidth_ValueChanged(object sender, EventArgs e)
        {
            _font.Data.SpaceWidth = (int)numSpaceWidth.Value;
        }

        /// <summary>
        /// Handles the char selection changing in the char editor
        /// </summary>
        private void HandleCharSelectionChanged(object sender, EventArgs e)
        {
            btnDelete.Enabled = (charEditor.SelectedChars.Count > 0);
        }

        #endregion


    }
}
