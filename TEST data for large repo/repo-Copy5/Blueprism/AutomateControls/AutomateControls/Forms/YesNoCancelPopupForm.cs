using System;
using System.Drawing;
using System.Windows.Forms;
using AutomateControls.Properties;

namespace AutomateControls.Forms
{
    public partial class YesNoCancelPopupForm : Form
    {

        public string Title { get; set; }
        public string InfoText { get; set; }
        public string ButtonText { get; set; }
        public string ContinueText { get; set; }
        private Point mMouseDownLocation { get; set; }

        public YesNoCancelPopupForm(string title, string infoText)
        {
            Title = title;
            InfoText = infoText;

            InitializeComponent();

            lblTitle.Text = this.Title;
            lblMessage.Text = this.InfoText;
            lblContinue.Text = Resources.FrmYesNo_Continue;
        }
        public YesNoCancelPopupForm(string title, string infoText, string decisionText)
            : this(title, infoText)
        {
            ContinueText = decisionText;
            lblContinue.Text = ContinueText;
        }

        private void YesNoPopupForm_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(this.Width, this.Height);
            this.MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            this.AutoSize = true;
        }

        private void btnProceed_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }
        private void btnNo_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
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
    }

}
