using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace AutomateControls
{
    /// <summary>
    /// Combo Box which is allows selection of colours from a predefined list of
    /// available colours (ie. those set in the ComboBox.Items collection).
    /// </summary>
    public class ColorComboBox : ComboBox
    {
        /// <summary>
        /// Creates a new, empty, ColorComboBox
        /// </summary>
        public ColorComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        /// <summary>
        /// The currently selected colour in this combo box
        /// </summary>
        public Color SelectedColor
        {
            get
            {
                object item = SelectedItem;
                if (item == null || !(item is Color))
                    return Color.Empty;
                return (Color)item;
            }
            set
            {
                SelectedItem = value;
            }
        }

        /// <summary>
        /// DropDownStyle of this combo box - it only really works for a drop down
        /// list so make it difficult to change (not impossible, of course, but if
        /// someone does change it, strange things will happen).
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ComboBoxStyle DropDownStyle
        {
            get { return base.DropDownStyle; }
            set { base.DropDownStyle = value; }
        }

        /// <summary>
        /// DrawMode of this combo box - it only really works for a OwnerDraw value
        /// so make it difficult to change (not impossible, of course, but if someone
        /// does change it... well, I imagine you'll end up with random nonsense in
        /// the combo box items so.. don't).
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DrawMode DrawMode
        {
            get { return base.DrawMode; }
            set { base.DrawMode = value; }
        }

        /// <summary>
        /// Draws the combo box item defined in the given args
        /// </summary>
        /// <param name="e">The args detailing the item to draw</param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            int ind = e.Index;
            if (ind < 0 || ind >= Items.Count)
                return;
            object item = Items[ind];
            if (!(item is Color))
                return;

            e.DrawBackground();

            Color col = (Color)item;

            Rectangle rect = e.Bounds;
            rect.Inflate(-3, -2);
            rect.Y--;
            using (Brush b = new SolidBrush(col))
            {
                e.Graphics.FillRectangle(b, rect);
                e.Graphics.DrawRectangle(Pens.Gray, rect);
            }
        }
    }
}
