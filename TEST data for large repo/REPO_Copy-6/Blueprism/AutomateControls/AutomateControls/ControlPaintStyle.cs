using System.Windows.Forms;
using System.Drawing;
using System;
using System.Runtime.InteropServices;

namespace AutomateControls
{

    // Project   : Automate
    // Class     : clsControlPaintStyle
    // 
    // <summary>
    // A graphics class providing shared control drawing functions via a mixture of Windows
    // API methods and .NET methods, depending on what visual style is enabled in the
    // operating system.
    // </summary>
    public class ControlPaintStyle
    {

        // <summary>
        // Determines whether visual styles are enabled.
        // </summary>
        // <returns></returns>
        public static bool VisualStylesEnabled()
        {
            try
            {
                if (IsThemeActive() == 1)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }





        // <summary>
        // Draws a button on the given graphics object.
        // </summary>
        // <param name="graphics">The graphics object on which to draw the button.</param>
        // <param name="x">The x position of the button.</param>
        // <param name="y">The y position of the button.</param>
        // <param name="width">The width of the button.</param>
        // <param name="height">The height of the button.</param>
        // <param name="state">The state of the button.</param>
        public static void DrawButton(Graphics graphics, int x, int y, int width, int height, System.Windows.Forms.ButtonState state)
        {
            Rectangle rRect = new Rectangle(x, y, width, height);
            DrawButton(graphics, rRect, state);
        }

        // <summary>
        // Draws a combo button on the given graphics object
        // </summary>
        // <param name="graphics"></param>
        // <param name="x">The x position of the combo button.</param>
        // <param name="y">The y position of the combo button.</param>
        // <param name="width">The width of the button.</param>
        // <param name="height">The height of the button.</param>
        // <param name="state">The state of the button.</param>
        public static void DrawComboButton(Graphics graphics, int x, int y, int width, int height, System.Windows.Forms.ButtonState state)
        {
            Rectangle rRect = new Rectangle(x, y - 1, width + 1, height + 2);
            DrawComboButton(graphics, rRect, state);
        }


        // <summary>
        // Draws a button on the given graphics object.
        // </summary>
        // <param name="graphics">The graphics object on which to draw the button.</param>
        // <param name="rectangle">A rectangle defining the bounds of the button.</param>
        // <param name="state">The state of the button.</param>
        public static void DrawButton(Graphics graphics, Rectangle rectangle, System.Windows.Forms.ButtonState state)
        {
            if (!VisualStylesEnabled())
            {
                ControlPaint.DrawButton(graphics, rectangle, state);
                return;
            }
            else
            {
                DrawPrimitive(graphics, rectangle, "Button", 1, 1);
            }
        }


        // <summary>
        // Draws a combo button on the given graphics object.
        // </summary>
        // <param name="graphics">The graphics object on which to draw the combo button.</param>
        // <param name="rectangle">A rectangle defining the bounds of the combo button.</param>
        // <param name="state">The state of the button.</param>
        public static void DrawComboButton(Graphics graphics, Rectangle rectangle, System.Windows.Forms.ButtonState state)
        {
            if (!VisualStylesEnabled())
            {
                ControlPaint.DrawComboButton(graphics, rectangle, state);
                return;
            }
            else
            {
                ComboBoxState cbState;
                switch (state)
                {
                    case System.Windows.Forms.ButtonState.Pushed:
                        cbState = ComboBoxState.CBXS_PRESSED;
                        break;
                    case System.Windows.Forms.ButtonState.Normal:
                        cbState = ComboBoxState.CBXS_NORMAL;
                        break;
                    case System.Windows.Forms.ButtonState.Inactive:
                        cbState = ComboBoxState.CBXS_DISABLED;
                        break;
                    default:
                        cbState = ComboBoxState.CBXS_NORMAL;
                        break;
                }
                DrawPrimitive(graphics, rectangle, "ComboBox", 1, (int)cbState);
            }
        }

        public enum ComboBoxState
        {
            CBXS_NORMAL = 1,
            CBXS_HOT = 2,
            CBXS_PRESSED = 3,
            CBXS_DISABLED = 4
        }

        private enum CheckBoxState
        {
            CBS_UNCHECKEDNORMAL = 1,
            CBS_CHECKEDNORMAL = 5
        }

        public static void DrawCheckbox(Graphics g, Rectangle Bounds, System.Windows.Forms.CheckState state)
        {
            if (VisualStylesEnabled())
            {
                CheckBoxState CBS = CheckBoxState.CBS_UNCHECKEDNORMAL;
                if (state == CheckState.Checked)
                    CBS = CheckBoxState.CBS_CHECKEDNORMAL;

                const int BP_CHECKBOX = 3;
                DrawPrimitive(g, Bounds, "Button", BP_CHECKBOX, (int)CBS);
            }
            else
            {
                ButtonState BS = ButtonState.Normal;
                if (state == CheckState.Checked)
                    BS = ButtonState.Checked;
                ControlPaint.DrawCheckBox(g, Bounds, BS);
            }
        }

        // <summary>
        // Draws a 3D border on the given graphics object.
        // </summary>
        // <param name="graphics">The graphics object on which to draw the 3D border.</param>
        // <param name="rectangle">A rectangle defining the bounds of the 3D border.</param>
        // <param name="state">The state of the 3D border.</param>
        public static void DrawBorder3D(Graphics graphics, Rectangle rectangle, System.Windows.Forms.Border3DStyle state)
        {
            if (!VisualStylesEnabled())
            {
                ControlPaint.DrawBorder3D(graphics, rectangle, state);
            }
            else
            {
                DrawPrimitive(graphics, rectangle, "Edit", 1, 1);
            }
        }

        /// <summary>
        /// Fills a rectangle with a gradient.
        /// </summary>
        /// <param name="objgraphics">The graphics object on to which to draw the gradient fill.</param>
        /// <param name="R">A rectangle defining the bounds of the gradient fill.</param>
        /// <param name="C1">The color to start the gradient from.</param>
        /// <param name="C2">The color to finish the gradient.</param>
        public static void FillRectangleWithGradient(Graphics objgraphics, Rectangle R, Color C1, Color C2)
        {
            Point point1 = new Point();
            Point point2 = new Point();
            point1.X = R.X;
            point1.Y = R.Y;
            point2.X = R.Width;
            point2.Y = 0;
            Brush brush1 = new System.Drawing.Drawing2D.LinearGradientBrush(point1, point2, C1, C2);
            objgraphics.FillRectangle(brush1, R);
        }

        /// <summary>
        /// Draws an explorer bar on the given graphics object.
        /// </summary>
        /// <param name="graphics">The graphics object on to which to draw the explorer bar</param>
        /// <param name="text">The text to display on the explorer bar.</param>
        /// <param name="textColor">The preferred color of the text.</param>
        /// <param name="font">The font of the text.</param>
        /// <param name="size">The size of the explorer bar.</param>
        /// <param name="btoggle">Value indicating whether the bar should be toggled or not.</param>
        /// <param name="ButtonImages">Parameter required to render the explorer bar using images when visual styles are not enabled.</param>
        public static void DrawExplorerBar(Graphics graphics, string text, Color textColor, Font font, Rectangle size, bool btoggle, [Optional] ImageList ButtonImages /* = null */)
        {
            if (!VisualStylesEnabled())
            {
                textColor = SystemColors.ActiveCaptionText;
                Point point3 = new Point(size.Width - 0x16, 4);
                graphics.FillRectangle(SystemBrushes.ActiveCaption, size);
                int imgIndex = btoggle ? 1 : 0;
                graphics.DrawImage(ButtonImages.Images[imgIndex], point3);
            }
            else
            {

                Rectangle rectangle1 = new Rectangle(size.Width - 0x19, size.Y, 0x19, 0x19);
                int state = btoggle ? 7 : 6;
                DrawPrimitive(graphics, size, "ExplorerBar", 8, 1);
                DrawPrimitive(graphics, rectangle1, "ExplorerBar", state, 1);
            }

            using (Brush b = new SolidBrush(textColor))
            {
                graphics.DrawString(text, font, b, 5, 5);
            }
        }

        /// <summary>
        /// This property gives an appropriate border color
        /// for controls when visualstyles are enabled.
        /// </summary>
        public static Color ControlBorderColour
        {
            get
            {
                if (VisualStylesEnabled())
                {
                    return Color.FromArgb(127, 157, 185);
                }
                else
                {
                    return Color.Black;
                }
            }
        }


        // <summary>
        // Draws a list view header on the given graphics object.
        // </summary>
        // <param name="graphics">The Graphics object on which to draw the list view header.</param>
        // <param name="size">The size of the list view header.</param>
        // <param name="bHover">Whether to draw the listview header in hovering state or not.</param>
        public static void DrawListViewHeader(Graphics graphics, Rectangle size, bool bHover)
        {
            if (VisualStylesEnabled())
            {
                DrawPrimitive(graphics, size, "Header", 1, bHover ? 2 : 1);
            }
            else
            {
                if (size.Width > 0)
                {
                    ControlPaint.DrawButton(graphics, size, ButtonState.Normal);
                }
            }
        }

        // <summary>
        // Draws an explorer bar pane
        // </summary>
        // <param name="graphics">The graphics object on which to draw the explorer bar pane.</param>
        // <param name="size">The size of the explorer bar pane.</param>
        public static void DrawExplorerBarPane(Graphics graphics, Rectangle size)
        {
            if (VisualStylesEnabled())
            {
                DrawPrimitive(graphics, size, "ExplorerBar", 9, 1);
            }
        }

        // <summary>
        // Draws a primitive, which is a part of the windows ui defined by windows xp themes engine.
        // </summary>
        // <param name="graphics">The Graphics object on which to draw the primitive.</param>
        // <param name="rectangle">The bounding rectangle of the primitive.</param>
        // <param name="PartType">The PartType of the primitive.</param>
        // <param name="PartNo">The Part Number of the primitive.</param>
        // <param name="State">The State of the primitive.</param>
        public static void DrawPrimitive(Graphics graphics, Rectangle rectangle, string PartType, int PartNo, int State)
        {
            System.IntPtr hDC;
            IntPtr hTheme;
            RECT rButton;
            RECT rClip;

            rButton.x1 = rectangle.X;
            rButton.y1 = rectangle.Y;
            rButton.x2 = rectangle.X + rectangle.Width;
            rButton.y2 = rectangle.Y + rectangle.Height;

            rClip = rButton;

            hTheme = OpenThemeData(IntPtr.Zero, ref PartType);
            hDC = graphics.GetHdc();
            DrawThemeBackground(hTheme, hDC, PartNo, State, ref rButton, ref rClip);
            graphics.ReleaseHdc(hDC);
            CloseThemeData(hTheme);
        }

        #region "pInvoke"
        private struct RECT
        {
            public System.Int32 x1;
            public Int32 y1;
            public Int32 x2;
            public Int32 y2;
        }

        [DllImport("UxTheme.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DrawThemeBackground(IntPtr hTheme, IntPtr hDC, int partId, int stateId, ref RECT pRect, ref RECT pClipRect);
        [DllImport("UxTheme.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int IsThemeActive();
        [DllImport("UxTheme.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr OpenThemeData(IntPtr hWnd, [MarshalAs(UnmanagedType.VBByRefStr)] ref string classList);
        [DllImport("UxTheme.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void CloseThemeData(IntPtr hTheme);

        #endregion

    }
}
