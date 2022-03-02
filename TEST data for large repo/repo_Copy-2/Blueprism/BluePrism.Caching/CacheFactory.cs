namespace BluePrism.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public class CacheFactory : ICacheFactory
    {
        private readonly Func<string, IDbConnection> _databaseConnectionFactory;
        private readonly Func<string, CommandType, IDbCommand> _databaseCommandFactory;
        private readonly IDictionary<string, object> _caches = new Dictionary<string, object>();

        public CacheFactory(
            Func<string, IDbConnection> databaseConnectionFactory,
            Func<string, CommandType, IDbCommand> databaseCommandFactory)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
            _databaseCommandFactory = databaseCommandFactory;
        }

        public IAutoExpireCache GetInMemoryAutoExpireCache(string name) =>
            new AutoExpireInMemoryCache(name);

        public ICache<string, TValue> GetInMemoryCache<TValue>(string name) =>
            new InMemoryCache<TValue>();

        public IRefreshCache<string, TValue> GetInMemoryCacheWithDatabaseRefresh<TValue>(
            string databaseRefreshKey,
            string connectionString,
            int refreshIntervalInMilliseconds)
            =>
                (IRefreshCache<string, TValue>)(
                    _caches.ContainsKey(databaseRefreshKey)
                        ? _caches[databaseRefreshKey]
                        : _caches[databaseRefreshKey] =
                            new InMemoryCacheWithDatabaseRefresh<TValue>(
                                databaseRefreshKey,
                                connectionString,
                                refreshIntervalInMilliseconds,
                                _databaseConnectionFactory,
                                _databaseCommandFactory));
    }
}