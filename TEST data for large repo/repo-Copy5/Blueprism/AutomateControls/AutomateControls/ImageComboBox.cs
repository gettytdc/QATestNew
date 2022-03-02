using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace AutomateControls
{
    public class ImageComboBox:ComboBox
    {
        public ImageComboBox()
        {
            this.DrawMode = DrawMode.OwnerDrawVariable;
            this.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public override string Text
        {
            get
            {
                // If the currently selected item is a string return that
                // if it's an image with a string tag, return that.
                // otherwise, delegate to parent.
                object item = SelectedItem;
                
                if (item == null) 
                    return null;

                if (item is String)
                    return (string)item;

                if (item is Image)
                {
                    object tag = ((Image)item).Tag;
                    if (tag is String)
                        return (string)tag;
                }
                return base.Text;
            }
            set
            {
                // check the tags in the images (and the string values) to
                // see if they are the text we're looking for - if so, ensure
                // that the appopriate item is selected; otherwise, default
                // to parent's behaviour
                if (value != null)
                {
                    foreach (object item in Items)
                    {
                        if (item == null)
                            continue;

                        object testee = item;
                        if (item is Image)
                        {
                            testee = ((Image)item).Tag;
                        }

                        if (testee is String && ((string)testee).Equals(value))
                        {
                            this.SelectedItem = item;
                            return;
                        }
                    }
                }
                base.Text = value;
            }
        }

        /// <summary>
        /// Handles the drawing of the combo box item if it is either an image
        /// or a string.
        /// </summary>
        /// <param name="e">The event args for the draw item event. </param>
        protected override void OnDrawItem(DrawItemEventArgs ea)
        {
            ea.DrawBackground();
            ea.DrawFocusRectangle();
            try
            {
                object itemValue = this.Items[ea.Index];
                // Assume that the item is the display item..
                object displayValue = itemValue;
                // ... but if there is a DisplayMember set, use that instead.
                // Obviously, that can only be accessed if the item is not null
                if (itemValue != null && !string.IsNullOrEmpty(DisplayMember))
                {
                    Type t = itemValue.GetType();
                    PropertyInfo prop = t.GetProperty(DisplayMember);
                    if (prop != null)
                    {
                        displayValue = prop.GetValue(itemValue, null);
                    }
                    // at this point, we double check - a null display value is
                    // a nonsense, so we revert back to the item proper if that
                    // is the case
                    if (displayValue == null)
                        displayValue = itemValue;
                }
                Image image = displayValue as Image;
                string strValue = displayValue as string;
                if (image != null)
                {
                    int offset = Math.Max(0, (ea.Bounds.Height - image.Height) / 2);
                    ea.Graphics.DrawImage(image, ea.Bounds.Left + offset, ea.Bounds.Top + offset,
                        image.Width, image.Height);
                }
                else if (strValue != null)
                {
                    Size size = ea.Graphics.MeasureString(strValue, this.Font).ToSize();
                    int offset = Math.Max(0, (ea.Bounds.Height - size.Height) / 2);
                    ea.Graphics.DrawString(strValue, this.Font, new SolidBrush(ea.ForeColor),
                        ea.Bounds.Left + offset, ea.Bounds.Top + offset);
                }
                else
                {
                    base.OnDrawItem(ea);
                }
            }
            catch
            {
                ea.Graphics.FillRectangle(Brushes.Red, ea.Bounds);
            }
        }
    }
}
