using System.Collections.Generic;
using System.Windows.Forms;
using BluePrism.CharMatching.Properties;
using System.Drawing;
using System.ComponentModel;
using BluePrism.BPCoreLib.Collections;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Class representing a button which is used to fire 'Shift' events on a
    /// <see cref="Shifter"/> control
    /// </summary>
    public class ShifterButton : Button
    {
        #region - Class Scope Declarations -

        /// <summary>
        /// Data representing the text and image that the button displays to the user
        /// </summary>
        private class HelperData
        {
            // The text detailing the function represented by this button
            private string _txt;
            // The image shown on this button to represent the function
            private Image _img;

            /// <summary>
            /// Creates a new HelperData object with the given help text and image
            /// </summary>
            public HelperData(string helpText, Image img)
            {
                _txt = helpText;
                _img = img;
            }

            /// <summary>
            /// The help text for the button
            /// </summary>
            public string Text { get { return _txt; } }

            /// <summary>
            /// The image representing the function that this button allows
            /// </summary>
            public Image Image { get { return _img; } }
        }

        /// <summary>
        /// Map of the helper data assigned to each shift operation supported by this
        /// class.
        /// </summary>
        private static readonly IDictionary<ShiftOperation, HelperData>
            ShiftOperationSchema = InitHelpData();

        /// <summary>
        /// Initialises the map of helper data supported by this class.
        /// </summary>
        /// <returns></returns>
        private static IDictionary<ShiftOperation, HelperData> InitHelpData()
        {
            IDictionary<ShiftOperation, HelperData> map =
                new Dictionary<ShiftOperation, HelperData>();

            map[ShiftOperation.PadTop] =
                new HelperData(Resources.PadTop_helpText, Resources.padtop);
            map[ShiftOperation.PadRight] =
                new HelperData(Resources.PadRight_helpText, Resources.padright);
            map[ShiftOperation.PadBottom] =
                new HelperData(Resources.PadBottom_helpText, Resources.padbottom);
            map[ShiftOperation.PadLeft] =
                new HelperData(Resources.PadLeft_helpText, Resources.padleft);

            map[ShiftOperation.TrimTop] =
                new HelperData(Resources.TrimTop_helpText, Resources.trimtop);
            map[ShiftOperation.TrimRight] =
                new HelperData(Resources.TrimRight_helpText, Resources.trimright);
            map[ShiftOperation.TrimBottom] =
                new HelperData(Resources.TrimBottom_helpText, Resources.trimbottom);
            map[ShiftOperation.TrimLeft] =
                new HelperData(Resources.TrimLeft_helpText, Resources.trimleft);

            map[ShiftOperation.ShiftUp] =
                new HelperData(Resources.ShiftUp_helpText, Resources.shiftup);
            map[ShiftOperation.ShiftRight] =
                new HelperData(Resources.ShiftRight_helpText, Resources.shiftright);
            map[ShiftOperation.ShiftDown] =
                new HelperData(Resources.ShiftDown_helpText, Resources.shiftdown);
            map[ShiftOperation.ShiftLeft] =
                new HelperData(Resources.ShiftLeft_helpText, Resources.shiftleft);

            map[ShiftOperation.None] = new HelperData(Resources.NoOperation, null);

            return GetReadOnly.IDictionary(map);
        }

        #endregion

        #region - Member Variables -

        // The operation that this button represents
        private ShiftOperation _op;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new shifter button representing the given operation
        /// </summary>
        /// <param name="op">The operation that should be represented by this
        /// button</param>
        public ShifterButton(ShiftOperation op)
        {
            this.Size = new Size(18, 18);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.MouseOverBackColor = Color.White;
            this.FlatAppearance.BorderColor = SystemColors.ButtonShadow;
            this.Operation = op;
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint, true);
            
        }

        /// <summary>
        /// Creates a new shifter button, not currently representing a shift
        /// operation
        /// </summary>
        public ShifterButton() : this(ShiftOperation.None) { }

        #endregion

        #region - Properties -

        /// <summary>
        /// The shift operation that this button represents
        /// </summary>
        public ShiftOperation Operation
        {
            get { return _op; }
            set
            {
                _op = value;
                Image = ShiftOperationSchema[value].Image;
            }
        }

        /// <summary>
        /// The tooltip text for this button
        /// </summary>
        public string TooltipText
        {
            get
            {
                HelperData hd;
                if (ShiftOperationSchema.TryGetValue(_op, out hd))
                    return hd.Text;
                return null;
            }
        }

        /// <summary>
        /// Hide the Image property from the designer - it's entirely dependent
        /// on the shift operation represented by this button
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image Image
        {
            get { return base.Image; }
            set { base.Image = value; }
        }

        #endregion

    }
}
