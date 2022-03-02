using System.Windows.Forms;
using System.ComponentModel;
using BluePrism.Server.Domain.Models;
using System.Drawing;
using BluePrism.BPCoreLib;

namespace AutomateControls
{
    /// <summary>
    /// Button containing only an arrow
    /// </summary>
    public class ArrowButton : Button
    {
        #region - Member Variables -

        // The direction of the arrow
        private Direction _dirn;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty arrow button pointing left
        /// </summary>
        public ArrowButton()
        {
            base.Text = "";
            _dirn = Direction.Left;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The direction that the arrow on this button should point
        /// </summary>
        [DefaultValue(Direction.Left)]
        public Direction Direction
        {
            get { return _dirn; }
            set
            {
                if (_dirn != value)
                {
                    _dirn = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// The minimum size of the button
        /// </summary>
        [Browsable(false)]
        public override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set { base.MinimumSize = value; }
        }

        /// <summary>
        /// The text on the button
        /// </summary>
        [Browsable(false), DefaultValue("")]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
        /// The maximum size of the button
        /// </summary>
        [Browsable(false)]
        public override Size MaximumSize
        {
            get { return base.MaximumSize; }
            set { base.MaximumSize = value; }
        }

        /// <summary>
        /// The default maximum size for this button
        /// </summary>
        protected override Size DefaultMaximumSize
        {
            get { return new Size(24, 24); }
        }

        /// <summary>
        /// The default minimum size of this button
        /// </summary>
        protected override Size DefaultMinimumSize
        {
            get { return new Size(18, 18); }
        }

        /// <summary>
        /// The triangle co-ordinates which describe the arrow pointing in the
        /// direction set in this button. Typically, 3 Point values are returned. If
        /// the direction set in this button is not a valid arrow direction, an
        /// empty array is returned.
        /// </summary>
        private Point[] TriangleCoords
        {
            get
            {
                Rectangle bounds = this.ClientRectangle;
                bounds.Inflate(-6, -6);
                bounds.Offset(-1, -1);

                Size s = bounds.Size;
                int w = s.Width;
                int h = s.Height;


                Point[] points;
                switch (_dirn)
                {
                    case Direction.Left:
                        points = new Point[]{
                        new Point(w, 0),
                        new Point(w, h),
                        new Point(0, h/2)
                    };
                        break;

                    case Direction.Right:
                        points = new Point[]{
                        new Point(0, 0),
                        new Point(0, h),
                        new Point(w, h/2)
                    };
                        break;

                    case Direction.Top:
                        points = new Point[]{
                        new Point(0, h),
                        new Point(w, h),
                        new Point(w/2, 0)
                    };
                        break;

                    case Direction.Bottom:
                        points = new Point[]{
                        new Point(0, 0),
                        new Point(w, 0),
                        new Point(w/2, h)
                    };
                        break;

                    case Direction.None:
                        return new Point[0];

                    default:
                        throw new InvalidValueException(
                            "Invalid direction: {0}", _dirn);
                }

                for (int i = 0; i < points.Length; i++)
                {
                    points[i].Offset(bounds.Location);
                }

                return points;
            }
        }

        #endregion

        #region - OnEvent overrides -

        /// <summary>
        /// Handles the painting of this button, overlaying the arrow on the button
        /// after all other painting is complete
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Point[] points = TriangleCoords;

            if (points.Length == 0) // No triangle to draw
                return;

            if (Enabled)
            {
                e.Graphics.FillPolygon(SystemBrushes.ControlDarkDark, points);
                e.Graphics.DrawPolygon(SystemPens.ControlDarkDark, points);
            }
            for (int i = 0; i < points.Length; i++)
            {
                points[i].Offset(1, 1);
            }
            Brush b;
            Pen p;
            if (!Enabled)
            {
                b = SystemBrushes.Control;
                p = SystemPens.ControlDark;
            }
            else if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                b = SystemBrushes.Highlight;
                p = SystemPens.Highlight;
            }
            else
            {
                b = SystemBrushes.ControlDark;
                p = SystemPens.ControlDark;
            }
            e.Graphics.FillPolygon(b, points);
            e.Graphics.DrawPolygon(p, points);
        }

        #endregion

    }
}
