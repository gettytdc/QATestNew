using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AutomateControls
{
    /// <summary>
    /// Binding list which sorts its items as long as they implement the
    /// IComparable interface
    /// </summary>
    /// <typeparam name="T">The type of object within the list.</typeparam>
    public class SortedBindingList<T>: BindingList<T>
    {
        // The state flag indicating if the list is currently sorting itself
        private bool _sorting;

        // The property descriptor for the property of the held type that
        // should be compared in order to sort this list
        private PropertyDescriptor _sortProp;

        /// <summary>
        /// Creates a new, empty sorted binding list
        /// </summary>
        public SortedBindingList() : this(null) { }

        /// <summary>
        /// Creates a new empty sorted binding list which uses the specified
        /// property on held objects for its sort value
        /// </summary>
        /// <param name="prop">The property descriptor to access on held
        /// elements to determine the sorting order of the list. If null,
        /// this will attempt to cast the held element itself into an
        /// IComparable and use that directly.</param>
        public SortedBindingList(PropertyDescriptor prop)
        {
            _sorting = false;
            _sortProp = prop;
        }

        /// <summary>
        /// Override property indicating that this list supports sorting
        /// </summary>
        protected override bool SupportsSortingCore { get { return true; } }

        /// <summary>
        /// Override property indicating that this list is sorted
        /// </summary>
        protected override bool IsSortedCore { get { return true; } }

        /// <summary>
        /// Override implementing the sorting for this list.
        /// </summary>
        /// <param name="prop">The descriptor of the property to sort on</param>
        /// <param name="direction">The direction on which to sort</param>
        protected override void ApplySortCore(
            PropertyDescriptor prop, ListSortDirection direction)
        {
            _sorting = true;
            int modifier = direction == ListSortDirection.Ascending ? 1 : -1;
            bool oldRaiseVal = RaiseListChangedEvents;
            RaiseListChangedEvents = false;

            try
            {
                if (prop == null)
                {
                    if (typeof(T).GetInterface("IComparable") != null)
                    {
                        List<T> items = new List<T>(Items);
                        items.Sort();
                        Items.Clear();
                        foreach (T i in items)
                            Items.Add(i);
                    }
                }
                else if (prop.PropertyType.GetInterface("IComparable") != null)
                {
                    List<T> items = new List<T>(this.Items);
                    items.Sort(new Comparison<T>(delegate(T a, T b) {
                        IComparable aVal = prop.GetValue(a) as IComparable;
                        IComparable bVal = prop.GetValue(b) as IComparable;
                        return aVal.CompareTo(bVal) * modifier;
                    }));
                    Items.Clear();
                    foreach (T i in items)
                        Items.Add(i);
                }
            }
            finally
            {
                RaiseListChangedEvents = oldRaiseVal;
                ResetBindings();
                _sorting = false;
            }
        }

        /// <summary>
        /// The property accessed on underlying objects which provides the
        /// sort order for this list. Null indicates that the sorting will
        /// just use the natural order of the held items to sort the list.
        /// </summary>
        public PropertyDescriptor SortProperty
        {
            get { return _sortProp; }
            set { _sortProp = value; }
        }

        /// <summary>
        /// Handles this list being changed by ensuring that it is sorted
        /// immediately after a change.
        /// </summary>
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (!_sorting)
                ((IBindingList) this).ApplySort(_sortProp, ListSortDirection.Ascending);
            base.OnListChanged(e);
        }
    }

}
