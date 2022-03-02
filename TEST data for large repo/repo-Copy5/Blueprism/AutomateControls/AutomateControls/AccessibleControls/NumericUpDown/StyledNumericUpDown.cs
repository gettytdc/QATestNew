using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls
{
    public partial class StyledNumericUpDown : NumericUpDown
    {
        private Color _originalBackColour = Color.Empty;
        private bool _hasFocus = false;

        public StyledNumericUpDown()
        {
            InitializeComponent();
            Controls[1].MouseLeave += OnMouseLeave;
            Controls[1].MouseEnter += OnMouseEnter;
            LostFocus += OnLostFocus;
            GotFocus += OnGotFocus;
        }
                
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Enabled && _hasFocus)
            {
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                            ColourScheme.BluePrismControls.FocusColor, 3, ButtonBorderStyle.Solid,
                                            ColourScheme.BluePrismControls.FocusColor, 3, ButtonBorderStyle.Solid,
                                            ColourScheme.BluePrismControls.FocusColor, 3, ButtonBorderStyle.Solid,
                                            ColourScheme.BluePrismControls.FocusColor, 3, ButtonBorderStyle.Solid);
            }
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            if (Enabled && !_hasFocus && 
                BackColor != ColourScheme.BluePrismControls.HoverColor)
            {     
                    _originalBackColour = BackColor;
                    BackColor = ColourScheme.BluePrismControls.HoverColor;
                
            }
            Invalidate();
        }
        
        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (BackColor == ColourScheme.BluePrismControls.HoverColor)
            {
                BackColor = _originalBackColour;
            }
            Invalidate();
        }

        private void OnLostFocus(object sender, EventArgs e)
        {
            _hasFocus = false;
            Invalidate();
        }

        private void OnGotFocus(object sender, EventArgs e)
        {
            _hasFocus = true;
            Invalidate();
        }
    }
}
