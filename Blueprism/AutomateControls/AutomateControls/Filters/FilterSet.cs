using System;
using System.Collections.Generic;

namespace AutomateControls.Filters
{
    /// <summary>
    /// Class to represent a collection of filters.
    /// </summary>
    [Serializable]
    public class FilterSet : ICollection<Filter>
    {
        /// <summary>
        /// The filters represented by this set.
        /// </summary>
        private ICollection<Filter> _filters;

        /// <summary>
        /// Creates a new empty filter set.
        /// </summary>
        public FilterSet()
        {
            _filters = new List<Filter>();
        }

        /// <summary>
        /// The map of filter names to their current values.
        /// If the current value of a filter is null (ie. not filtering on
        /// that particular filter), it will not be included in this map.
        /// </summary>
        public IDictionary<String, Object> FilterMap
        {
            get
            {
                IDictionary<String, Object> map = new Dictionary<String, Object>();
                foreach (Filter f in this)
                {
                    FilterItem fi = f.SelectedFilterItem;
                    if (!FilterItem.IsNull(fi))
                        map[f.Name] = fi.Value;
                }
                return map;
            }
        }

        /// <summary>
        /// Gets the first filter found with the given name
        /// </summary>
        /// <param name="name">The name of the required filter</param>
        /// <returns>The filter found in this set with the specified name, or null
        /// if no such filter was found</returns>
        public Filter this[string name]
        {
            get
            {
                foreach (Filter f in _filters)
                {
                    if (f.Name == name)
                        return f;
                }
                return null;
            }
        }


        #region - ICollection<Filter> Members -

        /// <summary>
        /// Adds the given filter to this set.
        /// </summary>
        /// <param name="item"></param>
        public void Add(Filter item)
        {
            _filters.Add(item);
        }

        /// <summary>
        /// Clears all the filters from this set.
        /// </summary>
        public void Clear()
        {
            _filters.Clear();
        }

        /// <summary>
        /// Checks if this set contains the given filter.
        /// </summary>
        /// <param name="filter">The filter to check for.</param>
        /// <returns>True if this set contains the given filter, false otherwise.
        /// </returns>
        public bool Contains(Filter filter)
        {
            return _filters.Contains(filter);
        }

        /// <summary>
        /// Copies this filter set to the given array at the given index.
        /// </summary>
        /// <param name="array">The array to which this set should be copied.</param>
        /// <param name="arrayIndex">The index at which this set should begin
        /// copying its filters. </param>
        public void CopyTo(Filter[] array, int arrayIndex)
        {
            _filters.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// The number of filters in this set.
        /// </summary>
        public int Count
        {
            get { return _filters.Count; }
        }

        /// <summary>
        /// True if this set is read only, false otherwise.
        /// Normally true for this class.
        /// </summary>
        public bool IsReadOnly
        {
            get { return _filters.IsReadOnly; }
        }

        /// <summary>
        /// Removes the given filter from this set.
        /// </summary>
        /// <param name="f">The filter to remove.</param>
        /// <returns>true if the given filter was found and removed from this 
        /// set; false otherwise.</returns>
        public bool Remove(Filter f)
        {
            return _filters.Remove(f);
        }

        #endregion

        #region - IEnumerable<Filter> Members -

        /// <summary>
        /// Gets an enumerator over the filters in this set.
        /// </summary>
        /// <returns>An enumerator which enumerates the filters in this set.
        /// </returns>
        public IEnumerator<Filter> GetEnumerator()
        {
            return _filters.GetEnumerator();
        }

        #endregion

        #region - IEnumerable Members -

        /// <summary>
        /// Gets an enumerator over the filters in this set.
        /// </summary>
        /// <returns>An enumerator which enumerates the filters in this set.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
