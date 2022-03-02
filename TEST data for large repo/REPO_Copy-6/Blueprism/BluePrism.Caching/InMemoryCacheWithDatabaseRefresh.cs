namespace BluePrism.Caching
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    using Utilities.Functional;

    using Data;

    public class InMemoryCacheWithDatabaseRefresh<TValue> : InMemoryCache<TValue>, IRefreshCache<string, TValue> 
    {
        private const string GetCacheETagStoredProcedureName = "usp_GetCacheETag";
        private const string SetCacheETagStoredProcedureName = "usp_SetCacheETag";

        private readonly string _databaseRefreshKey;
        private readonly int _refreshIntervalInMilliseconds;
        private readonly Func<string, IDbCommand> _storedProcedureCommandFactory;
        private readonly Func<IDbConnection> _databaseConnectionFactory;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Guid _entityTag = Guid.Empty;

        public event OnRefreshRequiredDelegate OnRefreshRequired;

        public InMemoryCacheWithDatabaseRefresh(
            string databaseRefreshKey,
            string connectionString,
            int refreshIntervalInMilliseconds,
            Func<string, IDbConnection> databaseConnectionFactory,
            Func<string, CommandType, IDbCommand> databaseCommandFactory)
        {
            _databaseRefreshKey = databaseRefreshKey;
            _refreshIntervalInMilliseconds = refreshIntervalInMilliseconds;

            _databaseConnectionFactory = () => databaseConnectionFactory(connectionString);
            _storedProcedureCommandFactory = 
                storedProcedureName => databaseCommandFactory(storedProcedureName, CommandType.StoredProcedure);

            _cancellationTokenSource = new CancellationTokenSource();
            StartDatabaseRefreshTask(_cancellationTokenSource.Token);
        }

        public override void Clear()
        {
            base.Clear();
            _databaseConnectionFactory()
                .Tee(x => x.Open())
                .Use(SetEntityTag(Guid.NewGuid()));
        }

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _cancellationTokenSource.Cancel();

            _disposed = true;
            base.Dispose(disposing);
        }


        private void StartDatabaseRefreshTask(CancellationToken cancellationToken) =>
            new TaskFactory(cancellationToken, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, null)
                .StartNew(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            CheckForTagUpdate();
                        }
                        catch(Exception)
                        {
                            await Task.Delay(30000, cancellationToken);
                        }

                        await Task.Delay(_refreshIntervalInMilliseconds, cancellationToken);
                    }
                });

        private void CheckForTagUpdate()
        {
            using (var connection = _databaseConnectionFactory())
            {
                connection.Open();
                var currentTag = GetCurrentEntityTag(connection);

                if (!currentTag.Equals(_entityTag))
                    UpdateEntityTag(currentTag, connection);
            }
        }

        private void UpdateEntityTag(Guid newEntityTag, IDbConnection connection)
        {
            _entityTag = newEntityTag;
            base.Clear();
            SetEntityTag(connection, _entityTag);
            Task.Run(() => OnRefreshRequired?.Invoke(this, null));
        }

        private Guid GetCurrentEntityTag(IDbConnection connection) =>
            _storedProcedureCommandFactory(GetCacheETagStoredProcedureName)
                .AddParameter("@cacheKey", _databaseRefreshKey)
                .SetConnection(connection)
                .Use(x => (Guid)x.ExecuteScalar())
                .Map(x => x.Equals(Guid.Empty) ? Guid.NewGuid() : x);

        private Action<IDbConnection> SetEntityTag(Guid tag) => connection =>
            SetEntityTag(connection, tag);

        private void SetEntityTag(IDbConnection connection, Guid tag) =>
            _storedProcedureCommandFactory(SetCacheETagStoredProcedureName)
                .AddParameter("@cacheKey", _databaseRefreshKey)
                .AddParameter("@tag", tag)
                .SetConnection(connection)
                .Use(x => x.ExecuteNonQuery());
    }
}