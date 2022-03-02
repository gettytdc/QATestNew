using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace AutomateControls
{
    public class TabPageEx : TabPage
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        public TabPageEx()
        {
            this.Invalidated += HandleInvalidated;
        }

        /// <summary>
        /// Handles the invalidation of this tab page. Causes invalidation
        /// of entire parent's set of tab pages.
        /// </summary>
        /// <param name="Sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        private void HandleInvalidated(Object Sender, InvalidateEventArgs e)
        {
            if (!(this.Parent == null))
            {
                this.Parent.Invalidate();
            }
        }

        /// <summary>
        /// Private member to store public property TabBackColor
        /// </summary>
        private Color mTabBackColor;

        /// <summary>
        /// The backcolor of the tab itself (not the place where the
        /// child controls go)
        /// </summary>
        /// <returns></returns>
        public Color TabBackColor
        {
            get
            {
                return mTabBackColor;
            }
            set
            {
                mTabBackColor = value;
            }
        }

        /// <summary>
        /// Reimplementation of the ImageKey property, just to ensure that this tab
        /// is on a TabControl. Otherwise, setting this property will have no effect
        /// </summary>
        public new string ImageKey
        {
            get { return base.ImageKey; }
            set
            {
                base.ImageKey = value;
                Debug.Assert(Parent != null,
                    "Setting ImageKey in a tab page has no effect if the tab " +
                    "is not yet added to a tab control");
            }
        }

        /// <summary>
        /// Private member to store public property TabSelectedBackColor
        /// </summary>
        private Color mTabSelectedBackColor;
        /// <summary>
        /// Color of the tab itself, when selected.
        /// </summary>
        /// <returns></returns>
        public Color TabSelectedBackColor
        {
            get
            {
                return mTabSelectedBackColor;
            }
            set
            {
                mTabSelectedBackColor = value;
            }
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Brush br = new SolidBrush(System.Drawing.Color.White);
            e.Graphics.FillRectangle(br, 0, 0, this.Width, this.Height);

        }

    }
}
