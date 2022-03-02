namespace BluePrism.Api.Setup.Dialogs
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using global::WixSharp;
    using global::WixSharp.UI.Forms;

    /// <summary>
    /// The standard InstallDir dialog
    /// </summary>
    public partial class InstallDirDialog : ManagedForm  // change ManagedForm->Form if you want to show it in designer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallDirDialog"/> class.
        /// </summary>
        public InstallDirDialog()
        {
            InitializeComponent();
            label1.MakeTransparentOn(banner);
            label2.MakeTransparentOn(banner);
        }

        string installDirProperty;

        void InstallDirDialog_Load(object sender, EventArgs e)
        {
            banner.Image = Runtime.Session.GetResourceBitmap("banner");

            installDirProperty = Runtime.Session.Property("WixSharp_UI_INSTALLDIR");

            string installDirPropertyValue = Runtime.Session.Property(installDirProperty);

            if (installDirPropertyValue.IsEmpty())
            {
                //We are executed before any of the MSI actions are invoked so the INSTALLDIR (if set to absolute path)
                //is not resolved yet. So we need to do it manually
                installDir.Text = Runtime.Session.GetDirectoryPath(installDirProperty);

                if (installDir.Text == "ABSOLUTEPATH")
                    installDir.Text = Runtime.Session.Property("INSTALLDIR_ABSOLUTEPATH");
            }
            else
            {
                //INSTALLDIR set either from the command line or by one of the early setup events (e.g. UILoaded)
                installDir.Text = installDirPropertyValue;
            }

            ResetLayout();
            CheckValidFormState();
        }

        void ResetLayout()
        {
            // The form controls are properly anchored and will be correctly resized on parent form
            // resizing. However the initial sizing by WinForm runtime doesn't a do good job with DPI
            // other than 96. Thus manual resizing is the only reliable option apart from going WPF.
            float ratio = (float)banner.Image.Width / (float)banner.Image.Height;
            topPanel.Height = (int)(banner.Width / ratio);
            topBorder.Top = topPanel.Height + 1;

            middlePanel.Top = topBorder.Bottom + 10;

            var upShift = (int)(next.Height * 2.3) - bottomPanel.Height;
            bottomPanel.Top -= upShift;
            bottomPanel.Height += upShift;
        }

        void back_Click(object sender, EventArgs e)
        {
            Shell.GoPrev();
        }

        void next_Click(object sender, EventArgs e)
        {
            if (!installDirProperty.IsEmpty())
                Runtime.Session[installDirProperty] = installDir.Text;
            Shell.GoNext();
        }

        void cancel_Click(object sender, EventArgs e)
        {
            Shell.Cancel();
        }

        void change_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { SelectedPath = installDir.Text })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    installDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void installDir_TextChanged(object sender, EventArgs e) =>
            CheckValidFormState();

        private void CheckValidFormState()
        {
            var hasValidRootedPath = Path.IsPathRooted(installDir.Text);
            if (!hasValidRootedPath)
            {
                next.Enabled = false;
                return;
            }

            var pathArray = installDir.Text.ToCharArray();
            next.Enabled = Path.GetInvalidPathChars().Any(x => !pathArray.Contains(x));
        }
    }
}
