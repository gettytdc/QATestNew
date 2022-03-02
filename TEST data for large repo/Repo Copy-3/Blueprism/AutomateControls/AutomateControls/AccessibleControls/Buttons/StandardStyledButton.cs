using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.Buttons
{
    public partial class StandardStyledButton : StyledButtonBase
    {
        private readonly Font _font = new Font("Segoe UI", 9F, FontStyle.Bold);
        private readonly int _buttonPadding = 15;
        private readonly int _minimumWidth = 75;
        #region Constructors

        public StandardStyledButton()
        {
            InitializeComponent();
            //default all back colour to White unless overridden.
            BackColor = Color.White;
        }

        public StandardStyledButton(string text, DialogResult dr, int tabIndex = 1)
            : this()
        {
            Text = text ?? throw new ArgumentNullException();

            Anchor = AnchorStyles.Top | AnchorStyles.Left;
            AutoSize = false;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(255, 11, 117, 183);
            Font = _font;
            ImageAlign = ContentAlignment.MiddleRight;
            Visible = true;
            ImeMode = ImeMode.NoControl;
            Location = new Point(10, 10);
         

            var requiredWidth = TextRenderer.MeasureText(text, _font).Width + _buttonPadding;
            requiredWidth = requiredWidth < _minimumWidth ? _minimumWidth : requiredWidth;

            Size = new Size(requiredWidth, 25);
            TabIndex = tabIndex;
            TextImageRelation = TextImageRelation.ImageBeforeText;
            UseVisualStyleBackColor = false;

            DialogResult = dr;
        }

        public StandardStyledButton(IContainer container)
            : this()
        {
            container.Add(this);
        }
        #endregion

        public new FlatStyle FlatStyle
        {
            get => FlatStyle.Flat;
            set { }
        }

    }
}
