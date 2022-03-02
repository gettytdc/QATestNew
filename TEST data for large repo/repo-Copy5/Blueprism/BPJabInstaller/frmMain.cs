using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security.Principal;

namespace BPJabInstaller
{
    public partial class frmMain : Form
    {

        private clsInstaller mInstaller;

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            txtOutput.Text = string.Empty;
            mInstaller = new clsInstaller();
            mInstaller.Status += AddStatusLine;
            mInstaller.Install(new Version(cmbVersion.Text), true);
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            txtOutput.Text = string.Empty;
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                AddStatusLine("You are not an administrator.");
                AddStatusLine("Only an administrator can perform this installation.");
            }
            else
            {
                mInstaller = new clsInstaller();
                mInstaller.Status += AddStatusLine;
                mInstaller.Uninstall(true);
            }
        }

        ///--------------------------------------------------------------------------
        /// <summary>
        /// Adds a message to the status output log on the form.
        /// </summary>
        /// <param name="msg">The message to be output.</param>
        ///--------------------------------------------------------------------------
        private void AddStatusLine(string msg)
        {
            // Check if we can safely call GUI-manipulation methods from this
            // thread - if not, invoke on the thread which owns the text box
            if (txtOutput.InvokeRequired)
                txtOutput.BeginInvoke(new SetTextCallback(AddStatusLine), msg);
            else
                txtOutput.AppendText(msg + "\r\n");
        }
        private delegate void SetTextCallback(string msg);

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            List<Version> versions = clsInstaller.GetVersions();
            foreach (Version version in versions)
                cmbVersion.Items.Add(version.ToString());
            cmbVersion.SelectedIndex = cmbVersion.Items.Count-1;
        }


    }
}