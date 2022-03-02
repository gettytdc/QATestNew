using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Diagnostics;
using System.Drawing;
using WixSharp;
using WixSharp.CommonTasks;

namespace BluePrism.Setup.Dialogs
{
    /// <summary>
    /// The standard Installation Progress dialog
    /// </summary>
    public partial class ProgressDialog : BaseDialog, IProgressDialog
    {

        private enum ProgressMode
        {
            Installing,
            UnInstalling,
            Fixing
        }

        private const string PortalUrl = "https://portal.blueprism.com/products";
        private Helpers _helpers;
        private ProgressMode _mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialog"/> class.
        /// </summary>
        public ProgressDialog()
        {
            HideExitButton();
            HideBackButton();
            InitializeComponent();
        }

        void ProgressDialog_Load(object sender, EventArgs e)
        {
            //resize the subtitle to accommodate the longest subtitle whilst maintaining look and feel
            _helpers = new Helpers(Shell);
            _helpers.ApplySubtitleAppearance(Subtitle);

            if (_helpers.IsPatchUpgrade(Runtime.ProductVersion))
            {
                Title.Text = Properties.Resources.UpgradeProgressTitle;
                Subtitle.Text = Properties.Resources.UpgradeProgressSubtitle;
            }
            WaitPrompt.Location = new Point(400 - (WaitPrompt.Width / 2), WaitPrompt.Location.Y);

            UACRevealer.Enter();
            Shell.MessageDialog = new MessageDialog(this);
            Shell.StartExecute();
            UACRevealer.Exit();
        }

        /// <summary>
        /// Called when Shell is changed. It is a good place to initialize the dialog to reflect the MSI session
        /// (e.g. localize the view).
        /// </summary>
        protected override void OnShellChanged()
        {
            if (Runtime.Session.IsUninstalling())
            {
                _mode = ProgressMode.UnInstalling;
                Title.Text = Properties.Resources.OnUnInstallingTitle;
                Subtitle.Text = Properties.Resources.OnUnInstallingSubTitle;
                Subtitle.Multiline = true;
                PrismImage.Image = Properties.Resources.sad_prism;
                PrismImage.Size = new Size(243, 176);
                PrismImage.Location = new Point(277, 360);
                OpenPortalButton.Visible = true;
            }
            else if (Runtime.Session.IsRepairing())
            {
                _mode = ProgressMode.Fixing;
                Title.Text = Properties.Resources.OnRepairingTitle;
                Subtitle.Text = Properties.Resources.OnRepairingSubTitle;
                PrismImage.Image = Properties.Resources.fixing_prism;
                PrismImage.Size = new Size(269, 146);
                PrismImage.Location = new Point(263, 370);
                OpenPortalButton.Visible = false;
            }
            else if (Runtime.Session.IsInstalling())
            {
                _mode = ProgressMode.Installing;
                Title.Text = Properties.Resources.OnInstallingTitle;
                Subtitle.Text = Properties.Resources.OnInstallingSubTitle;
                PrismImage.Image = Properties.Resources.installing_prism;
                PrismImage.Size = new Size(265, 147);
                PrismImage.Location = new Point(263, 370);
                OpenPortalButton.Visible = false;
                WaitButton.Visible = false;
            }

            this.Localize();
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="messageRecord">The message record.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="defaultButton">The default button.</param>
        /// <returns></returns>
        public override MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton)
        {
            switch (messageType)
            {
                case InstallMessage.InstallStart:
                case InstallMessage.InstallEnd:
                    {
                        WaitButton.Visible = false;
                    }
                    break;

                case InstallMessage.ActionStart:
                    {
                        try
                        {
                            //messageRecord[0] - is reserved for FormatString value
                            var messageId = $"ProgressText{messageRecord[1]}";
                            var message = Properties.Resources.ResourceManager.GetString(messageId);

                            if (message.IsNotEmpty())
                                CurrentAction.Text = message;
                        }
                        catch
                        {
                            // Do nothing
                        }
                    }
                    break;
            }
            return MessageResult.OK;
        }

        /// <summary>
        /// Called when MSI execution progress is changed.
        /// </summary>
        /// <param name="progressPercentage">The progress percentage.</param>
        public override void OnProgress(int progressPercentage)
        {
            ProgressBar.Value = progressPercentage;

            if (progressPercentage > 0)
            {
                WaitButton.Visible = false;
            }
        }

        /// <summary>
        /// Called when MSI execution is complete.
        /// </summary>
        public override void OnExecuteComplete()
        {
            CurrentAction.Text = Properties.Resources.ProgressTextComplete;
            if (Shell.UserInterrupted || _helpers.ShellLogContainsCancellationMessage || Shell.ErrorDetected)
            {
                int index = Shell.Dialogs.IndexOf(typeof(ErrorDialog));
                if (index != -1)
                    Shell.GoTo(index);
                return;
            }

            if (_mode != ProgressMode.UnInstalling)
                Shell.GoNext();
            else
            {
                WaitButton.Text = Properties.Resources.Finish;


                WaitButton.Click += WaitButton_Click;
                WaitButton.Visible = true;
                WaitButton.Enabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the cancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void WaitButton_Click(object sender, EventArgs e)
        {
            Shell.Exit();
        }

        private void OpenPortalButton_Click(object sender, EventArgs e)
        {
            Process.Start(PortalUrl);
        }
    }
}