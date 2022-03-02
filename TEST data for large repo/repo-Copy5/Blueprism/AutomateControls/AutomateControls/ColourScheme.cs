using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls
{

    public class ColourScheme
    {
        #region "Default"

        public class Default
        {

            public static Color TitleBarColor
            {
                get
                {
                    if (Application.RenderWithVisualStyles)
                        return EnvironmentBackColor;

                    return SystemColors.ActiveCaption;
                }
            }

            public static Color TitleBarText
            {
                get
                {
                    if (Application.RenderWithVisualStyles)
                        return Color.White;

                    return SystemColors.ActiveCaptionText;
                }
            }

            /// <summary>
            /// Gets the default background colour in a Blue Prism environment
            /// </summary>
            public static Color EnvironmentBackColor
            {
                get
                {
                    return Color.FromArgb(13, 42, 72);
                }
                private set { }

            }

            /// <summary>
            /// The value of a default Blue Prism Environment Scheme Background Color 
            /// as a constant string representation of the R,G,B components
            /// </summary>
            public const string EnvironmentBackColorRGB = "13, 42, 72";


            /// <summary>
            /// Gets the default foreground colour in a Blue Prism environment
            /// </summary>
            public static Color EnvironmentForeColor
            {
                get
                {
                    // Color.White but without the name - we want to ensure that this works
                    // in Equals() checks for dynamically generated colours
                    return Color.FromArgb(255, 255, 255);
                }
                private set { }

            }


            public static readonly Color StatusBarColor = TitleBarColor;
            public static readonly Color StatusBarText = TitleBarText;

            public static readonly Color TaskPaneBackColor = Color.White;
            public static readonly Color TaskPaneLink = Color.Black;
            public static readonly Color TaskPaneLinkDisabled = Color.LightSteelBlue;

            public static readonly Color TaskPaneTitleText = Color.Black;

            public static readonly Color TabPublishedSubSheet = Color.SandyBrown;
            public static readonly Color ListViewSelectedRowOutline = Color.Red;

            public static readonly Color ListViewSelectedRowBackground = Color.FromArgb(182, 202, 234);
            public static readonly Color ListViewElementDragDropHighlightOuter = Color.Transparent;
            public static readonly Color ListViewElementDragDropHighlightInner = Color.Transparent;
            public static readonly Color ListViewDataStoreInDragDropHighlightOuter = Color.Transparent;
            public static readonly Color ListViewDataStoreInDragDropHighlightInner = Color.Transparent;
            public static readonly Color ListViewExpressionDataDragDropHighlightOuter = Color.Transparent;

            public static readonly Color ListViewExpressionDataDragDropHighlightInner = Color.Transparent;
            public static readonly Color[] GraphColours = {
                Color.Green,
                Color.Yellow,
                Color.Blue,
                Color.Red,
                Color.Plum,
                Color.PowderBlue,
                Color.Pink,
                Color.SandyBrown

            };
            public static readonly Color ClockHourHand = Color.Black;
            public static readonly Color ClockMinuteHand = Color.Black;
            public static readonly Color ClockSecondHand = Color.LightBlue;

            public static readonly Color ClockHighlightHand = Color.Lavender;
            //blue prism logo light blue
            public static readonly Color PrintBorder = Color.FromArgb(255, 33, 106, 173);

            //Orange
            public static readonly Color UndoMenuHighLightBackground = Color.FromArgb(255, 238, 194);

            public static readonly Color UndoMenuHighLightBorder = Color.Black;
        }

        #endregion

        #region "ProcessMI"

        public class ProcessMI
        {

            public static readonly Color DecisionTrueMajor = Color.Green;
            //Light green
            public static readonly Color DecisionTrueMinor = Color.FromArgb(204, 255, 204);
            public static readonly Color DecisionFalseMajor = Color.Red;
            public static readonly Color DecisionFalseMinor = Color.MistyRose;
            public static readonly Color Calculation = Color.Blue;
            public static readonly Color MultipleCalculation = Color.Cyan;
            public static readonly Color Action = Color.DarkGray;
            public static readonly Color Process = Color.DarkOrange;
            public static readonly Color SubSheet = Color.DarkTurquoise;
            public static readonly Color Choice = Color.DarkMagenta;
            public static readonly Color Start = Color.Coral;
            public static readonly Color End = Color.DarkCyan;
            public static readonly Color Alert = Color.DarkCyan;
            public static readonly Color Code = Color.DarkKhaki;
            public static readonly Color Read = Color.DarkSeaGreen;
            public static readonly Color Write = Color.DeepPink;
            public static readonly Color Navigate = Color.DeepSkyBlue;
            public static readonly Color Wait = Color.Goldenrod;
            public static readonly Color Exception = Color.IndianRed;
            public static readonly Color Recover = Color.PeachPuff;

            public static readonly Color Resume = Color.LightCoral;
        }

        #endregion

        public class BluePrismControls
        {
            #region LightTheme
            public static readonly Color FocusColor = Color.FromArgb(255, 195, 0);
            public static readonly Color HoverColor = Color.FromArgb(184, 201, 216);
            public static readonly Color DisabledForeColor = Color.FromArgb(97, 115, 129);
            public static readonly Color DisabledBackColor = Color.FromArgb(212, 212, 212);
            public static readonly Color BluePrismBlue = Color.FromArgb(11, 117, 183);
            public static readonly Color ForeColor = Color.FromArgb(67, 74, 79);
            public static readonly Color TextColor = Color.Black;
            public static readonly Color MouseLeaveColor = Color.White;
            #endregion
        }


        #region "Expression"

        public class Expression
        {

            public static readonly Color Text = Color.Green;
            public static readonly Color Number = Color.Blue;
            public static readonly Color DataItem = Color.Magenta;
            public static readonly Color Operator = Color.Red;
            public static readonly Color Parameter = Color.Gray;
            public static readonly Color[] Brackets = {
                Color.Yellow,
                Color.Purple,
                Color.Cyan,
                Color.Brown,
                Color.Orange,
                Color.Black

            };
        }

        #endregion
    }
}

