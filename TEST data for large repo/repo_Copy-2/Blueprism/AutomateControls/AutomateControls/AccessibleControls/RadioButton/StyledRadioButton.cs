using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls
{
    public partial class StyledRadioButton : RadioButton
    {
        public int ButtonHeight { get; set; } = 21;
        public int RadioButtonDiameter { get; set; } = 12;
        public int FocusDiameter { get; set; } = 16;
        public int RadioButtonThickness { get; set; } = 2;
        public int FocusThickness { get; set; } = 3;
        public int RadioYLocation { get; set; } = 7;
        public int FocusYLocation { get; set; } = 9;
        public int StringYLocation { get; set; } = 1;
        public Color FocusColor { get; set; } = ColourScheme.BluePrismControls.FocusColor;
        public Color ForeGroundColor { get; set; } = ColourScheme.BluePrismControls.ForeColor;
        public Color DisabledColor { get; set; } = ColourScheme.BluePrismControls.DisabledBackColor;
        public Color HoverColor { get; set; } = ColourScheme.BluePrismControls.HoverColor;
        public Color TextColor { get; set; } = ColourScheme.BluePrismControls.TextColor;
        public Color MouseLeaveColor { get; set; } = ColourScheme.BluePrismControls.MouseLeaveColor;
        public bool ForceFocus { get; set; } = true;

        private bool _focused;
        private Color _fillColor;

        public StyledRadioButton()
        {
            _fillColor = MouseLeaveColor;
            InitializeComponent();
        }

        protected override void OnMouseHover(EventArgs e)
        {
            _fillColor = Focused ? MouseLeaveColor : HoverColor;

            Refresh();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _fillColor = MouseLeaveColor;
            base.OnMouseLeave(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            _focused = true;
            _fillColor = MouseLeaveColor;
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            _focused = false;
            base.OnLostFocus(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.Clear(GetBackgroundFillColor());

            RectangleF innerRect = new RectangleF(FocusThickness, (Height / 2) - RadioYLocation, RadioButtonDiameter, RadioButtonDiameter);

            g.FillEllipse(new SolidBrush(_fillColor), innerRect);

            var circleColor = ForeGroundColor;
            var textColor = TextColor;

            if (!Enabled)
            {
                circleColor = DisabledColor;
                textColor = DisabledColor;
            }

            g.DrawEllipse(new Pen(new SolidBrush(circleColor), RadioButtonThickness), innerRect);

            if (_focused && ForceFocus)
            {
                RectangleF focusRectangle = new RectangleF(1, (Height / 2) - FocusYLocation, FocusDiameter, FocusDiameter);
                g.DrawEllipse(new Pen(new SolidBrush(FocusColor), FocusThickness), focusRectangle);
            }

            if (Checked)
            {
                innerRect.Inflate(-FocusThickness, -FocusThickness);
                g.FillEllipse(new SolidBrush(circleColor), innerRect);
            }
            
            g.DrawString(Text, Font, new SolidBrush(textColor), ButtonHeight - FocusThickness, StringYLocation);            
        }

        private Color GetBackgroundFillColor()
        {
            if (Parent != null && Parent.BackColor != Color.Transparent)
            {
                return Parent.BackColor;
            }

            return Color.White;
        }
    }
}
