using System.Collections.Generic;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace AutomateControls.Filters
{
    /// <summary>
    /// Interface describing a filter definition.
    /// </summary>
    public interface IFilterDefinition
    {
        /// <summary>
        /// The name associated with the filter that this object describes
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Flag indicating whether the user is permitted to edit the
        /// filter value for the filter defined here.
        /// </summary>
        bool Editable { get; }

        /// <summary>
        /// Flag indicating whether this filter definition is represented
        /// by images, or by strings.
        /// </summary>
        bool RepresentedByImages { get; }

        /// <summary>
        /// The filter items which make up the filter defined here.
        /// </summary>
        ICollection<FilterItem> Items { get; }

        /// <summary>
        /// Parses the given filter item.
        /// </summary>
        /// <param name="txt">The text from which the filter item should be
        /// created.</param>
        /// <returns>A filter item, valid for this definition, which was
        /// created from the given string.</returns>
        FilterItem Parse(string txt);

        /// <summary>
        /// Generates a combo box for use in a filter which is defined by
        /// this object.
        /// </summary>
        /// <returns>A newly created combo box to use to represent a filter
        /// defined by this definition.</returns>
        ComboBox GenerateComboBox();

    }

    /// <summary>
    /// The basic filter definition which most implementations extend.
    /// </summary>
    public abstract class BaseFilterDefinition : IFilterDefinition
    {
        // The name for this filter.
        private string _name;

        // Whether this filter is editable or not.
        private bool _editable;

        /// <summary>
        /// Creates a new base editable filter definition with no name
        /// </summary>
        public BaseFilterDefinition() : this(null, true) { }

        /// <summary>
        /// Creates a new base filter definition with the given name and
        /// the specified editable-ness.
        /// </summary>
        /// <param name="name">The name of the filter represented by this
        /// definition.</param>
        /// <param name="editable">True if this filter should be editable
        /// or false otherwise.</param>
        public BaseFilterDefinition(string name, bool editable)
        {
            _name = name;
            _editable = editable;
        }

        /// <summary>
        /// The name of the filter represented by this definition.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// True if the filter represented by this definition is editable;
        /// False otherwise.
        /// </summary>
        public bool Editable
        {
            get { return _editable; }
        }

        /// <summary>
        /// True if filter terms described by this definition are typically
        /// represented by images; False otherwise.
        /// </summary>
        public virtual bool RepresentedByImages
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the collection of filter items that filters with this
        /// definition should display.
        /// </summary>
        public abstract ICollection<FilterItem> Items { get; }

        /// <summary>
        /// Parses the given text into a filter item.
        /// </summary>
        /// <param name="txt">The text to parse into a filter item.</param>
        /// <returns>A filter item representing the given text.</returns>
        public virtual FilterItem Parse(string txt)
        {
            if (FilterItem.IsNull(txt))
                return FilterItem.Empty;
            return new FilterItem(txt);
        }

        /// <summary>
        /// Creates an empty combo box which can be used by filters described
        /// by this definition to store and display filter items.
        /// </summary>
        /// <returns>A combo box for displaying and selecting filter items
        /// described by this filter definition</returns>
        protected virtual ComboBox CreateEmptyComboBox()
        {
            return new FilterComboBox();
        }

        /// <summary>
        /// Generates a new combo box containing the filter items that should
        /// be displayed by a filter described by this definition.
        /// </summary>
        /// <returns>A new combo box to use for filters described by this
        /// definition.</returns>
        public virtual ComboBox GenerateComboBox()
        {
            ComboBox box = CreateEmptyComboBox();
            if (!Editable)
            {
                box.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            box.DisplayMember = "DisplayValue";
            foreach (FilterItem item in Items)
            {
                box.Items.Add(item);
            }
            if (box.Items.Count > 0)
                box.SelectedIndex = 0;

            return box;
        }
    }

    /// <summary>
    /// A basic filter definition describing a filter which uses a string
    /// to filter items.
    /// </summary>
    public class StringFilterDefinition : BaseFilterDefinition
    {
        /// <summary>
        /// The items to display in filters using this definition.
        /// </summary>
        private ICollection<FilterItem> _items = new ReadOnlyCollection<FilterItem>(
            new FilterItem[] { new FilterItem() });

        /// <summary>
        /// Creates a new string filter definition with the given name.
        /// </summary>
        /// <param name="name">The name assigned to the filter using this
        /// definition.</param>
        public StringFilterDefinition(string name) : base(name, true) { }

        /// <summary>
        /// The collection of items to display for this filter definition.
        /// </summary>
        public override ICollection<FilterItem> Items
        {
            get { return _items; }
        }
    }

    /// <summary>
    /// A null filter definition, which only contains the null filter item and
    /// is not editable, implying that the columns using this filter cannot be
    /// searched.
    /// </summary>
    public class NullFilterDefinition : BaseFilterDefinition
    {
        /// <summary>
        /// The items to display for this filter definition. Just the null
        /// filter item for this definition.
        /// </summary>
        private ICollection<FilterItem> _items = new ReadOnlyCollection<FilterItem>(
            new FilterItem[] { new FilterItem() });

        /// <summary>
        /// Creates a new null filter definition with the given name.
        /// </summary>
        /// <param name="name">The name of the column being filtered on by a
        /// filter using this definition.</param>
        public NullFilterDefinition(string name) : base(name, false) { }

        /// <summary>
        /// Gets the item to use for this definition.
        /// </summary>
        public override ICollection<FilterItem> Items
        {
            get { return _items; }
        }
    }


}
