using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.DataGridViews
{
    public partial class StyledDataGridViewButtonCell : DataGridViewButtonCell
    {
        #region "Member Variables"

        private bool _focused;

        #endregion

        #region "Constructors"

        public StyledDataGridViewButtonCell()
        {
            InitializeComponent();
            FlatStyle = FlatStyle.Flat;
        }

        #endregion

        protected override void OnEnter(int rowIndex, bool throughMouseClick)
        {
            _focused = true;
        }

        protected override void OnLeave(int rowIndex, bool throughMouseClick)
        {
            _focused = false;
        }

        protected override void Paint(Graphics graphics,
            Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
            DataGridViewElementStates elementState, object value,
            object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            if (DataGridView.Focused && _focused)
            {
                cellStyle.Padding = new Padding(0);
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText,
                    cellStyle, advancedBorderStyle, paintParts);

                ControlPaint.DrawBorder(graphics, cellBounds,
                    ColourScheme.BluePrismControls.FocusColor, BorderWidth, ButtonBorderStyle.Solid,
                    ColourScheme.BluePrismControls.FocusColor, BorderWidth, ButtonBorderStyle.Solid,
                    ColourScheme.BluePrismControls.FocusColor, BorderWidth, ButtonBorderStyle.Solid,
                    ColourScheme.BluePrismControls.FocusColor, BorderWidth, ButtonBorderStyle.Solid);
                return;
            }

            base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText,
                cellStyle, advancedBorderStyle, paintParts);
        }

        [Description("Sets the width of the focus rectangle, too big will impact text wrapping"),
         Category("Accessibility"),
         DefaultValue(3),
         Browsable(true)]
        public int BorderWidth { get; set; } = 3;
    }
}
