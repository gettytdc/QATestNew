using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;
using BluePrism.BPCoreLib.Collections;
using AutomateControls;
using AutomateControls.Forms;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Control to handle font generation.
    /// </summary>
    public partial class FontGenerator : UserControl
    {
        #region - Class Scope Declarations -

        /// <summary>
        /// Class to encapsulate the payload given to the worker thread in order to
        /// generate the BP font representing the system font
        /// </summary>
        private class WorkerPayload
        {
            // The font to generate from
            private Font _font;

            // The name of the BP font to generate
            private string _name;

            /// <summary>
            /// Creates a new worker payload object with the given font and
            /// destination font name.
            /// </summary>
            /// <param name="font">The font to generate from</param>
            /// <param name="name">The BP font name to generate</param>
            public WorkerPayload(Font font, string name)
            {
                _font = font;
                _name = name;
            }

            /// <summary>
            /// The system font to generate a BP font from
            /// </summary>
            public Font Font { get { return _font; } }

            /// <summary>
            /// The name of the BP font to generate
            /// </summary>
            public string Name { get { return _name; } }
        }

        #endregion

        #region - Member Variables -

        // The collection of installed fonts to check against
        private InstalledFontCollection _fonts;

        // The font store to use to read and write fonts from and to the system.
        private IFontStore _store;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new font generator control
        /// </summary>
        public FontGenerator()
        {
            InitializeComponent();
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The font store used to read and write Blue Prism fonts.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFontStore Store
        {
            get { return _store; }
            set
            {
                _store = value;
                btnGenerate.Enabled = (cmbFont.Items.Count > 0);
            }
        }

        /// <summary>
        /// The collection of fonts installed on this system
        /// </summary>
        private InstalledFontCollection FontCollection
        {
            get
            {
                if (_fonts == null)
                    _fonts = new InstalledFontCollection();
                return _fonts;
            }
        }

        /// <summary>
        /// The currently selected font with the currently selected attributes
        /// </summary>
        private Font SelectedFont
        {
            get { return new Font(SelectedFamily, SelectedSize, SelectedStyle); }
        }

        /// <summary>
        /// The currently selected font family.
        /// </summary>
        private FontFamily SelectedFamily
        {
            set { cmbFont.SelectedItem = value; }
            get { return cmbFont.SelectedItem as FontFamily; }
        }

        /// <summary>
        /// The currently selected font style(s)
        /// </summary>
        private FontStyle SelectedStyle
        {
            set
            {
                cbBold.Checked = (value & FontStyle.Bold) != 0;
                cbItal.Checked = (value & FontStyle.Italic) != 0;
                cbUline.Checked = (value & FontStyle.Underline) != 0;
                cbStr.Checked = (value & FontStyle.Strikeout) != 0;
            }
            get
            {
                FontStyle style = FontStyle.Regular;
                if (Bold) style |= FontStyle.Bold;
                if (Italic) style |= FontStyle.Italic;
                if (Uline) style |= FontStyle.Underline;
                if (Strike) style |= FontStyle.Strikeout;
                return style;
            }
        }

        /// <summary>
        /// A collection of font styles which represent the collected styles in this
        /// control. <see cref="FontStyle.Regular"/> is implied on at all times.
        /// </summary>
        private ICollection<string> SelectedStyles
        {
            get
            {
                var styles = new List<string>();
                if (Bold) styles.Add(cbBold.Text);
                if (Italic) styles.Add(cbItal.Text);
                if (Uline) styles.Add(cbUline.Text);
                if (Strike) styles.Add(cbStr.Text);
                return styles;
            }
        }

        /// <summary>
        /// Flag indicating if the Bold style is currently selected
        /// </summary>
        private bool Bold { get { return (cbBold.Enabled && cbBold.Checked); } }

        /// <summary>
        /// Flag indicating if the Italic style is currently selected
        /// </summary>
        private bool Italic { get { return (cbItal.Enabled && cbItal.Checked); } }

        /// <summary>
        /// Flag indicating if the Underline style is currently selected
        /// </summary>
        private bool Uline { get { return (cbUline.Enabled && cbUline.Checked); } }

        /// <summary>
        /// Flag indicating if the Strikeout style is currently selected
        /// </summary>
        private bool Strike { get { return (cbStr.Enabled && cbStr.Checked); } }

        /// <summary>
        /// The currently selected font size.
        /// </summary>
        private float SelectedSize
        {
            set { spinnerSize.Value = (decimal)value; }
            get { return (float)spinnerSize.Value; }
        }

        /// <summary>
        /// The suggested font name for the currently selected font
        /// </summary>
        private string SuggestedFontName
        {
            get
            {
                StringBuilder sb = new StringBuilder(SelectedFamily.Name);
                sb.Append(' ').Append(SelectedSize);
                ICollection<string> styles = SelectedStyles;
                if (styles.Count > 0)
                {
                    sb.Append(" (")
                        .Append(CollectionUtil.Join(styles, ", ")).Append(')');
                }
                return sb.ToString();
            }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed;
        /// otherwise, false.</param>
        protected override void Dispose(bool explicitly)
        {
            if (explicitly)
            {
                if (components != null)
                    components.Dispose();

                if (_fonts != null)
                {
                    cmbFont.Items.Clear(); // get rid of the font families in the cmb
                    _fonts.Dispose();
                    _fonts = null;
                }
            }
            base.Dispose(explicitly);
        }

        /// <summary>
        /// Loads the fonts into the fonts drop down in this control
        /// </summary>
        internal void LoadFonts()
        {
            cmbFont.BeginUpdate();
            try
            {
                cmbFont.Items.Clear();
                foreach (FontFamily fam in FontCollection.Families)
                {
                    cmbFont.Items.Add(fam);
                }
            }
            finally
            {
                cmbFont.EndUpdate();
            }
            if (cmbFont.Items.Count > 0)
            {
                cmbFont.SelectedIndex = 0;
                btnGenerate.Enabled = (Store != null);
            }
        }

        /// <summary>
        /// Updates the availability of the style checkboxes according to the
        /// currently selected font family
        /// </summary>
        private void UpdateStyleAvailability()
        {
            FontFamily fam = SelectedFamily;
            cbBold.Enabled = fam.IsStyleAvailable(FontStyle.Bold);
            cbItal.Enabled = fam.IsStyleAvailable(FontStyle.Italic);
            cbUline.Enabled = fam.IsStyleAvailable(FontStyle.Underline);
            cbStr.Enabled = fam.IsStyleAvailable(FontStyle.Strikeout);
        }

        /// <summary>
        /// Handles a font family being selected in the font family drop down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleFontFamilySelected(object sender, EventArgs e)
        {
            UpdateStyleAvailability();
            UpdateGeneratedFontName();
        }

        /// <summary>
        /// Updates the font name with the currently selected font details
        /// </summary>
        private void UpdateGeneratedFontName()
        {
            txtGeneratedName.Text = SuggestedFontName;
        }

        /// <summary>
        /// Handles the Generate button being pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        private void HandleGenerate(object sender, EventArgs ea)
        {
            // Check that we have a valid environment
            if (!FontConfig.ValidateEnvironment(TopLevelControl))
                return; // it handles any required user prompts

            Font f = SelectedFont;
            string name = txtGeneratedName.Text;
            ICollection<string> currentFonts = _store.AvailableFontNames;
            while (FontExists(name, currentFonts))
            {
                name = TextEnterBox.Show(null, Resources.NameAlreadyInUse,
                    string.Format(Resources.TheName0IsAlreadyInUsePleaseChooseAnother, name),
                    name, false, true
                );
                if (name == null) // ie. user cancelled
                    return;
            }
            txtGeneratedName.Text = name;
            panInput.Enabled = false;
            panWork.Enabled = true;
            worker.RunWorkerAsync(new WorkerPayload(f, name));
        }

        /// <summary>
        /// Check if font already exists
        /// </summary>
        private Boolean FontExists(string name, ICollection<string> fontNames)
        {
            foreach (string font in fontNames)
            {
                if (name.Equals(font, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the font size control being changed
        /// </summary>
        private void HandleFontSizeChanged(object sender, EventArgs e)
        {
            UpdateGeneratedFontName();
        }

        /// <summary>
        /// Handles the font style control being changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleStyleChanged(object sender, EventArgs e)
        {
            UpdateGeneratedFontName();
        }

        /// <summary>
        /// Handles the work of extracting the font from the payload given it
        /// </summary>
        /// <param name="e">The args for this background work - this should contain
        /// an <see cref="DoWorkEventArgs.Argument">Argument</see> value of a
        /// <see cref="WorkerPayload"/> object, initialised with the font to extract
        /// from and the target BP Font's name.</param>
        private void HandleWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            WorkerPayload payload = e.Argument as WorkerPayload;

            e.Result = SystemFontExtractor.ExtractAndSave(
                payload.Font, payload.Name, Store,
                new BackgroundWorkerProgressMonitor(worker), false
            );

            // Whatever happened, we're done with this now...
            payload.Font.Dispose();
        }

        /// <summary>
        /// Handles a progress change update from the font extraction.
        /// </summary>
        /// <param name="e">The event args detailing the progress change. The
        /// <see cref="ProgressChangedEventArgs.UserState">UserState</see> value
        /// should be a string which is output to the user in a label</param>
        private void HandleWorkerProgressChanged(
            object sender, ProgressChangedEventArgs e)
        {
            progbar.Value = e.ProgressPercentage;
            lblStatus.Text = e.UserState as string;
        }

        /// <summary>
        /// Handles the extracter background worker completing.
        /// </summary>
        /// <param name="e">The args detailing the completed event. The
        /// <see cref="RunWorkerCompletedEventArgs.Result">Result</see> property is
        /// expected to be a <see cref="BPFont"/> object which was genearted from
        /// the extraction process. If this is null and there is no reported error,
        /// it is assumed that the work was cancelled.</param>
        private void HandleWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Exception ex = e.Error;
            if (ex != null)
            {
                string err =
                    string.Format(Resources.AnErrorOccurredWhileGeneratingTheFont0, ex.Message);
                MessageBox.Show(
                    err, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = err;
                progbar.Value = 0;
            }
            else
            {
                BPFont f = e.Result as BPFont;
                if (f == null)
                {
                    lblStatus.Text = Resources.FontGenerationAborted;
                }
                else
                {
                    lblStatus.Text =
                        string.Format(Resources.BluePrismFont0GeneratedAndSaved, f.Name);
                    progbar.Value = 100;
                }
            }
            panWork.Enabled = false;
            panInput.Enabled = true;
        }

        /// <summary>
        /// Handles the Abort button being pressed.
        /// </summary>
        private void HandleAbort(object sender, EventArgs e)
        {
            worker.CancelAsync();
        }

        /// <summary>
        /// Handles the validating of the font family - it is modifiable so that you
        /// don't have to find it in the list if you know what it is called.
        /// </summary>
        private void HandleFontFamilyValidating(object sender, CancelEventArgs e)
        {
            // Double check that what's been entered is a valid font name,
            // because the user can enter their own values (in order to allow
            // family name suggestions in the combo box).
            FontFamily fam = SelectedFamily;
            if (fam != null) // We have a family, that's fine.
                return;

            // If there are no fonts, do nothing - the generate button isn't
            // actualy enabled in this case anyway, so no harm, no foul
            if (cmbFont.Items.Count == 0)
                return;

            // Otherwise, get the text
            string famName = cmbFont.Text;
            if (famName == "")
            {
                MessageBox.Show(Resources.PleaseSelectAValidFont, Resources.NoFontSelected,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }
            foreach (FontFamily ff in FontCollection.Families)
            {
                if (ff.Name.Equals(famName, StringComparison.CurrentCultureIgnoreCase))
                {
                    cmbFont.SelectedItem = ff;
                    return;
                }
            }
            // Otherwise, font family not found...
            MessageBox.Show(null, string.Format(Resources.TheFont0WasNotRecognised, famName),
                Resources.InvalidFontName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            e.Cancel = true;
        }

        #endregion

    }
}
