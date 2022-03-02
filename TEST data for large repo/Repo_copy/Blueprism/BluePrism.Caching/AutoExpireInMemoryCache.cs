using System;
using System.Linq;
using System.Runtime.Caching;

namespace BluePrism.Caching
{
    public class AutoExpireInMemoryCache : IAutoExpireCache
    {
        #region Private Members

        private readonly string _name;
        private MemoryCache _cache;

        #endregion Private Members

        #region Constructor(s)

        public AutoExpireInMemoryCache() :
            this(null)
        {
        }

        public AutoExpireInMemoryCache(string name)
        {
            _name = name;
            CreateCache();
        }

        #endregion Constructor(s)

        #region IAutoExpireCache Implementation

        public void Clear()
        {
            if(_cache == MemoryCache.Default)
            {
                RemoveAll();
                return;
            }
            _cache?.Dispose();
            CreateCache();
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
            {
                return;
            }

            if (disposing)
            {
                if(_cache != MemoryCache.Default)
                {
                    _cache?.Dispose();
                }
            }

            _disposed = true;
        }

        ~AutoExpireInMemoryCache()
        {
            Dispose(false);
        }

        public object GetValue(string key, Func<object> valueResolver, int expirySeconds)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (expirySeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expirySeconds));
            }

            if (_cache.Contains(key))
            {
                return _cache[key];
            }

            _cache.Add(key, valueResolver(), CachePolicy(expirySeconds));
            return _cache[key];
        }

        public object GetValue(string key, Func<object> valueResolver) =>
            GetValue(key, valueResolver, 0);

        public void Remove(string key) =>
            _cache.Remove(key);

        public void SetValue(string key, object value) =>
            SetValue(key, value, 0);

        public void SetValue(string key, object value, int expirySeconds)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (expirySeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expirySeconds));
            }

            _cache.Remove(key);
            _cache.Add(key, value, CachePolicy(expirySeconds));
        }

        #endregion IAutoExpireCache Implementation

        #region Private Methods

        private void CreateCache() =>
            _cache = string.IsNullOrWhiteSpace(_name) ? MemoryCache.Default : new MemoryCache(_name);

        private static CacheItemPolicy CachePolicy(int expirySeconds) =>
            expirySeconds == 0 ? null : new CacheItemPolicy()
            { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expirySeconds) };

        private void RemoveAll()
        {
            foreach (var a in _cache.ToList())
            {
                _cache.Remove(a.Key);
            }
        }

        #endregion Private Methods
    }
}
