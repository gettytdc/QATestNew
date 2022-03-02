using System;
using AutomateControls.Filters;
using System.Windows.Forms;

namespace AutomateControls
{
    /// <summary>
    /// ComboBox designed for use with <see cref="FilterItem"/> items
    /// </summary>
    public class FilterComboBox : ComboBox
    {
        /// <summary>
        /// Overrides the text property, coping with the text being set when the
        /// 'DisplayMember' is set for FilterItems. This could probably be
        /// extrapolated out into a more general fix for the problem, but that's
        /// not what I was aiming for here.
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                foreach (object itemObject in Items)
                {
                    FilterItem item = itemObject as FilterItem;
                    if (item == null)
                        continue;
                    string str = item.DisplayValue as string;
                    if (str != null && str == value)
                    {
                        // The base class handles it fine when the text exists
                        // in the combo box.. so leave it to that
                        base.Text = value;
                        return;
                    }
                }

                // However, it doesn't handle it so well when the text *isn't* in
                // the combo box, so set the selected index to -1 first.
                // See http://support.microsoft.com/kb/814346 for an explanation of
                // a different symptom, but probably the same bug.
                SelectedIndex = -1;
                base.Text = value;
            }
        }


        /// <summary>
        /// Ensures that the combo box being resized doesn't cause the text of the
        /// combo box to be selected.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!Focused)
                base.SelectionLength = 0;
        }
    }
}
