namespace BluePrism.Caching
{
    using System;

    public interface ICache<in TKey, TValue> : IDisposable
        where TKey : IEquatable<TKey>
    {
        void SetValue(TKey key, TValue value);
        TValue GetValue(TKey key, Func<TValue> valueResolver);
        void Remove(TKey key);
        void Clear();
    }
}