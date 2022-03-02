using System;
using WixSharp;

namespace BluePrism.Setup.Dialogs
{
    public partial class VersionCheckDialog : BaseDialog, IManagedDialog
    {
        public VersionCheckDialog()
        {
            HideBackButton();
            InitializeComponent();
            this.WarningText.Text = string.Empty;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Shell.GoNext();
        }

        private void VersionCheckDialog_Shown(object sender, EventArgs e)
        {
            var helpers = new Helpers(Shell);
            // If we do not have an installed version skip this screen as its a fresh install
            // Also skip it if the versions are compatible
            if (!helpers.IsPatchUpgrade(Runtime.ProductVersion) || Helpers.SkipApiWarning)
            {
                Shell.GoNext();
            }
            else
            {
                this.Title.Text = Properties.Resources.VersionWarning;
                this.WarningText.Text = Properties.Resources.VersionWarningMessage;
            }
        }
    }
}
