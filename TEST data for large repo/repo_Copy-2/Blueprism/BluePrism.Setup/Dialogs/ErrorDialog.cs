using System;
using System.Drawing;
using System.Windows.Forms;

namespace BluePrism.Setup.Dialogs
{
    public partial class ErrorDialog : BaseDialog
    {
        public ErrorDialog()
        {
            InitializeComponent();
            base.HideBackButton();
        }

        void Dialog_Load(object sender, EventArgs e)
        {
            //resize the subtitle
            var helpers = new Helpers(Shell);
            helpers.ApplySubtitleAppearance(Subtitle);

            CopyLink.Location = new Point(400 - (CopyLink.Width / 2), CopyLink.Location.Y);
            if (Shell.UserInterrupted || helpers.ShellLogContainsCancellationMessage)
                ErrorsText.Text = string.Concat(Properties.Resources.UserInteruptedInstall, Environment.NewLine);
            ErrorsText.Text += string.Join(Environment.NewLine, Shell.Errors.ToArray());
            ErrorsText.Text += string.Concat(Environment.NewLine, Shell.Log);
            //resolve all Control.Text cases with embedded MSI properties (e.g. 'ProductName') and *.wxl file entries
            base.Localize();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Shell.Exit();
        }

        private void CopyLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(Shell.Log);
            ErrorsText.ContentsCopied = true;
            Invalidate();
        }
    }
}