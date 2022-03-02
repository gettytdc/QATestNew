using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace AutomateControls
{
    /// <summary>
    /// Button with a drop down menu
    /// </summary>
    public class MenuButton : Button
    {
        #region - Member Variables -

        private Color _savedBackColor;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty menu button
        /// </summary>
        public MenuButton()
        {
            base.Text = "";
            base.UseVisualStyleBackColor = false;
            base.FlatStyle = FlatStyle.Flat;
            base.FlatAppearance.BorderSize = 0;
            base.Image = BluePrism.Images.ToolImages.Menu_Button_16x16;
            base.ImageAlign = ContentAlignment.MiddleCenter;
        }

        #endregion

        #region - Properties -

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
            get { return new Size(23, 23); }
        }

        /// <summary>
        /// The default minimum size of this button
        /// </summary>
        protected override Size DefaultMinimumSize
        {
            get { return new Size(23, 23); }
        }

        /// <summary>
        /// Rework of the UseVisualStyleBackColor property to hide it as much as
        /// possible from the user (not in designer, not in Intellisense)
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
         EditorBrowsable(EditorBrowsableState.Never)]
        public new bool UseVisualStyleBackColor
        {
            get { return base.UseVisualStyleBackColor; }
            set { base.UseVisualStyleBackColor = value; }
        }

        #endregion

        #region - Event Invoker Overrides -

        /// <summary>
        /// Raises the Click event handler
        /// </summary>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (ContextMenuStrip == null) return;
            Size prefSize = ContextMenuStrip.GetPreferredSize(Size.Empty);
            Point locn = new Point((this.Width + 1) - prefSize.Width, this.Height);
            ContextMenuStrip.Show(this, locn);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            //_savedVisualStyleBackColor = UseVisualStyleBackColor;
            if (Application.RenderWithVisualStyles)
                UseVisualStyleBackColor = false;
            _savedBackColor = BackColor;
            BackColor = Lighten(_savedBackColor);
            base.OnMouseEnter(e);
        }

        private Color Lighten(Color col)
        {
            ColorHSL hsl = col;
            if (hsl.Luminance < 128f)
                return hsl.Illuminate(20);
            else
                return hsl.Illuminate(-20);
        }

        /// <summary>
        /// Overrides the back color to ensure that 'white' isn't actually used as a
        /// back color (since the menu icon is white on transparent, it would not
        /// show an awful lot). Silently replaces any white values being set with a
        /// <see cref="Color.LightGray"/> value.
        /// </summary>
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                if (value.ToArgb() == Color.White.ToArgb())
                    value = Color.FromArgb(230, 230, 230);
                base.BackColor = value;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            BackColor = _savedBackColor;
        }

        #endregion

    }
}
