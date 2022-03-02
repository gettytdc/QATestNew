using System;
using System.Diagnostics;
using System.Windows.Forms;
namespace BluePrism.Setup.Dialogs
{
    /// <summary>
    /// The standard Licence dialog
    /// </summary>
    public partial class LicenceDialog : BaseDialog
    {
        private Helpers _helpers;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenceDialog"/> class.
        /// </summary>
        public LicenceDialog()
        {
            if (_helpers == null) _helpers = new Helpers(Shell);
            InitializeComponent();
        }

        void LicenceDialog_Load(object sender, EventArgs e)
        {
            //resize the subtitle to accommodate the longest subtitle whilst maintaining look and feel
            _helpers.Shell = Shell;
            _helpers.ApplySubtitleAppearance(Subtitle);

            Agreement.Rtf = Runtime.Session.GetResourceString("WixSharp_LicenceFile");
            Accepted.Checked = Runtime.Session["LastLicenceAcceptedChecked"] == "True";
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (_helpers.IsPatchUpgrade(Runtime.ProductVersion))
            {
                int index = Shell.Dialogs.IndexOf(typeof(ProgressDialog));
                if (index != -1)
                    Shell.GoTo(index);
            }
            else
            {
                Shell.GoNext();
            }
        }

        void Accepted_CheckedChanged(object sender, EventArgs e)
        {
            NextButton.Enabled = Accepted.Checked;
            Runtime.Session["LastLicenceAcceptedChecked"] = Accepted.Checked.ToString();
        }

        private void Agreement_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }
    }
}