using System;

namespace BluePrism.Caching
{
    public interface IAutoExpireCache : ICache<string, object>
    {
        void SetValue(string key, object value, int expirySeconds);
        object GetValue(string key, Func<object> valueResolver, int expirySeconds);
    }
}
