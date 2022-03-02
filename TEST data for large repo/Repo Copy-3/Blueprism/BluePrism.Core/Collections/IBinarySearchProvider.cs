namespace BluePrism.Core.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Provides methods for performing binary searches on sorted collections
    /// </summary>
    public interface IBinarySearchProvider
    {
        /// <summary>
        /// Gets the index of the given item in the given collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="sortedCollection">The sorted collection.</param>
        /// <param name="item">The item to find.</param>
        /// <returns>The index of the item or <c>-1</c> if the item cannot be found.</returns>
        int IndexOf<T>(IList<T> sortedCollection, T item) where T : IComparable<T>;

        /// <summary>
        /// Gets the index of the given item in the given collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="sortedCollection">The sorted collection.</param>
        /// <param name="item">The item to find.</param>
        /// <param name="comparer">The comparer to use for items in the list.</param>
        /// <returns>The index of the item or <c>-1</c> if the item cannot be found.</returns>
        int IndexOf<T>(IList<T> sortedCollection, T item, IComparer comparer);

        /// <summary>
        /// Gets the index of the given item in the given collection.
        /// </summary>
        /// <param name="sortedCollection">The sorted collection.</param>
        /// <param name="item">The item to find.</param>
        /// <param name="comparer">The comparer to use for items in the list.</param>
        /// <returns>The index of the item or <c>-1</c> if the item cannot be found.</returns>
        int IndexOf(IList sortedCollection, object item, IComparer comparer);

        /// <summary>
        /// Gets the index of the given item in the given collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortedCollection">The sorted collection.</param>
        /// <param name="item">The item to find.</param>
        /// <param name="comparisonFunction">The comparison function.</param>
        /// <returns>The index of the item or <c>-1</c> if the item cannot be found.</returns>
        int IndexOf<T>(IList<T> sortedCollection, T item, Func<T, T, int> comparisonFunction);

        /// <summary>
        /// Gets the index at which the given item should be inserted into the collection to maintain sorting order.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="sortedCollection">The sorted collection.</param>
        /// <param name="item">The item which is to be inserted.</param>
        /// <returns>The index at which the item should be inserted in the collection to maintain sorting order.</returns>
        int InsertIndexOf<T>(IList<T> sortedCollection, T item) where T : IComparable<T>;

        /// <summary>
        /// Gets the index at which the given item should be inserted into the collection to maintain sorting order.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="sortedCollection">The sorted collection.</param>
        /// <param name="item">The item which is to be inserted.</param>
        /// <param name="comparer">The comparer to use for items in the list.</param>
        /// <returns>The index at which the item should be inserted in the collection to maintain sorting order.</returns>
        int InsertIndexOf<T>(IList<T> sortedCollection, T item, IComparer comparer);

        /// <summary>
        /// Gets the index at which the given item should be inserted into the collection to maintain sorting order.
        /// </summary>
        /// <param name="sortedCollection">The sorted collection.</param>
        /// <param name="item">The item which is to be inserted.</param>
        /// <param name="comparer">The comparer to use for items in the list.</param>
        /// <returns>The index at which the item should be inserted in the collection to maintain sorting order.</returns>
        int InsertIndexOf(IList sortedCollection, object item, IComparer comparer);
    }
}