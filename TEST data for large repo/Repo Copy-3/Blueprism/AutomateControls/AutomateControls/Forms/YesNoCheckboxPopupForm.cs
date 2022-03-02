using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.Forms
{
    public partial class YesNoCheckboxPopupForm : Form
    {
        #region Properties
        public string Title { get; set; }
        public string InfoText { get; set; }
        public string CheckBoxText { get; set; }
        private Point MouseDownLocation { get; set; }
        public bool IsChecked { get; set; }

        public event Action<object, EventArgs> OnYesButtonClick;
        #endregion

        public YesNoCheckboxPopupForm(string title, string infoText, string checkBoxText)
        {
            Title = title;
            InfoText = infoText;
            CheckBoxText = checkBoxText;

            InitializeComponent();

            TitleLabel.Text = Title;
            MessageLabel.Text = InfoText;

            //Extends the form to accomodate larger messages
            Height += infoText.Length / 10;

            CheckBox.Text = CheckBoxText;
        }

        #region Event Handlers
        private void YesButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            OnYesButtonClick?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void LayoutPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MouseDownLocation = e.Location;
            }
        }

        private void LayoutPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Left += e.Location.X - MouseDownLocation.X;
                Top += e.Location.Y - MouseDownLocation.Y;
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e) => IsChecked = CheckBox.Checked;
        #endregion
    }
}
