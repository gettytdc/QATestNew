namespace BluePrism.Core.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utilities.Functional;

    /// <summary>
    /// Contains extension methods for handling enumerables
    /// </summary>
    public static class EnumerableExtensionMethods
    {
        public static bool None<TSource>(this IEnumerable<TSource> source)
        {
            return source?.Any() != true;
        }

        /// <summary>
        /// Performs an action on corresponding items within 2 sequences. This forces
        /// enumeration of the sequences.
        /// </summary>
        /// <typeparam name="T1">The type of element in the first sequence</typeparam>
        /// <typeparam name="T2">The type of element in the first sequence</typeparam>
        /// <param name="first">The first sequence</param>
        /// <param name="second">The first sequence</param>
        /// <param name="action">The action to perform on each pair of items</param>
        public static void ZipEach<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second, Action<T1, T2> action)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            first.Zip(second, Tuple.Create)
                .ForEach(pair => action(pair.Item1, pair.Item2))
                .Evaluate();
        }

        /// <summary>
        /// Determines if all items in an enumerable are equal to and in the same order as
        /// the items in another enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the items in the enumerables.</typeparam>
        /// <param name="this">The first enumerable to compare.</param>
        /// <param name="other">The second enumerable to compare.</param>
        /// <returns><c>true</c> if the enumerables are equal; otherwise, <c>false</c>.</returns>
        public static bool ElementsEqual<T>(this IEnumerable<T> @this, IEnumerable<T> other)
            where T : IEquatable<T>
            =>
                @this.Zip(other, (x, y) => x.Equals(y)).All(x => x);
    }
}
