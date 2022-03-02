using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace BluePrism.ExternalLoginBrowser.Form
{

    public partial class NavigationMenu : UserControl
    {
        public event EventHandler FormCloseButtonClicked;
        public event EventHandler FormResizeButtonClicked;

        public NavigationMenu()
        {
            InitializeComponent();
        }

        private void HandleFormResizeButtonClicked(object sender, EventArgs e)
            => FormResizeButtonClicked?.Invoke(this, e);

        private void HandleCloseFormButtonClicked(object sender, EventArgs e)
            => FormCloseButtonClicked?.Invoke(this, e);

      
    }
}
