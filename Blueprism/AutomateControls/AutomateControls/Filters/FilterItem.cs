using AutomateControls.Properties;
namespace AutomateControls.Filters
{
    public class FilterItem
    {
        /// <summary>
        /// A filter item which represents 'no filtering' - ie. view "All".
        /// </summary>
        public static FilterItem Empty { get; set; } = new FilterItem();

        /// <summary>
        /// A localized filter item which represents 'no filtering' - ie. view "All".
        /// </summary>
        public FilterItem() : this("All", "All", Resources.FilterItem_All)
        {
        }

        /// <summary>
        /// Checks the given object to see if it represents a null filter, ie.
        /// no filter at all.
        /// This will be the case if the value is null, or matches the
        /// NULL_FILTER_ITEM's Item value, or if it is an empty string, or a
        /// string which matches NULL_FILTER_ITEM's FilterTerm value.
        /// Otherwise (ie. it's not a string, or a non-empty string other than 
        /// NULL_FILTER_ITEM's term), this will return false.
        /// </summary>
        /// <param name="value">true if the given value would normally be
        /// interpreted as a null filter item; false otherwise</param>
        /// <returns></returns>
        public static bool IsNull(object value)
        {
            if (value == Empty)
            {
                return true;
            }

            if (value == null || value.Equals(Empty.Value))
            {
                return true;
            }

            if (value == null || value.Equals(Empty))
            {
                return true;
            }

            string sval = value as string;
            if (sval == null) // it's not a string - it cannot be the null filter item.
            {
                return false;
            }

            if (sval.Length == 0)
            {
                return true;
            }

            if (sval.Equals(Empty.FilterTerm))
            {
                return true;
            }

            // A string, not equal to "", or "All" - thus, not the null filter.
            return false;
        }

        // The term for this filter item
        private readonly string _term;

        /// <summary>
        /// The object used to display,  - this is what is held in the
        // underlying combo box.
        /// </summary>
        private readonly object _display;

        /// <summary>
        /// The value for the filter item. This is what is used programmatically
        /// when the filter is applied.
        /// </summary>
        private readonly object _value;

        /// <summary>
        /// Creates a new filter item based on the given term, all of the values are
        /// set to the given term (such that the combo box will display the term and
        /// the underlying value will be the given string term).
        /// </summary>
        /// <param name="term">The term representing this filter item.</param>
        public FilterItem(string term) : this(term, term, term) { }

        /// <summary>
        /// Creates a new filter item based on the given term with the given underlying
        /// value. The display term - ie. that shown by the combo box will be the
        /// string term rather than the underlying value.
        /// </summary>
        /// <param name="term">The string term representing this filter term.</param>
        /// <param name="value">The underlying value associated with this filter term.
        /// </param>
        public FilterItem(string term, object value) : this(term, value, term) { }

        /// <summary>
        /// Creates a new filter item with the given values.
        /// </summary>
        /// <param name="term">The term that represents this item.</param>
        /// <param name="value">The underlying value which is made available to the
        /// code when this filter item is applied to a filter. This is not necessarily
        /// human-readable.</param>
        /// <param name="displayValue">The display value object - this is what is held
        /// inside the combo box, and thus needs be humand readable - usually either a
        /// string or an image, but as long as the object's ToString() returns something
        /// that is useful to the user, anything goes.</param>
        public FilterItem(string term, object value, object displayValue)
        {
            _term = term;
            _display = displayValue;
            _value = value;
        }

        /// <summary>
        /// The display value, used to represent this filter term in a filter combo box.
        /// </summary>
        public object DisplayValue
        {
            get { return _display; }
        }

        /// <summary>
        /// The underlying value that this filter term represents.
        /// </summary>
        public object Value
        {
            get { return _value; }
        }

        /// <summary>
        /// The string representing this filter term.
        /// </summary>
        public string FilterTerm
        {
            get { return _term; }
        }

        /// <summary>
        /// Checks if this term is equal to the given object.
        /// </summary>
        /// <param name="obj">The object to check for equality against.</param>
        /// <returns>True if the given object represents the same filter term
        /// as this object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            FilterItem item = obj as FilterItem;
            if (item != null)
            {
                return string.Equals(this.FilterTerm, item.FilterTerm) &&
                    object.Equals(this.DisplayValue, item.DisplayValue) &&
                    object.Equals(this.Value, item.Value);
            }
            return false;
        }

        /// <summary>
        /// Gets a hash representing this filter item.
        /// </summary>
        /// <returns>A hash of this filter item</returns>
        public override int GetHashCode()
        {
            // shift a bit or 2 because they may be the same objects, and we don't
            // want to wipe them out when we XOR them together.
            return (
                _term.GetHashCode() ^
                (_value.GetHashCode() << 2) ^
                (_display.GetHashCode() << 4)
            );
        }

        /// <summary>
        /// Gets a string representation of this filter item
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.FilterTerm;
        }
    }

}
