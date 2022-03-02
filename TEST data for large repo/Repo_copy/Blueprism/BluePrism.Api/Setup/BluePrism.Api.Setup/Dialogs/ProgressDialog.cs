namespace BluePrism.Api.Setup.Dialogs
{
    using System;
    using System.Drawing;
    using System.Security.Principal;
    using Common;
    using global::WixSharp;
    using global::WixSharp.CommonTasks;
    using Microsoft.Deployment.WindowsInstaller;
    using global::WixSharp.UI.Forms;
    using System.Globalization;
    using System.Resources;
    using System.Threading;

    /// <summary>
    /// The standard Installation Progress dialog
    /// </summary>
    public partial class ProgressDialog : ManagedForm, IProgressDialog // change ManagedForm->Form if you want to show it in designer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialog"/> class.
        /// </summary>
        public ProgressDialog()
        {
            InitializeComponent();
            dialogText.MakeTransparentOn(banner);

            showWaitPromptTimer = new System.Windows.Forms.Timer() { Interval = 4000 };
            showWaitPromptTimer.Tick += (s, e) =>
            {
                this.waitPrompt.Visible = true;
                showWaitPromptTimer.Stop();
            };
        }

        System.Windows.Forms.Timer showWaitPromptTimer;

        void ProgressDialog_Load(object sender, EventArgs e)
        {
            var culture = new CultureInfo(Runtime.Session[MsiProperties.Locale]);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            banner.Image = Runtime.Session.GetResourceBitmap("banner");

            if (!WindowsIdentity.GetCurrent().IsAdmin() && Uac.IsEnabled())
            {
                showWaitPromptTimer.Start();
            }

            ResetLayout();

            Shell.StartExecute();
        }

        void ResetLayout()
        {
            // The form controls are properly anchored and will be correctly resized on parent form
            // resizing. However the initial sizing by WinForm runtime doesn't a do good job with DPI
            // other than 96. Thus manual resizing is the only reliable option apart from going WPF.
            float ratio = (float)banner.Image.Width / (float)banner.Image.Height;
            topPanel.Height = (int)(banner.Width / ratio);
            topBorder.Top = topPanel.Height + 1;

            var upShift = (int)(next.Height * 2.3) - bottomPanel.Height;
            bottomPanel.Top -= upShift;
            bottomPanel.Height += upShift;

            var fontSize = waitPrompt.Font.Size;
            float scaling = 1;
            waitPrompt.Font = new Font(waitPrompt.Font.Name, fontSize * scaling, FontStyle.Italic);
        }

        /// <summary>
        /// Called when Shell is changed. It is a good place to initialize the dialog to reflect the MSI session
        /// (e.g. localize the view).
        /// </summary>
        protected override void OnShellChanged()
        {
            var manager = new ResourceManager(GetType());
            var culture = new CultureInfo(Runtime.Session[MsiProperties.Locale]);

            if (Runtime.Session.IsUninstalling())
            {
                dialogText.Text =
                Text = GetTranslation(manager, "[ProgressDlgTitleRemoving]", culture);
                description.Text = GetTranslation(manager, "[ProgressDlgTextRemoving]", culture);
            }
            else if (Runtime.Session.IsRepairing())
            {
                dialogText.Text =
                Text = GetTranslation(manager, "[ProgressDlgTextRepairing]", culture);
                description.Text = GetTranslation(manager, "[ProgressDlgTitleRepairing]", culture);
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
                        showWaitPromptTimer.Stop();
                        waitPrompt.Visible = false;
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
                                currentAction.Text = message;
                        }
                        catch
                        {
                            //Catch all, we don't want the installer to crash in an
                            //attempt to process message.
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
            progress.Value = progressPercentage;

            if (progressPercentage > 0)
            {
                waitPrompt.Visible = false;
            }
        }

        /// <summary>
        /// Called when MSI execution is complete.
        /// </summary>
        public override void OnExecuteComplete()
        {
            currentAction.Text = null;
            Shell.GoNext();
        }

        /// <summary>
        /// Handles the Click event of the cancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        void cancel_Click(object sender, EventArgs e)
        {
            if (Shell.IsDemoMode)
                Shell.GoNext();
            else
                Shell.Cancel();
        }

        private static string GetTranslation(ResourceManager manager, string placeholder, CultureInfo culture) =>
            culture.Name == "en-US"
                ? placeholder
                : manager.GetString(placeholder, culture);
    }
}
