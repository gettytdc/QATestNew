using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.Forms
{
    public partial class YesNoPopupForm : Form
    {

        public string Title { get; set; }
        public string InfoText { get; set; }
        public string ButtonText { get; set; }
        public string ContinueText { get; set; }
        private Point mMouseDownLocation { get; set; }

        public YesNoPopupForm(string title, string infoText)
        {
            Title = title;
            InfoText = infoText;

            InitializeComponent();

            lblTitle.Text = this.Title;
            lblMessage.Text = this.InfoText;

            //Extends the form to accomodate larger messages
            this.Height = this.Height + infoText.Length / 10;
        }
        public YesNoPopupForm(string title,string infoText, string decisionText) 
            : this(title,infoText)
        {
            ContinueText = decisionText;
            lblContinue.Text = ContinueText;
        }

        private void YesNoPopupForm_Load(object sender, EventArgs e)
        { 
        }

        private void btnProceed_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
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
