using System.Windows.Forms;
using System.Drawing;
using BluePrism.BPCoreLib;

namespace AutomateControls
{
    /// <summary>
    /// Little graphic class to paint
    /// a "3d" line.
    /// </summary>
    public class Line3D : Control
    {

        /// <summary>
        /// Handles the control being resized
        /// forces it to be height 2px.
        /// </summary>
        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            this.Height = 2;
            this.Invalidate();
        }

        /// <summary>
        /// Paint event that draws the line.
        /// </summary>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            GraphicsUtil.Draw3DLine(e.Graphics, Point.Empty, ListDirection.LeftToRight, Width);
        }

    }
}
