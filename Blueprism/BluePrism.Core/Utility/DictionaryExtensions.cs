using System;
using System.Collections.Generic;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Extension methods for dictionary objects
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets an item from the dictionary for the given key. If the key does not exist
        /// the value is created, added to the dictionary and returned
        /// </summary>
        /// <typeparam name="TKey">The type of key</typeparam>
        /// <typeparam name="TValue">The type of value</typeparam>
        /// <param name="dictionary">The dictionary object</param>
        /// <param name="key">The key value</param>
        /// <param name="create">The function used to create the value if the key is not found</param>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TValue> create)
        {
            if (!dictionary.TryGetValue(key, out TValue value))
            {
                value = create();
                dictionary[key] = value;
            }
            return value;
        }

        /// <summary>
        /// Gets an item from the dictionary for the given key. If an item does not exist,
        /// then the default value for the value type is returned.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns>The value in the dictionary or default value for the type if it is
        /// not found in the dictionary</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetOrDefault(key, () => default(TValue));
        }

        /// <summary>
        /// Gets an item from the dictionary for the given key. If an item does not exist,
        /// then the specified default value is returned.
        /// </summary>
        /// <typeparam name="TKey">The type of key</typeparam>
        /// <typeparam name="TValue">The type of value</typeparam>
        /// <param name="dictionary">The dictionary object</param>
        /// <param name="key">The key value</param>
        /// <param name="default">The default value used if the key is not found</param>
        /// <returns>The value in the dictionary or the default value</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue @default)
        {
            return dictionary.GetOrDefault(key, () => @default);
        }

        /// <summary>
        /// Gets an item from the dictionary for the given key. If an item does not exist,
        /// then the specified function is used to create the value.
        /// </summary>
        /// <typeparam name="TKey">The type of key</typeparam>
        /// <typeparam name="TValue">The type of value</typeparam>
        /// <param name="dictionary">The dictionary object</param>
        /// <param name="key">The key value</param>
        /// <param name="create">The function used to create the value if the key is not found</param>
        /// <returns>The value in the dictionary or the default value</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TValue> create)
        {
            if (!dictionary.TryGetValue(key, out TValue value))
            {
                value = create();
            }
            return value;
        }
    }
}