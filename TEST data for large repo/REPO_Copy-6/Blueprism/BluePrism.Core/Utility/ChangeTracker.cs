using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// keep track of changes in winform controls
    /// </summary>
    public class ChangeTracker : IChangeTracker
    {
        private class Item
        {
            public Control Control { get; private set; }
            public string InitalValue { get; private set; }

            public Item(Control control,string initalValue)
            {
                Control = control;
                InitalValue = initalValue;
            }            
            public bool CompareValue() => GetControlValue(Control).CompareTo(InitalValue) == 0;
        }

        private readonly IDictionary<int, Item> _items = new Dictionary<int, Item>();
              
        public void RecordValue(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            if (!IsSupportedType(control))
            {
                throw new NotSupportedException($"the control {control.GetType().ToString()} is not supported");
            }

            var initalValue = GetControlValue(control);
            _items.Add(control.GetHashCode(), new Item(control, initalValue));            
        }

        /// <summary>
        /// loop through the control and see if the value has changed.
        /// </summary>
        /// <returns></returns>
        public bool HasChanged() => _items.Values.Any(i => !i.CompareValue());

        public void Reset() => _items.Clear();

        /// <summary>
        /// Get a comparison value to compare.
        /// </summary>
        /// <param name="control">control to check</param>
        /// <returns></returns>
        private static string GetControlValue(Control control)
        {
            if(control is TextBox)
            {
                return (control as TextBox).Text;
            }
            if(control is CheckBox)
            {
                return (control as CheckBox).Checked.ToString();
            }
            if(control is ComboBox)
            {
                return (control as ComboBox).Text;
            }
            if (control is ListBox)
            {
                return (control as ListBox).SelectedIndex.ToString();
            }
            if (control is NumericUpDown)
            {
                return (control as NumericUpDown).Value.ToString();
            }           
            if (control is RadioButton)
            {
                return (control as RadioButton).Checked.ToString();
            }
            throw new NotSupportedException($"the control {control.GetType().ToString()} is not supported");
        }

        /// <summary>
        /// Check the control to supported for comparison.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private static bool IsSupportedType(Control  control)
        {
            return (control is TextBox) || (control is CheckBox) || (control is ComboBox)
                || (control is ListBox) || (control is NumericUpDown) || (control is RadioButton);
        }
    }
}
