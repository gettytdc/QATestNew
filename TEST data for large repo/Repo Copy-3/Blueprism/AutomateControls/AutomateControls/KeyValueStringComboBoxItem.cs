using System;
using System.Collections.Generic;

namespace AutomateControls
{

    /**
     * Helper class when dealing with combo boxes that are just being fed a collection of strings
     * and we need to localise with a display string
     * 
     */

    public class KeyValueStringComboBoxItem : IComparable<object>
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public KeyValueStringComboBoxItem(string newValue, string newText)
        {
            Value = newValue;
            Text = newText;
        }
        public KeyValueStringComboBoxItem(string newValue)
        {
            Value = newValue;
            Text = newValue;
        }

        public int CompareTo(object other)
        {
            if (other is KeyValueStringComboBoxItem)
                return Value.CompareTo(((KeyValueStringComboBoxItem)other).Value);
            else
                return -1;
        }

        public override string ToString()
        {
            return Text;
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (other is string)
                return Value.Equals(other);

            if (GetType() != other.GetType())
                return false;

            return Value.Equals(((KeyValueStringComboBoxItem)other).Value);
        }

        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
        }
    }
}

