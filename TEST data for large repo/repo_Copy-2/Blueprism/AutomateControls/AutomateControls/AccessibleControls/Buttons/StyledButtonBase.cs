using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.Buttons
{
    public partial class StyledButtonBase : Button
    {

        private Color _focusColor;
        private Color _hoverColor;
        private Color _disabledBackColor;
        private Color _disabledForeColor;
      
        private Color _originalBackColor;
        private Color _originalForeColor;
        
        private bool _working;
        private bool _hovering;
        private bool _currentEnabledState;

        public StyledButtonBase()
        {
            InitializeComponent();
            SetAccessibility();
        }

        public StyledButtonBase(IContainer container)
            : this()
        {
            container.Add(this);
        }

        private void SetAccessibility()
        {
            _focusColor = ColourScheme.BluePrismControls.FocusColor;
            _hoverColor = ColourScheme.BluePrismControls.HoverColor;
            _disabledBackColor = ColourScheme.BluePrismControls.DisabledBackColor;
            _disabledForeColor = ColourScheme.BluePrismControls.DisabledForeColor;

            _originalBackColor = BackColor;
            _originalForeColor = ForeColor;
            
            MouseEnter += StyledButton_MouseEnter;
            MouseLeave += StyledButton_MouseLeave;
            BackColorChanged += StyledButton_BackColorChanged;
            EnabledChanged += StyledButtonBase_EnabledChanged;
            Enter += OnFocus;

            _currentEnabledState = this.Enabled;
        }

        private void OnFocus(object sender, EventArgs e)
        {
            _working = true;
            BackColor = _originalBackColor;
            _working = false;
        }

        private void StyledButtonBase_EnabledChanged(object sender, EventArgs e)
        {
            // Only update if the state has changed, we are getting mysterious events fired when the 
            // state hasn't yet changed.
            if (_currentEnabledState == Enabled)
                return;

            if (Enabled)
            {
                ForeColor = _originalForeColor;
                ChangeBackColor(_originalBackColor);
            }
            else
            {
                if (!_hovering)
                {
                    _originalBackColor = BackColor;
                    _originalForeColor = ForeColor;
                }

                ForeColor = _disabledForeColor;
                ChangeBackColor(_disabledBackColor);
            }

            _currentEnabledState = Enabled;
        }

        private void ChangeBackColor(Color color)
        {
            _working = true;
            BackColor = color;
            _working = false;
        }
        private void StyledButton_MouseLeave(object sender, EventArgs e)
        {
            if (!Enabled) return;
            _hovering = false;
            ChangeBackColor(_originalBackColor);
        }

        private void StyledButton_MouseEnter(object sender, EventArgs e)
        {
            if (!Enabled || Focused) return;
            _hovering = true;
            ChangeBackColor(_hoverColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Focused)
            {
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                    _focusColor, BorderWidth, ButtonBorderStyle.Solid,
                    _focusColor, BorderWidth, ButtonBorderStyle.Solid,
                    _focusColor, BorderWidth, ButtonBorderStyle.Solid,
                    _focusColor, BorderWidth, ButtonBorderStyle.Solid);
            }
        }

        private void StyledButton_BackColorChanged(object sender, EventArgs e)
        {
            if (!_working)
                _originalBackColor = this.BackColor;
            if (!Enabled && BackColor != _disabledBackColor)
            {
                ChangeBackColor(_disabledBackColor);
            }
        }

        public new bool UseVisualStyleBackColor
        {
            get => false;
            set { }
        }

        [Description("Sets the width of the focus rectangle, too big will impact text wrapping"),
         Category("Accessibility"),
         DefaultValue(3),
         Browsable(true)]
        public int BorderWidth { get; set; } = 3;
    }
}
