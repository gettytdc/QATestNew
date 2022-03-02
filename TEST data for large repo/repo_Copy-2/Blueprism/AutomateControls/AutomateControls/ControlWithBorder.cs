using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Security.Permissions;

namespace AutomateControls
{
    /// <summary>
    /// Fixes the annoying problem with not being able to set
    /// the border style on a user control, by extending the
    /// class.
    /// </summary>
    public class UserControlWithBorder : UserControl
    {
        public UserControlWithBorder()
        {
        }

        /// <summary>
        /// Private member to store public property BorderStyle
        /// </summary>
        private BorderStyle _BorderStyle;
        /// <summary>
        /// The style of border used by the control
        /// </summary>
        public System.Windows.Forms.BorderStyle FrameBorderStyle
        {
            get
            {
                return _BorderStyle;
            }
            set
            {
                if (this._BorderStyle != value)
                {
                    if (!Enum.IsDefined(typeof(BorderStyle), value))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(BorderStyle));
                    }
                    this._BorderStyle = value;
                    base.UpdateStyles();
                }

            }
        }


        /// <summary>
        /// We override this method to make sure that the 
        /// base class includes border style when it calls
        /// SetWindowPos.
        /// </summary>
        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand)]
            get
            {
                CreateParams params1 = base.CreateParams;
                params1.ExStyle |= 0x10000;
                params1.ExStyle &= -513;
                params1.Style &= -8388609;
                switch (this._BorderStyle)
                {
                    case BorderStyle.FixedSingle:
                        params1.Style |= 0x800000;
                        return params1;

                    case BorderStyle.Fixed3D:
                        params1.ExStyle |= 0x200;
                        return params1;
                }
                return params1;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // UserControlWithBorder
            // 
            this.Name = "UserControlWithBorder";
            this.ResumeLayout(false);

        }
    }
}
