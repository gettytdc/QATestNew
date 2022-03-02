using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WixSharp;

namespace BluePrism.Setup.Dialogs
{
    public partial class MessageDialog : BaseDialog, IManagedDialog
    {
        private ProgressDialog parentDialog;

        private IList<Button> _navigationButtons = new List<Button>();

        public MessageDialog(ProgressDialog progressDialog)
        {
            HideBackButton();
            HideExitButton();
            InitializeComponent();

            _navigationButtons.Add(ignoreButton);
            _navigationButtons.Add(retryButton);
            _navigationButtons.Add(abortButton);
            _navigationButtons.Add(cancelButton);
            _navigationButtons.Add(okButton);
            _navigationButtons.Add(noButton);
            _navigationButtons.Add(yesButton);

            this.parentDialog = progressDialog;
        }

        private void UpdateButtons(MessageButtons buttons)
        {
            foreach (var b in _navigationButtons)
            {
                BorderPanel.Controls.Remove(b);
                b.Visible = false;
            }

            switch (buttons)
            {
                case MessageButtons.AbortRetryIgnore:
                    ignoreButton.Visible = true;
                    retryButton.Visible = true;
                    abortButton.Visible = true;
                    break;
                case MessageButtons.OK:
                    okButton.Visible = true;
                    break;
                case MessageButtons.OKCancel:
                    cancelButton.Visible = true;
                    okButton.Visible = true;
                    break;
                case MessageButtons.RetryCancel:
                    retryButton.Visible = true;
                    cancelButton.Visible = true;
                    break;
                case MessageButtons.YesNo:
                    noButton.Visible = true;
                    yesButton.Visible = true;
                    break;
                case MessageButtons.YesNoCancel:
                    cancelButton.Visible = true;
                    noButton.Visible = true;
                    yesButton.Visible = true;
                    break;
            }

            // re-position the buttons from right to left
            var buttonLocation = new Point(534, 640);
            
            foreach (var b in _navigationButtons.Where(x => x.Visible))
            {
                const int ButtonMargin = 16;
                int nextButtonXCoordinate;

                BorderPanel.Controls.Add(b);

                if (b.Name == "ignoreButton")
                {
                    const int IgnoreButtonXCoordinate = 484;
                    const int IgnoreButtonWidthDifference = 50;

                    buttonLocation.X = IgnoreButtonXCoordinate;
                    nextButtonXCoordinate = buttonLocation.X - (b.Width - IgnoreButtonWidthDifference + ButtonMargin);
                }
                else
                {
                    nextButtonXCoordinate = buttonLocation.X - (b.Width + ButtonMargin);
                }

                b.Location = buttonLocation;
                buttonLocation.X = nextButtonXCoordinate;
            }
        }

        private void SetDialogText(InstallMessage messageType, Record messageRecord)
        {
            string message = messageRecord.ToString();
            ignoreButton.Text = Properties.Resources.MessageDialogIgnoreButtonText;
            if (message.Contains("ChromeIsRunning"))
            {
                Subtitle.Text = "Chrome";
                WarningText.Text = Properties.Resources.ChromeIsRunning;
            }
            else if (message.Contains("FirefoxIsRunning"))
            {
                Subtitle.Text = "Firefox";
                WarningText.Text = Properties.Resources.FirefoxIsRunning;
            }
            else if (message.Contains("EdgeIsRunning"))
            {
                Subtitle.Text = "Edge";
                WarningText.Text = Properties.Resources.EdgeIsRunning;
            }
            else if (message.Contains("NewerVersionInstalled"))
            {
                Subtitle.Text = Properties.Resources.MessageTypeError;
                WarningText.Text = Properties.Resources.NewerVersionInstalled;
            }
            else if (message.Contains("ThisProductRequiresDotNet"))
            {
                Subtitle.Text = Properties.Resources.MessageTypeError;
                WarningText.Text = Properties.Resources.ThisProductRequiresDotNet;
            }
            else if (message.Contains("Invalid Drive"))
            {
                Subtitle.Text = Properties.Resources.MessageTypeError;
                string driveName = message.Substring(message.IndexOf(':') + 1);
                WarningText.Text = string.Format(Properties.Resources.InvalidDrive, driveName);
            }
            else
            {
                ignoreButton.Text = Properties.Resources.MessageDialogIgnore;
                switch (messageType)
                {
                    case InstallMessage.Error:
                        Subtitle.Text = Properties.Resources.MessageTypeError;
                        break;
                    case InstallMessage.Warning:
                        Subtitle.Text = Properties.Resources.MessageTypeWarning;
                        break;
                    case InstallMessage.User:
                        Subtitle.Text = Properties.Resources.MessageTypeUser;
                        break;
                    default:
                        Subtitle.Text = messageType.ToString();
                        break;
                }
                WarningText.Text = message;
            }

        }

        public IManagedUIShell Shell { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void OnExecuteComplete()
        {
            throw new NotImplementedException();
        }

        public void OnExecuteStarted()
        {
            throw new NotImplementedException();
        }

        public void OnProgress(int progressPercentage)
        {
            throw new NotImplementedException();
        }

        public MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton)
        {
            SetDialogText(messageType, messageRecord);
            UpdateButtons(buttons);
            return (MessageResult)ShowDialog(owner: parentDialog);
        }
    }
}
