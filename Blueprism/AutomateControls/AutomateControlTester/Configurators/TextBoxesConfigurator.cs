using System;
using System.Windows.Forms;

namespace AutomateControlTester.Configurators
{
    public partial class TextBoxesConfigurator : BaseConfigurator
    {
        public TextBoxesConfigurator()
        {
            InitializeComponent();
        }

        public override string ConfigName { get { return "TextBoxes"; } }

        private void HandleFilterTextChanged(object sender, AutomateControls.FilterEventArgs e)
        {
            MessageBox.Show("Text Changed: " + filterTextBox1.Text);
        }

        private void HandleFilterCleared(object sender, EventArgs e)
        {
            MessageBox.Show("Filter Cleared");
        }

        private void HandleFilterIconClick(object sender, EventArgs e)
        {
            MessageBox.Show("Filter Icon Clicked");
        }
    }
}
