namespace BluePrism.Caching
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class InMemoryCache<TValue> : ICache<string, TValue>
    {
        protected IDictionary<string, TValue> Cache;

        public InMemoryCache()
        {
            Cache = new ConcurrentDictionary<string, TValue>();
        }

        public virtual void SetValue(string key, TValue value) =>
            Cache[key] = value;

        public virtual TValue GetValue(string key, Func<TValue> valueResolver) =>
            Cache.ContainsKey(key)
                ? Cache[key]
                : Cache[key] = valueResolver();

        public virtual void Remove(string key) =>
            Cache.Remove(key);

        public virtual void Clear()
        {
            Cache.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
        }

        ~InMemoryCache()
        {
            Dispose(false);
        }

    }
}