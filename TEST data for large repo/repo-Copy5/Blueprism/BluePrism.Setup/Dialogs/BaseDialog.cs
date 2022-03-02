using System.Drawing;
using System.Windows.Forms;
using WixSharp;
using WixSharp.UI.Forms;

namespace BluePrism.Setup
{
    public partial class BaseDialog : ManagedForm, IManagedDialog
    {
        public BaseDialog()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, System.EventArgs e)
        {
            Shell.Exit();
        }

        #region "Drag And Drop Window"

        private Point _MouseDownLocation;

        private void BorderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _MouseDownLocation = e.Location;
        }

        private void BorderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var form = (Form) Parent ?? this;
            form.Left += e.Location.X - _MouseDownLocation.X;
            form.Top += e.Location.Y - _MouseDownLocation.Y;
        }

        #endregion

        protected virtual void Back_Click(object sender, System.EventArgs e)
        {
            Shell.GoPrev();
        }

        protected void ReInitialize()
        {
            InitializeComponent();
        }

        protected void HideBackButton()
        {
            BackButton.Visible = false;
            BackLabel.Visible = false;
        }

        protected void HideExitButton()
        {
            ExitButton.Visible = false;
        }
    }
}