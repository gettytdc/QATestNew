using System;
using System.Collections.Generic;

namespace BluePrism.Core.Filters
{
    /// <summary>
    /// Basic filter class which currently is a very thin wrapper around a list of
    /// <see cref="FilterElement"/> objects.
    /// </summary>
    [Serializable]
    public class Filter : ICollection<FilterElement>
    {
        #region - Member Variables -

        // The wrapped collection of elements.
        private ICollection<FilterElement> _elems;

        #endregion

        #region - Properties -

        /// <summary>
        /// The wrapped collection of elements
        /// </summary>
        private ICollection<FilterElement> Elements
        {
            get
            {
                if (_elems == null)
                    _elems = new List<FilterElement>();
                return _elems;
            }
        }

        /// <summary>
        /// Gets a count of the number of filter elements present in this filter
        /// </summary>
        public int Count { get { return Elements.Count; } }

        /// <summary>
        /// Gets whether this filter is readonly or not; it is not.
        /// </summary>
        public bool IsReadOnly { get { return false; } }

        #endregion

        #region - Methods -

        /// <summary>
        /// Adds the given filter element to this filter.
        /// </summary>
        /// <param name="elem"></param>
        public void Add(FilterElement elem)
        {
            Elements.Add(elem);
        }

        /// <summary>
        /// Adds a filter element with the given attributes to this filter
        /// </summary>
        /// <param name="attrName">The name of the attribute to filter on</param>
        /// <param name="comp">The type of comparison to make</param>
        /// <param name="value">The value to compare the attribute value against,
        /// using the specified comparison to determine whether the item should pass
        /// the filter element or not.</param>
        public void Add(string attrName, FilterComparison comp, object value)
        {
            Add(new FilterElement()
            {
                AttributeName = attrName,
                Comparison = comp,
                Value = value
            });
        }

        /// <summary>
        /// Clears the filter
        /// </summary>
        public void Clear()
        {
            Elements.Clear();
        }

        /// <summary>
        /// Checks if this filter contains the given element
        /// </summary>
        /// <param name="item">The item to search for</param>
        /// <returns>true if this filter contains the given element, false otherwise.
        /// </returns>
        public bool Contains(FilterElement item)
        {
            return Elements.Contains(item);
        }

        /// <summary>
        /// Copies the contents of this filter into the given array.
        /// </summary>
        /// <param name="array">The array to copy into</param>
        /// <param name="arrayIndex">The index within the array to start copying.
        /// </param>
        public void CopyTo(FilterElement[] array, int arrayIndex)
        {
            Elements.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first instance of an element from this filter.
        /// </summary>
        /// <param name="item">The element to remove</param>
        /// <returns>true if the element was found and removed; false otherwise.
        /// </returns>
        public bool Remove(FilterElement item)
        {
            return Elements.Remove(item);
        }

        /// <summary>
        /// Gets an enumerator over this filter
        /// </summary>
        /// <returns>An enumerator over all the elements held in this filter.
        /// </returns>
        public IEnumerator<FilterElement> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator over this filter
        /// </summary>
        /// <returns>An enumerator over all the elements held in this filter.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
