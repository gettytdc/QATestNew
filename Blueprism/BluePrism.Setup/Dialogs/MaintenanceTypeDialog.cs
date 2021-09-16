using BluePrism.Setup.Forms;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using WixSharp;

namespace BluePrism.Setup.Dialogs
{
    /// <summary>
    /// The standard Maintenance Type dialog
    /// </summary>
    public partial class MaintenanceTypeDialog : BaseDialog
    {
        private Helpers _helpers;
        Type ProgressDialog
        {
            get
            {
                return Shell.Dialogs
                            .Where(d => d.GetInterfaces().Contains(typeof(IProgressDialog)))
                            .FirstOrDefault();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceTypeDialog"/> class.
        /// </summary>
        public MaintenanceTypeDialog()
        {
            if (_helpers == null) _helpers = new Helpers(Shell);
            HideBackButton();
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Shell.Exit();
        }

        private void ChooseLanguagePictureBox_Click(object sender, EventArgs e)
        {
            ChangeLanguage();
        }

        private void ChooseLanguageLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ChangeLanguage();
        }

        void MaintenanceTypeDialog_Load(object sender, System.EventArgs e)
        {
            ResetLayout();
        }

        void ResetLayout()
        {
            ChangeLink.Location = new Point(400 - (ChangeLink.Width / 2), ChangeLink.Location.Y);
            RepairLink.Location = new Point(400 - (RepairLink.Width / 2), RepairLink.Location.Y);
            RemoveLink.Location = new Point(400 - (RemoveLink.Width / 2), RemoveLink.Location.Y);
        }

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
            ResetLayout();
        }

        private void ChangeLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Runtime.Session["MODIFY_ACTION"] = "Change";
            Shell.GoNext();
        }

        private void RemoveLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Runtime.Session["REMOVE"] = "ALL";
            Runtime.Session["MODIFY_ACTION"] = "Remove";

            int index = Shell.Dialogs.IndexOf(ProgressDialog);
            if (index != -1)
                Shell.GoTo(index);
            else
                Shell.GoNext();
        }

        private void RepairLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Runtime.Session["MODIFY_ACTION"] = "Repair";
            int index = Shell.Dialogs.IndexOf(ProgressDialog);
            if (index != -1)
                Shell.GoTo(index);
            else
                Shell.GoNext();
        }

        private void MaintenanceTypeDialog_Shown(object sender, EventArgs e)
        {
            _helpers.ForceFocusToForm(this);
        }
    }
}