using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.Forms
{
    public partial class PopupCheckBoxCloseForm : Form
    {
        public string Title { get; set; }
        public string InfoText { get; set; }
        public string CheckBoxText { get; set; }
        public string ButtonText { get; set; }
        private Point mMouseDownLocation { get; set; }
        public bool IsChecked { get; set; }
        public event Action<object, EventArgs> OnBtnOKClick;

        public PopupCheckBoxCloseForm(string title, string infoText, string checkBoxText, string buttonText)
        {
            Title = title;
            InfoText = infoText;
            CheckBoxText = checkBoxText;
            ButtonText = buttonText;

            InitializeComponent();

            lblTitle.Text = this.Title;
            lblTextInfo.Text = this.InfoText;
            checkBox.Text = this.CheckBoxText;
            btnOK.Text = this.ButtonText;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            OnBtnOKClick?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void tableLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mMouseDownLocation = e.Location;
            }
        }

        private void tableLayoutPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.Location.X - mMouseDownLocation.X;
                this.Top += e.Location.Y - mMouseDownLocation.Y;
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            IsChecked = this.checkBox.Checked;
        }
    }
}
