namespace BluePrism.Caching
{
    public interface ICacheFactory
    {
        IAutoExpireCache GetInMemoryAutoExpireCache(string name);

        ICache<string, TValue> GetInMemoryCache<TValue>(string name);

        IRefreshCache<string, TValue> GetInMemoryCacheWithDatabaseRefresh<TValue>(
            string databaseRefreshKey,
            string connectionString,
            int refreshIntervalInMilliseconds);
    }
}