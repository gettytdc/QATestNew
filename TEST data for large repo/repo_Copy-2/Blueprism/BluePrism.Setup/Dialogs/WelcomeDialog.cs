using BluePrism.Setup.Forms;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WixSharp;

namespace BluePrism.Setup.Dialogs
{
    /// <summary>
    /// The standard Welcome dialog
    /// </summary>
    public partial class WelcomeDialog : BaseDialog
    {
        private Helpers _helpers;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeDialog"/> class.
        /// </summary>
        public WelcomeDialog()
        {
            if (_helpers == null) _helpers = new Helpers(Shell);
            HideBackButton();
            InitializeComponent();
        }

        #region "Event Handlers"
        private void NextButton_Click(object sender, EventArgs e)
        {
            Shell.GoNext();
        }
        private void UpdateButton_Click(object sender, EventArgs e)
        {
            Shell.GoNext();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ChangeLanguage();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ChangeLanguage();
        }

        private void WelcomeDialog_Load(object sender, EventArgs e)
        {
            //resize the subtitle to accommodate the longest subtitle whilst maintaining look and feel
            _helpers.Shell = Shell;

            _helpers.ApplySubtitleAppearance(Subtitle);

            UpdateIfPatching();
        }

        private void UpdateIfPatching()
        {
            if (_helpers.IsPatchUpgrade(Runtime.ProductVersion))
            {
                var installDirProperty = Runtime.Session.Property("WixSharp_UI_INSTALLDIR");
                var installedDirectory = _helpers.GetInstalledDirectory();
                if (!string.IsNullOrEmpty(installedDirectory))
                {
                    Runtime.Session[installDirProperty] = installedDirectory;
                }

                Title.Text = Properties.Resources.WelcomeReadyToUpdate;
                Subtitle.Location = new System.Drawing.Point(75, 163);
                Subtitle.Multiline = true;
                Subtitle.Height = 64;
                Subtitle.Text = Properties.Resources.WelcomeClickUpdateToContinue;
                UpdateButton.Visible = true;
                NextButton.Visible = false;
            }
        }

        #endregion

        #region "Methods"
        private void ChangeLanguage()
        {
            var pseudoLocalization = bool.Parse(Runtime.Session["PSEUDOLOCALIZATION"]);
            using (var lcf = new SelectLanguageForm(pseudoLocalization))
            {
                lcf.StartPosition = FormStartPosition.CenterParent;
                lcf.ShowDialog(owner: this);

                if (lcf.NewLocale != null &&
                    lcf.DialogResult == DialogResult.OK)
                {
                    _helpers.ChangeLocale(lcf.NewLocale);
                    Runtime.Session["LOCALE"] = lcf.NewLocale;
                }
            }

            Controls.Remove(BorderPanel);
            BorderPanel.Dispose();

            base.ReInitialize();
            HideBackButton();
            InitializeComponent();
            UpdateIfPatching();

        }
        #endregion

        private void WelcomeDialog_Shown(object sender, EventArgs e)
        {
            _helpers.ForceFocusToForm(this);
        }

    }
}