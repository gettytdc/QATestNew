using System;
using System.Collections.Generic;

namespace BluePrism.Core
{
    /// <summary>
    /// Provides comparer and equality comparer implementations based on delegates,
    /// so they can be created inline in the code without having to create separate
    /// classes each time.
    /// </summary>
    public static class Compare
    {
        /// <summary>
        /// Gets an equality comparer which compares on a function of a class, rather
        /// than on the class itself.
        /// </summary>
        /// <typeparam name="T">The type of object being compared</typeparam>
        /// <typeparam name="TId">The type representing the identity of the class on
        /// which the comparer will operate</typeparam>
        /// <param name="idGetter">The function which is used to retrieve the
        /// identity of the object, used for the equality comparison</param>
        /// <returns>An IEqualityComparer which uses the given function to provide
        /// the value on which two objects should be compared for equality.</returns>
        public static
            IEqualityComparer<T> EqualityComparer<T, TId>(Func<T, TId> idGetter)
        {
            return new DelegateEqualityComparer<T, TId>(idGetter);
        }

        /// <summary>
        /// Class to represent an equality comparer which operates on a function of
        /// the class that it is comparing.
        /// </summary>
        /// <typeparam name="T">The type of object being compared</typeparam>
        /// <typeparam name="TId">The type representing the identity of the class on
        /// which the comparer will operate</typeparam>
        /// <param name="idGetter">The function which is used to retrieve the
        /// identity of the object, used for the equality comparison</param>
        private class DelegateEqualityComparer<T, TId> : IEqualityComparer<T>
        {
            // The retriever of the identity to use for equality comparisons
            private readonly Func<T, TId> _idGetter;

            /// <summary>
            /// Creates a new equality comparer based on the identity of an object
            /// retrieved using a delegate
            /// </summary>
            /// <param name="idGetter">The function which is used to retrieve the
            /// identity of the object, used for the equality comparison</param>
            public DelegateEqualityComparer(Func<T, TId> idGetter)
            {
                _idGetter =
                    idGetter ?? throw new ArgumentNullException(nameof(idGetter));
            }

            /// <summary>
            /// Tests if the two objects are equal, using the the identity as
            /// provided by the idGetter set in the constructor of this object.
            /// </summary>
            /// <param name="x">The first object to test</param>
            /// <param name="y">The second object to test</param>
            /// <returns>True if the identities retrieved by the getter given in
            /// construction of this object are equal for the two objects; false
            /// otherwise.</returns>
            public bool Equals(T x, T y)
            {
                return Equals(_idGetter(x), _idGetter(y));
            }

            /// <summary>
            /// Gets the hashcode of the identity of the given object.
            /// </summary>
            /// <param name="obj">The object for which the identity hashcode is
            /// required.</param>
            /// <returns>An integer hash of the identity of the given object, as
            /// provided by the identity getter given in construction of this
            /// comparer.</returns>
            public int GetHashCode(T obj)
            {
                TId id = _idGetter(obj);
                return (id == null ? 0 : id.GetHashCode());
            }
        }

        /// <summary>
        /// Gets an IComparer based on a supplied delegate, used for the actual
        /// comparison.
        /// </summary>
        /// <typeparam name="T">The type of object to compare</typeparam>
        /// <param name="comp">
        /// A comparer which takes two instances of the comparable object and returns
        /// a signed integer indicating their relative value. The return value should
        /// fall into the appropriate one of the following categories:
        /// Less than zero: x is less than y.
        /// Zero: x equals y.
        /// Greater than zero: x is greater than y.
        /// </param>
        /// <returns></returns>
        public static IComparer<T> Comparer<T>(Func<T, T, Int32> comp)
        {
            return new DelegateComparer<T>(comp);
        }

        /// <summary>
        /// Class which uses a delegate to compare two values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class DelegateComparer<T> : IComparer<T>
        {
            // Function used to compare two instances of the comparable object
            private Func<T, T, Int32> _comparer;

            /// <summary>
            /// Creates a new comparer which delegates the actual comparison to a
            /// supplied delegate.
            /// </summary>
            /// <param name="comparer">The comparer function to compare the two
            /// objects and return the value as expected by IComparer.Compare
            /// </param>
            public DelegateComparer(Func<T, T, Int32> comparer)
            {
                _comparer =
                    comparer ?? throw new ArgumentNullException(nameof(comparer));
            }

            /// <summary>
            /// Compares two objects, returning an indicator of their relative value.
            /// </summary>
            /// <param name="x">The first object to compare</param>
            /// <param name="y">The second object to compare</param>
            /// <returns>
            /// A signed integer that indicates the relative values of x and y, from
            /// the following:
            /// Less than zero: x is less than y.
            /// Zero: x equals y.
            /// Greater than zero: x is greater than y.
            /// </returns>
            public int Compare(T x, T y)
            {
                return _comparer(x, y);
            }
        }

    }
}
