using System.Collections;
using System.Linq;

namespace BluePrism.Core.Collections
{
    using System;
    using System.Collections.Generic;

    /// <inheritdoc />
    public class BinarySearchProvider : IBinarySearchProvider
    {
        /// <inheritdoc />
        public int IndexOf<T>(IList<T> sortedCollection, T item) where T : IComparable<T> =>
            IndexOf(sortedCollection, item, Comparer<T>.Default);

        /// <inheritdoc />
        public int IndexOf<T>(IList<T> sortedCollection, T item, IComparer comparer) =>
            IndexOf(sortedCollection, item, (x, y) => comparer.Compare(x, y));

        /// <inheritdoc />
        public int IndexOf(IList sortedCollection, object item, IComparer comparer) =>
            IndexOf<object>(sortedCollection.OfType<object>().ToList(), item, comparer);

        /// <inheritdoc />
        public int IndexOf<T>(IList<T> sortedCollection, T item, Func<T, T, int> comparisonFunction)
        {
            var searchMaxIndex = sortedCollection.Count;
            var searchMinIndex = 0;

            while (searchMinIndex < searchMaxIndex)
            {
                var searchIndex = (searchMinIndex + searchMaxIndex) / 2;
                var compareResult = comparisonFunction(item, sortedCollection[searchIndex]);

                if (compareResult < 0)
                    searchMaxIndex = searchIndex;
                else if (compareResult > 0)
                    searchMinIndex = searchIndex + 1;
                else
                    return searchIndex;
            }

            return -1;
        }


        /// <inheritdoc />
        public int InsertIndexOf<T>(IList<T> sortedCollection, T item) where T : IComparable<T> =>
            InsertIndexOf(sortedCollection, item, Comparer<T>.Default);

        /// <inheritdoc />
        public int InsertIndexOf<T>(IList<T> sortedCollection, T item, IComparer comparer) =>
            InsertIndexOf((IList) sortedCollection, item, comparer);

        /// <inheritdoc />
        public int InsertIndexOf(IList sortedCollection, object item, IComparer comparer)
        {
            var searchMaxIndex = sortedCollection.Count;
            var searchMinIndex = 0;
            var searchIndex = 0;

            // Special case where either the collection is empty or the item to be inserted should
            // be at the start of the collection.
            if (sortedCollection.Count == 0 || comparer.Compare(item, sortedCollection[0]) < 0)
                return 0;

            while (searchMinIndex<searchMaxIndex)
            {
                searchIndex = (searchMinIndex + searchMaxIndex) / 2;
                var compareResult = comparer.Compare(item, sortedCollection[searchIndex]);

                if (compareResult< 0)
                    searchMaxIndex = searchIndex;
                else if (compareResult > 0)
                    searchMinIndex = searchIndex + 1;
                else
                    return searchIndex;
            }

            return searchIndex + 1;
        }
    }
}