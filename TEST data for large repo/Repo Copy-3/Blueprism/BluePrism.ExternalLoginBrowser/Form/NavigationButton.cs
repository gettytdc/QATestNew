using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BluePrism.ExternalLoginBrowser.Form
{
    public partial class NavigationButton : UserControl
    {
        public event EventHandler ButtonClicked;

        public Image EnabledImage { get; set; }

        public Image DisabledImage { get; set; }

        [Browsable(true)]
        [Localizable(true)]
        public string ButtonText
        {
            get => Label.Text;
            set => Label.Text = value;
        }

        public NavigationButton()
        {
            InitializeComponent();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            panNavigationButtonContainer.RightToLeft = RightToLeft;
            base.OnRightToLeftChanged(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Image.BackgroundImage = Enabled ? EnabledImage : DisabledImage;
            Label.ForeColor = Enabled ? Color.FromArgb(255, 11, 117, 183) : Color.FromArgb(255, 67, 74, 79);
            base.OnEnabledChanged(e);
        }

        private void NavigationButton_Load(object sender, EventArgs e)
        {
            Image.BackgroundImage = Enabled ? EnabledImage : DisabledImage;
        }

        private void HandleButtonClicked(object sender, EventArgs e)
        {
            ButtonClicked?.Invoke(this, e);
        }
    }
}
