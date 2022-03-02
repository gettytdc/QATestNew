using System;
using System.Windows.Forms;
using WixSharp;

namespace BluePrism.Setup.Dialogs
{
    /// <summary>
    /// The standard InstallDir dialog
    /// </summary>
    public partial class InstallDirDialog : BaseDialog
    {

        private Helpers _helpers;
        private string installDirProperty;
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallDirDialog"/> class.
        /// </summary>
        public InstallDirDialog()
        {
            InitializeComponent();
            var helpers = new Helpers(Shell);
            NextButton.Image = helpers.GetUacShield(16);
            ChangeDirLabel.Left = 621 - (ChangeDirLabel.Width / 2);
        }

        void InstallDirDialog_Load(object sender, EventArgs e)
        {
            //resize the subtitle to accommodate the longest subtitle whilst maintaining look and feel
            _helpers = new Helpers(Shell);
            _helpers.ApplySubtitleAppearance(Subtitle);

            installDirProperty = Runtime.Session.Property("WixSharp_UI_INSTALLDIR");

            string installDirPropertyValue = Runtime.Session.Property(installDirProperty);

            if (installDirPropertyValue.IsEmpty())
            {
                //We are executed before any of the MSI actions are invoked so the INSTALLDIR (if set to absolute path)
                //is not resolved yet. So we need to do it manually
                InstallDir.Text = Runtime.Session.GetDirectoryPath(installDirProperty);

                if (InstallDir.Text == "ABSOLUTEPATH")
                    InstallDir.Text = Runtime.Session.Property("INSTALLDIR_ABSOLUTEPATH");
            }
            else
            {
                //INSTALLDIR set either from the command line or by one of the early setup events (e.g. UILoaded)
                InstallDir.Text = installDirPropertyValue;
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (!installDirProperty.IsEmpty())
                Runtime.Session[installDirProperty] = InstallDir.Text;

            if (!AdvancedInstall.Checked)
            {
                int index = Shell.Dialogs.IndexOf(typeof(ProgressDialog));
                if (index != -1)
                    Shell.GoTo(index);
                return;
            }
            Shell.GoNext();
        }

        private void ChangeDirLabel_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { SelectedPath = InstallDir.Text })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    InstallDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void AdvancedInstall_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                NextButton.Text = Properties.Resources.Next;
                NextButton.Image = null;
            }
            else
            {
                var helpers = new Helpers(Shell);
                NextButton.Text = Properties.Resources.Install;
                NextButton.Image = helpers.GetUacShield(16);
            }
        }
    }
}