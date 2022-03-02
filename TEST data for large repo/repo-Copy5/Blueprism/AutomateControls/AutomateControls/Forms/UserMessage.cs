using System;
using System.Windows.Forms;

namespace AutomateControls.Forms
{
    public partial class UserMessage : Form
    {
        public static DialogResult OKCancel(string text, string title)
        {
            using (UserMessage f = new UserMessage())
            {
                f.Text = title;
                f.txtMessage.Text = text;
                f.ShowInTaskbar = false;
                f.StartPosition = FormStartPosition.CenterParent;
                return f.ShowDialog();
            }
        }

        public static DialogResult Ok(string text)
        {
            using (UserMessage f = new UserMessage())
            {
                f.cancelButton.Hide();
                f.txtMessage.Text = text;
                f.ShowInTaskbar = false;
                f.StartPosition = FormStartPosition.CenterParent;
                return f.ShowDialog();
            }
        }

        public UserMessage()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
