using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BluePrism.BPCoreLib;

namespace AutomateControls.Wizard
{
    /// <summary>
    /// Base class for Wizards with additional
    /// blueprism branding.
    /// </summary>
    public partial class StandardWizard
    {
        /// <summary>
        /// Allows setting of the title.
        /// </summary>
        public override string Title
        {
            set { Bluebar.Title = value; }
        }

        protected override void OnHelpButtonClicked(CancelEventArgs e)
        {
            base.OnHelpButtonClicked(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            GraphicsUtil.Draw3DLine(e.Graphics, new Point(0, CancelledButton.Top - 10), ListDirection.LeftToRight, Width);
        }

        private void StandardWizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnClosing(sender, e);
        }

        public virtual void OnClosing(object sender, FormClosingEventArgs e)
        {
            // do nothing
        }
    }
}
