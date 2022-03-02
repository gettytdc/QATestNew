using BluePrism.CharMatching.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AutomateControls.Forms;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Form to enable the user to discover an installed font which matches a region
    /// or image
    /// </summary>
    public partial class FontIdentifierForm : AutomateForm
    {
        /// <summary>
        /// The label to display when attempting to match a font manually - ie. when
        /// the embedded <see cref="FontBrowserPanel"/> is being used.
        /// </summary>
        private string ManualText =
            Resources.YouCanSearchManuallyForFontsWhichMatchTheSelectedRegionSelectARegionAndEnterThe;

        /// <summary>
        /// The label to display when attempting to match a font automatically - ie.
        /// when a <see cref="FontScannerPanel"/> is being used.
        /// </summary>
        private string AutomaticText =
            Resources.YouCanSearchAutomaticallyForFontsWhichMatchTheSelectedRegionSelectARegionAndEnt;

        // The spy region container which provides the regions to use to aid in
        // identifying the system font.
        private ISpyRegionContainer _cont;

        /// <summary>
        /// Creates a new font identifier form
        /// </summary>
        public FontIdentifierForm()
        {
            InitializeComponent();
            lblHelp.Text = AutomaticText;
        }

        /// <summary>
        /// Handles a tab page being selected, ensuring the label text is updated
        /// appropriately
        /// </summary>
        private void HandleTabControlSelected(object sender, TabControlEventArgs e)
        {
            lblHelp.Text = (e.TabPage == tabAutomatic ? AutomaticText : ManualText);
        }

        /// <summary>
        /// The currently selected region on this form
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SpyRegion SelectedRegion
        {
            get { return cmbRegions.SelectedItem as SpyRegion; }
            set { cmbRegions.SelectedItem = value; }
        }

        /// <summary>
        /// The spy region contained which contains the regions / bitmaps which are
        /// to be used to identify the system font in use
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISpyRegionContainer SpyRegionContainer
        {
            get { return _cont; }
            set
            {
                _cont = value;
                cmbRegions.BeginUpdate();
                try
                {
                    cmbRegions.Items.Clear();
                    foreach (SpyRegion reg in _cont.SpyRegions)
                        cmbRegions.Items.Add(reg);
                    SpyRegion sel = _cont.SelectedRegion;
                    if (sel != null)
                        cmbRegions.SelectedItem = sel;
                }
                finally
                {
                    cmbRegions.EndUpdate();
                }
            }
        }

        /// <summary>
        /// The currently selected font on whichever identifer panel is currently
        /// being viewed
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Font SelectedFont
        {
            get
            {
                return (tabs.SelectedTab == tabAutomatic
                    ? autoIdentifier.SelectedFont : manualIdentifier.SelectedFont);
            }
        }

        /// <summary>
        /// Handles the OK button being pressed
        /// </summary>
        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the Cancel button being pressed
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Handles a region being selected in the region drop down.
        /// </summary>
        private void cmbRegions_SelectedIndexChanged(object sender, EventArgs e)
        {
            SpyRegion reg = cmbRegions.SelectedItem as SpyRegion;
            manualIdentifier.SpyRegion = reg;
            autoIdentifier.SpyRegion = reg;
        }

        /// <summary>
        /// Handles the tab control select by cancelling the event - this handler
        /// is added to the tab control when the embedded panel is doing some work
        /// which can't be navigated away from until it is either completed or
        /// cancelled.
        /// </summary>
        void DisableTabControlSelect(object sender, TabControlCancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Handles the form closing by cancelling the event after informing the
        /// user that the window cannot be closed. This handler is added to the form
        /// when the embedded panel is doing some work which can't be navigated away
        /// from until it is either completed or cancelled.
        /// </summary>
        void DisableFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                MessageBox.Show(this,
                    Resources.CannotCloseWindowWhileWorkIsOngoing, Resources.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles the embedded panel starting some work
        /// </summary>
        private void HandleWorkStarted(object sender, EventArgs e)
        {
            cmbRegions.Enabled = false;
            btnOk.Enabled = false;
            btnCancel.Enabled = false;
            tabs.Selecting += DisableTabControlSelect;
            this.FormClosing += DisableFormClosing;
        }

        /// <summary>
        /// Handles the embedded panel finishing some work.
        /// </summary>
        private void HandleWorkFinished(object sender, EventArgs e)
        {
            cmbRegions.Enabled = true;
            btnOk.Enabled = true;
            btnCancel.Enabled = true;
            tabs.Selecting -= DisableTabControlSelect;
            this.FormClosing -= DisableFormClosing;
        }
    }
}
