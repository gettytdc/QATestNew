using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using BluePrism.Server.Domain.Models;

namespace AutomateControls
{
    /// <summary>
    /// A SplitContainer class which displays a visible grip at the splitter location
    /// </summary>
    public class GrippableSplitContainer : FlickerFreeSplitContainer
    {
        #region - Published Events -

        /// <summary>
        /// Event fired when the colour of the split is changed in this container
        /// </summary>
        public event EventHandler SplitColorChanged;

        /// <summary>
        /// Event fired when the visibility of the split is changed
        /// </summary>
        public event EventHandler SplitLineModeChanged;

        /// <summary>
        /// Event fired when the visibility of the gripper is changed
        /// </summary>
        public event EventHandler GripVisibleChanged;

        #endregion

        #region - Member Variables -

        /// <summary>
        /// The colour of the split to paint
        /// </summary>
        private Color _splitColor = SystemColors.ControlDark;

        /// <summary>
        /// The mode detailing the way the split line(s) should be rendered
        /// </summary>
        private GrippableSplitLineMode _splitLineMode;

        /// <summary>
        /// Flag indicating whether to paint the grip or not
        /// </summary>
        private bool _gripVisible = true;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty GrippableSplitContainer
        /// </summary>
        public GrippableSplitContainer()
        {
            this.TabStop = false;
            this.BackColor = Color.Transparent;
            this.SplitterWidth = 5;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets or sets the background colour of this container
        /// </summary>
        [Category("Appearance"),
         Description("The background color of the component"),
         DefaultValue(typeof(Color), "Transparent")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        /// <summary>
        /// 'Override' of SplitterWidth, just so we can expose a different default
        /// value.
        /// </summary>
        [Category("Layout"),
         Description("Determines the thickness of the splitter"),
         DefaultValue(5)]
        public new int SplitterWidth
        {
            get { return base.SplitterWidth; }
            set { base.SplitterWidth = value; }
        }

        /// <summary>
        /// The colour of the split lines to paint between the 2 split panels.
        /// </summary>
        [Category("Appearance"),
         Description("Sets the colour of the splitter in this container"),
         DefaultValue(typeof(Color), "ControlDark")]
        public Color SplitLineColor
        {
            get { return _splitColor; }
            set
            {
                if (value != _splitColor)
                {
                    _splitColor = value;
                    OnSplitColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Set the splitter lines to be visible or not.
        /// </summary>
        /// <exception cref="InvalidArgumentValue">When setting, if the value given
        /// is not one of the defined values in <see cref="GrippableSplitLineMode"/>.
        /// </exception>
        [Category("Appearance"),
         Description(
             "The mode in which split lines should be rendered in this container"),
         DefaultValue(GrippableSplitLineMode.None)]
        public GrippableSplitLineMode SplitLineMode
        {
            get { return _splitLineMode; }
            set
            {
                // Ignore the set if it's already at this value
                if (value == _splitLineMode)
                    return;

                // Validate the given value
                switch (value)
                {
                    case GrippableSplitLineMode.None:
                    case GrippableSplitLineMode.Single:
                    case GrippableSplitLineMode.Double:
                        break;
                    default:
                        throw new InvalidArgumentException(
                            "Invalid GrippableSplitLineMode value: {0}", value);
                }
                _splitLineMode = value;
                OnSplitLineModeChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets the grip to be visible or otherwise.
        /// </summary>
        [Category("Appearance"),
         Description("Shows or hides the grip in the splitter of this container"),
         DefaultValue(true)]
        public bool GripVisible
        {
            get { return _gripVisible; }
            set
            {
                if (value != _gripVisible)
                {
                    _gripVisible = value;
                    OnGripVisibleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Whether or not the split line or lines are visible
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SplitVisible
        {
            get { return (_splitLineMode != GrippableSplitLineMode.None); }
        }

        #endregion

        #region - OnXXX Event Handlers -

        /// <summary>
        /// Raises the <see cref="SplitColorChanged"/ event
        /// </summary>
        /// <param name="e">The args detailing the event</param>
        protected virtual void OnSplitColorChanged(EventArgs e)
        {
            RaiseSplitterEvent(SplitColorChanged, e);
        }

        /// <summary>
        /// Raises the <see cref="SplitLineModeChanged"/> event
        /// </summary>
        /// <param name="e">The args detailing the event</param>
        protected virtual void OnSplitLineModeChanged(EventArgs e)
        {
            RaiseSplitterEvent(SplitLineModeChanged, e);
        }

        /// <summary>
        /// Raises the <see cref="GripVisibleChanged"/> event
        /// </summary>
        /// <param name="e">The args detailing the evnet</param>
        private void OnGripVisibleChanged(EventArgs e)
        {
            RaiseSplitterEvent(GripVisibleChanged, e);
        }

        /// <summary>
        /// Shifts focus away from this container as soon as it gets it, to ensure
        /// that the 'selected splitter' ugliness is around for as short a time as
        /// possible
        /// </summary>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (!DesignMode)
                Panel1.Focus();
        }

        /// <summary>
        /// Invalidates the container
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate(SplitterRectangle, false);
        }

        /// <summary>
        /// Handles the Paint event on this container, drawing the lines which
        /// make the splitter easier to see.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!Panel1Collapsed && !Panel2Collapsed)
                PaintGrips(e.Graphics);
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Invalidates the splitter rectangle area and invokes the given
        /// event handler with the specified args.
        /// </summary>
        /// <param name="evt">The event handler to invoke for this event</param>
        /// <param name="e">The args to pass to the event handler</param>
        private void RaiseSplitterEvent(EventHandler evt, EventArgs e)
        {
            Invalidate(SplitterRectangle, false);
            if (evt != null)
                evt(this, e);
        }

        /// <summary>
        /// Paints the grips as currently configured in this container
        /// </summary>
        /// <param name="g">The graphics context on which to draw the grips</param>
        private void PaintGrips(Graphics g)
        {
            // Nothing to paint if neither split nor grip are visible...
            if (!_gripVisible && !SplitVisible)
                return;

            // paint the three dots'
            Point[] ellipses = new Point[3];
            Rectangle[] gripLines = new Rectangle[
                _splitLineMode == GrippableSplitLineMode.Single ? 1 :
                _splitLineMode == GrippableSplitLineMode.Double ? 2 : 0];

            int w = Width;
            int h = Height;
            int d = SplitterDistance;
            int sw = SplitterWidth;
            int firstLine = (
                _splitLineMode == GrippableSplitLineMode.Single ? d + (sw / 2) :
                _splitLineMode == GrippableSplitLineMode.Double ? d : 0);
            int secondLine = d + sw - 1;

            //calculate the position of the points'
            if (Orientation == Orientation.Horizontal)
            {
                ellipses[0] = new Point((w / 2), d + (sw / 2));
                ellipses[1] = new Point(ellipses[0].X - 10, ellipses[0].Y);
                ellipses[2] = new Point(ellipses[0].X + 10, ellipses[0].Y);
                switch (_splitLineMode)
                {
                    case GrippableSplitLineMode.Double:
                        gripLines[1] = new Rectangle(0, secondLine, w, 0);
                        goto case GrippableSplitLineMode.Single;

                    case GrippableSplitLineMode.Single:
                        gripLines[0] = new Rectangle(0, firstLine, w, 0);
                        break;
                }
                
            }
            else
            {
                ellipses[0] = new Point(d + (sw / 2), (h / 2));
                ellipses[1] = new Point(ellipses[0].X, ellipses[0].Y - 10);
                ellipses[2] = new Point(ellipses[0].X, ellipses[0].Y + 10);
                switch (_splitLineMode)
                {
                    case GrippableSplitLineMode.Double:
                        gripLines[1] = new Rectangle(secondLine, 0, 0, h);
                        goto case GrippableSplitLineMode.Single;

                    case GrippableSplitLineMode.Single:
                        gripLines[0] = new Rectangle(firstLine, 0, 0, h);
                        break;
                }
            }

            if (GripVisible)
            {
                foreach (Point p in ellipses)
                {
                    p.Offset(-2, -2);
                    g.FillEllipse(SystemBrushes.ControlDark,
                        new Rectangle(p, new Size(3, 3)));

                    p.Offset(1, 1);
                    g.FillEllipse(SystemBrushes.ControlLight,
                        new Rectangle(p, new Size(3, 3)));
                }
            }

            // If we're showing the split draw the lines.
            if (SplitVisible)
            {
                using (Pen p = new Pen(_splitColor))
                {
                    foreach (Rectangle r in gripLines)
                    {
                        g.DrawLine(p, r.Location, r.Location + r.Size);
                    }
                }
            }

        }

        #endregion

    }
}
