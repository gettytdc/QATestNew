using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using BluePrism.Server.Domain.Models;

namespace AutomateControls.Filters
{
    /// <summary>
    /// Delegate called when a filter is applied
    /// </summary>
    /// <param name="source">The filter which is having a filter item applied.
    /// </param>
    /// <param name="args">The arguments detailing the filter item that has
    /// been changed to in the specified filter.</param>
    public delegate void FilterChangedHandler(Filter source, FilterChangingEventArgs args);

    /// <summary>
    /// Delegate called when a filter set is changed.
    /// </summary>
    /// <param name="source">The filter set which has changed.</param>
    /// <returns></returns>
    public delegate void FilterSetChangedHandler(FilterSet source);

    #region FilterChangingEventArgs

    /// <summary>
    /// The arguments detailing the change for the FilterItem.
    /// </summary>
    public class FilterChangingEventArgs : EventArgs
    {
        // Flag to indicate that the filter item is new or pre-existing
        private bool _isNew;
        // The item that the filter has been set to.
        private FilterItem _item;
        // Flag set by listeners to indicate that a filter term is valid or not.
        private bool _valid;

        /// <summary>
        /// Creates a new argument object for a filter changed event.
        /// </summary>
        /// <param name="item">The item to which the filter is being changed.
        /// </param>
        /// <param name="isNew">True to indicate that this filter item is new,
        /// ie. it does not already exist within the filter.</param>
        public FilterChangingEventArgs(FilterItem item, bool isNew)
        {
            _item = item;
            _isNew = isNew;
            _valid = true;
        }

        /// <summary>
        /// Gets the selected item that the filter is being changed to
        /// </summary>
        public FilterItem SelectedItem
        {
            get { return _item; }
        }

        /// <summary>
        /// Indicates whether the selected item is new or not, ie. whether the filter
        /// already contained the element or whether the user has entered it.
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
        }

        /// <summary>
        /// Flag for listeners to set to indicate that a new filter item is invalid.
        /// This is ignored if the item is not new, and defaults to being valid.
        /// If the item is valid when the event has completed, the item will be
        /// added to the filter and selected (without a further event).
        /// </summary>
        public bool Valid
        {
            get { return _valid; }
            set { _valid = true; }
        }
    }

    #endregion

    #region Filter

    /// <summary>
    /// A class to hold the data and UI elements required for a single filter.
    /// </summary>
    [Serializable]
    public class Filter
    {
        /// <summary>
        /// Event fired when this filter is changing.
        /// 
        /// </summary>
        /// <remarks>This event and any registered listeners are not serialized
        /// when the rest of this object is serialized.</remarks>
        [field:NonSerialized]
        public event FilterChangedHandler FilterChanging;

        /// <summary>
        /// The combo box used to represent this filter in the UI.
        /// </summary>
        /// <remarks>This combo box is not serialized when the rest of this object
        /// is serialized.</remarks>
        [NonSerialized]
        private ComboBox _combo;

        /// <summary>
        /// The definition of this filter. This provides the initial list of
        ///  filter items and can potentially provide a different type of
        ///  ComboBox in which the filter items are displayed.
        /// </summary>
        private IFilterDefinition _defn;

        /// <summary>
        /// The collection of filter items currently held in this filter.
        /// Note that filter items can be added to but not removed.
        /// </summary>
        private ICollection<FilterItem> _items;

        /// <summary>
        /// State flag to indicate that the filter is currently being changed.
        /// The combo box's selected index may change in the middle of a
        /// filter change, which would fire another filter-change event if 
        /// this flag were not set and checked.
        /// </summary>
        private bool _filterChanging;

        /// <summary>
        /// The last filter item registered by this class
        /// </summary>
        private FilterItem _lastFilterItem;

        /// <summary>
        /// Creates a new filter based on the given filter definition.
        /// </summary>
        /// <param name="defn">The filter definition to use to generate the
        /// UI view and filter items for this filter.</param>
        public Filter(IFilterDefinition defn)
        {
            _defn = defn;
            _combo = defn.GenerateComboBox();
            _items = new List<FilterItem>(defn.Items);

            _combo.SelectedIndexChanged += FilterIndexChanged;
            _combo.KeyDown += FilterKeyDown;
            _combo.Validated += FilterValidated;
            _combo.TextChanged += FilterTextChanged;

            _filterChanging = false;
            _lastFilterItem = SelectedFilterItem;
        }

        // Not sure if I need this constructor or not...
        //protected Filter(SerializationInfo info, StreamingContext ctx)
        //{
        //}

        /// <summary>
        /// Gets the name associated with this filter.
        /// </summary>
        public string Name
        {
            get { return _defn.Name; }
        }

        /// <summary>
        /// The UI view of this filter in the form of a combo box.
        /// </summary>
        public ComboBox Combo
        {
            get { return _combo; }
        }

        /// <summary>
        /// Gets the definition which describes this filter.
        /// </summary>
        public IFilterDefinition Definition
        {
            get { return _defn; }
        }

        public void Clear()
        {
            if (FilterTermString == "")
                return;
            _combo.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles the filter being validated
        /// </summary>
        void FilterValidated(object sender, EventArgs e)
        {
            HandleFilterChange();
        }

        /// <summary>
        /// Handles the text being changed on the filter combo - this really just
        /// handles the case when the text is set programmatically - ie. when the
        /// user is not currently in and changing the text. In that case, the filter
        /// isn't applied until the enter key is pressed.
        /// </summary>
        void FilterTextChanged(object sender, EventArgs e)
        {
            if (_combo.Focused) // we don't update the filters until enter is pressed
                return;
            HandleFilterChange();
        }

        /// <summary>
        /// Handles the keydown event being called on the combo box.
        /// This ensures that a filter-change event is fired when the user
        /// presses the Enter key.
        /// </summary>
        void FilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                HandleFilterChange();
            }
        }

        /// <summary>
        /// Handles the selected index being changed in the combo box.
        /// This ensures that a filter-change event is fired.
        /// </summary>
        void FilterIndexChanged(object sender, EventArgs e)
        {
            HandleFilterChange();
        }

        /// <summary>
        /// Handles a change of filter item within the filter.
        /// </summary>
        private void HandleFilterChange()
        {
            // If we're already in a filter-change event, don't do it again.
            if (_filterChanging)
                return;

            try
            {
                _filterChanging = true;
                if (FilterChanging != null)
                {
                    FilterItem item = this.SelectedFilterItem;

                    if (item == null && !string.IsNullOrEmpty(Combo.Text))
                    {
                        item = new FilterItem(Combo.Text);
                    }

                    // If this is the same item as before, there's no change, so
                    // no need to do anything
                    if (item == _lastFilterItem)
                        return;
                    _lastFilterItem = item;

                    bool isNew = !Combo.Items.Contains(item);
                    FilterChangingEventArgs args = new FilterChangingEventArgs(item, isNew);
                    FilterChanging(this, args);
                    if (args.Valid)
                    {
                        Combo.BackColor = Color.White;
                        if (isNew)
                        {
                            Add(item);
                            Combo.SelectedItem = item;
                            Combo.SelectionLength = 0;
                        }
                    }
                    else
                    {
                        Combo.BackColor = Color.LightPink;
                    }
                }
            }
            catch (InvalidFormatException ife)
            {
                MessageBox.Show("Error: {0}" + ife.Message);
            }
            finally
            {
                _filterChanging = false;
            }
        }

        /// <summary>
        /// Adds the given filter item to this filter if it doesn't already
        /// contain it.
        /// </summary>
        /// <param name="item">The item to add to this filter.</param>
        public virtual bool Add(FilterItem item)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);
                Combo.Items.Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the given term and item to this filter, if it is not
        /// already present.
        /// </summary>
        /// <param name="term">The filter term to apply to the new item
        /// </param>
        /// <param name="value"></param>
        public virtual bool Add(string term, object item)
        {
            return Add(new FilterItem(term, item));
        }

        /// <summary>
        /// Gets the currently selected filter item if one is currently
        /// selected.
        /// Note that if no item is selected (eg. if the user has entered
        /// a value in an editable filter), this will be null.
        /// </summary>
        public virtual FilterItem SelectedFilterItem
        {
            get
            {
                object selected = Combo.SelectedItem;
                FilterItem foundItem = null;
                if (selected != null)
                {
                    foreach (FilterItem item in _items)
                    {
                        if (object.Equals(selected, item))
                        {
                            foundItem = item;
                            break;
                        }
                    }
                }

                // If the found item exists and the display value matches that
                // in the combo box text -or- the combo box isn't editable,
                // then we have our item
                if (foundItem != null &&
                    (!_defn.Editable || object.Equals(foundItem.DisplayValue, Combo.Text)))
                    return foundItem;

                // It's not one in the combo box, if this filter's not editable,
                // then assume a null item
                if (!_defn.Editable)
                    return new FilterItem();

                // otherwise, parse it into a filter item.
                return _defn.Parse(Combo.Text);
            }
        }

        private string Normalise(string str)
        {
            // First escape the escape char, then escape the boundary chars and the
            // 'assigned' char. This is very much subject to change cos I hate it.
            return str
                .Replace("\\", "\\\\")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace(":", "\\:");
        }

        public string FilterTermString
        {
            get
            {
                FilterItem item = this.SelectedFilterItem;
                if (FilterItem.IsNull(item))
                    return "";
                // otherwise, return the term.
                return item.FilterTerm;
            }
        }

        public override string ToString()
        {
            // We really should have an item now.
            // If this is null, then return nada.
            if (FilterItem.IsNull(SelectedFilterItem))
                return "";

            // otherwise, return the term.
            return Normalise(Name) + ":" + Normalise(this.FilterTermString);
        }

    }
    #endregion

}
