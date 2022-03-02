using AutomateControls.Buttons;
using AutomateControls.Properties;
using System;
using System.Windows.Forms;

namespace AutomateControls.Forms
{
    public partial class BPMessageBox : AutomateForm
    {
        private int _tabOrderCount;
        private readonly Exception _exception;

        private string StackTrace { get { return _exception?.StackTrace; } }

        public static DialogResult ShowDialog(string message, string title, MessageBoxButtons buttons)
        {
            using (var dialog = new BPMessageBox(message, title, buttons))
            {
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ShowInTaskbar = false;
                return dialog.ShowDialog();
            }
        }

        public static DialogResult ShowDialog(string message, string title, MessageBoxButtons buttons, Exception ex)
        {
            using (var dialog = new BPMessageBox(message, title, ex, buttons))
            {
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ShowInTaskbar = false;
                return dialog.ShowDialog();
            }
        }


        public BPMessageBox(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            InitializeComponent();

            Text = title ?? throw new ArgumentNullException(nameof(title));
            labelMessage.Text = message ?? throw new ArgumentNullException(nameof(message));


            InitButtons(buttons);

        }

        public BPMessageBox(string message, string title, Exception ex, MessageBoxButtons buttons = MessageBoxButtons.RetryCancel)
            : this(message, title, buttons)
        {
            _exception = ex;
            BtnExceptionClipboard.Visible = StackTrace != null;
        }

        private void InitButtons(MessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxButtons.AbortRetryIgnore:
                    AddButton(Resources.Ignore, DialogResult.Ignore);
                    AddButton(Resources.Retry, DialogResult.Retry);
                    AddButton(Resources.Abort, DialogResult.Abort);
                    break;
                case MessageBoxButtons.OK:
                    AddButton(Resources.OK, DialogResult.OK);
                    break;
                case MessageBoxButtons.OKCancel:
                    AddButton(Resources.Cancel, DialogResult.Cancel);
                    AddButton(Resources.OK, DialogResult.OK);
                    break;
                case MessageBoxButtons.RetryCancel:
                    AddButton(Resources.Cancel, DialogResult.Cancel);
                    AddButton(Resources.Retry, DialogResult.Retry);
                    break;
                case MessageBoxButtons.YesNo:
                    AddButton(Resources.FrmYesNo_No, DialogResult.No);
                    AddButton(Resources.FrmYesNo_Yes, DialogResult.Yes);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    AddButton(Resources.FrmYesNo_Yes, DialogResult.Yes);
                    AddButton(Resources.FrmYesNo_No, DialogResult.No);
                    AddButton(Resources.Cancel, DialogResult.Cancel);
                    break;
                default:
                    AddButton(Resources.OK, DialogResult.OK);
                    break;
            }
        }

        private void AddButton(string text, DialogResult dialogResult)
        {
            var btn = new StandardStyledButton(text, dialogResult, _tabOrderCount++);
            btn.MouseClick += Button_MouseClick;
            buttonPanel.Controls.Add(btn);
        }

        private void AddButton(string text, DialogResult dialogResult, MouseEventHandler func)
        {
            var btn = new StandardStyledButton(text, dialogResult, _tabOrderCount++);
            btn.MouseClick += func;
        }

        private void Button_MouseClick(object sender, MouseEventArgs e)
        {
            var btn = (StandardStyledButton)sender;
            DialogResult = btn.DialogResult;
        }

        private void BtnExceptionClipboard_Click(object sender, EventArgs e)
        {
            if (StackTrace != null)
                Clipboard.SetText(StackTrace);
        }
    }
}
