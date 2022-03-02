#if UNITTESTS

namespace BluePrism.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AutomateAppCore;
    using Utilities.Testing;
    using Utilities.Functional;
    using Data;
    using NUnit.Framework;
    using BluePrism.DatabaseInstaller;
    using BluePrism.Common.Security;
    using BluePrism.BPCoreLib;

    [Category("Database Test")]
    public abstract class DatabaseTestBase : IntegrationTestBase
    {
        protected readonly string DatabaseName = $"test_{Guid.NewGuid().ToString().Replace("-", "")}";

        protected readonly Lazy<clsDBConnectionSetting> BluePrismDatabaseConnectionSetting;

        private const string ConnectionString = @"Server=(LocalDB)\MSSQLLocalDB; Integrated Security=true;";

        private bool _hasCreatedDatabase;

        private static bool _triedExtendedTimeout = false;

        protected DatabaseTestBase()
        {
            BluePrismDatabaseConnectionSetting =
                new Lazy<clsDBConnectionSetting>(() =>
                    new clsDBConnectionSetting(
                        DatabaseName,
                        @"(LocalDB)\MSSQLLocalDB",
                        DatabaseName,
                        "",
                        new SafeString(""),
                        true));
        }

        public override void OneTimeSetup()
        {
            base.OneTimeSetup();
            WaitForServer();
        }

        static void WaitForServer()
        {
            try
            {
                // Only use an extended timeout the first time this method runs to give
                // MSSQLLocalDB a chance to wake up. It should then fail fast so that we
                // don't delay feedback when connection errors are caused by a genuine outage.
                bool extendTimeout = !_triedExtendedTimeout;
                TestServerConnection(extendTimeout);
            }
            finally
            {
                _triedExtendedTimeout = true;
            }
        }

        private static void TestServerConnection(bool extendTimeout)
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            if (extendTimeout)
            {
                builder.ConnectTimeout = 60;
            }
            string testConnectionString = builder.ConnectionString;
            using (var connection = new SqlConnection(testConnectionString))
            {
                connection.Open();
                connection.ChangeDatabase("master");
                using (var command = new SqlCommand("select 1;", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void TearDown()
        {
            base.TearDown();

            if(_hasCreatedDatabase)
            {
                try
                {
                    ExecuteQuery(
                        $"alter database [{DatabaseName}] set single_user with rollback immediate; drop database [{DatabaseName}]",
                        _ => { },
                        databaseName: "master");
                }
                catch { /* ignored */ }
            }
        }

        protected void CreateDatabase(int version)
        {
            var errorString = default(string);

            var scriptLoader = new DatabaseScriptLoader(new EmbeddedResourceLoader());
            var connectionFactory = new SqlConnectionFactory();
            var wrapper = new DatabaseScriptGenerator(scriptLoader);

            new Installer(BluePrismDatabaseConnectionSetting.Value.CreateSqlSettings(),
                    TimeSpan.FromMinutes(15),
                    ApplicationProperties.ApplicationName,
                    clsServer.SingleSignOnEventCode,
                    (ISqlDatabaseConnectionSetting s, TimeSpan t) => new Database(connectionFactory, scriptLoader, s, t),
                    wrapper,
                    new SqlConnectionWrapper(),
                    scriptLoader)
                .CreateDatabase(null, true, false, version);

            if (!string.IsNullOrEmpty(errorString))
                Assert.Fail($"Database creation failed {errorString}");

            Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(x => x.EndsWith(".sql"))
                .Select(Assembly.GetExecutingAssembly().GetManifestResourceStream)
                .Select(x => new StreamReader(x).Use(reader => reader.ReadToEnd()))
                .ForEach(x =>
                {
                    try
                    {
                        ExecuteQuery(x, _ => { });
                    }
                    catch { /* ignored */ }
                })
                .Evaluate();

            _hasCreatedDatabase = true;
        }

        protected IDatabaseConnection GetDatabaseConnection() =>
            new clsDBConnection(BluePrismDatabaseConnectionSetting.Value);

        protected void ExecuteQuery(string query, Action<IDbCommand> commandSetup, string databaseName = null) =>
            GetCommandContext(
                query,
                databaseName ?? DatabaseName,
                commandSetup,
                command => command.ExecuteNonQuery());

        protected void ExecuteQuery<T>(string query, Action<IDbCommand> commandSetup, out T result, string databaseName = null) =>
            result =
                GetCommandContext(
                    query,
                    databaseName ?? DatabaseName,
                    commandSetup,
                    command => (T) command.ExecuteScalar());

        protected void ExecuteQuery(string query, Action<IDbCommand> commandSetup, Action<IDataRecord> rowHandler, string databaseName = null) =>
            ExecuteQuery(query, commandSetup, x =>
            {
                rowHandler(x);
                return 0;
            });

        protected IReadOnlyCollection<T> ExecuteQuery<T>(string query, Action<IDbCommand> commandSetup, Func<IDataRecord, T> rowHandler, string databaseName = null) =>
            GetCommandContext(
                query,
                databaseName ?? DatabaseName,
                commandSetup,
                command =>
                {
                    using (var dataReader = command.ExecuteReader())
                    {
                        var values = new List<T>();
                        while (dataReader.Read())
                        {
                            values.Add(rowHandler(dataReader));
                        }

                        return values;
                    }
                });

        protected static object NullOrValue(object item) =>
            item == DBNull.Value ? null : item;

        protected static Func<IDataRecord, T> GetValue<T>(string name) => row =>
            (T)(row.GetValue(row.GetOrdinal(name)).Map(NullOrValue) ?? default(T));

        protected static Action<IDbCommand> AddParameter(string name, object value) => command =>
            command.Parameters.Add(CreateParameter(command, name, value));

        protected static IDbDataParameter CreateParameter(IDbCommand command, string name, object value) =>
            command.CreateParameter()
                .Tee(x => x.ParameterName = name)
                .Tee(x => x.Value = value);

        protected static TryResult<TOut> Try<TOut>(Func<TOut> func) =>
            new TryResult<TOut>(func);

        private static T GetCommandContext<T>(string query, string databaseName, Action<IDbCommand> commandSetup, Func<IDbCommand, T> runInContext)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                connection.ChangeDatabase(databaseName);
                using (var command = new SqlCommand(query, connection))
                {
                    commandSetup(command);
                    return runInContext(command);
                }
            }
        }

        protected static IInstaller CreateInstaller(IDatabaseConnectionSetting setting, IDatabaseScriptLoader loader)
        {
            var connectionFactory = new SqlConnectionFactory();
            var wrapper = new DatabaseScriptGenerator(loader);

            return new Installer(
                setting.CreateSqlSettings(),
                TimeSpan.FromSeconds(120),     //real databaseinstallcommandtimeout default is 15min, 2s too small - gives random timeouts in testing
                ApplicationProperties.ApplicationName,
                clsServer.SingleSignOnEventCode,
                (ISqlDatabaseConnectionSetting s, TimeSpan t) => new Database(connectionFactory, new DatabaseScriptLoader(new EmbeddedResourceLoader()), s, t),
                wrapper,
                new SqlConnectionWrapper(),
                loader);
        }

        protected static IInstaller CreateInstaller(IDatabaseConnectionSetting setting)
            => CreateInstaller(setting, new DatabaseScriptLoader(new EmbeddedResourceLoader()));
    }

    public class TryResult<T>
    {
        private readonly Func<T> _statement;
        private readonly IReadOnlyDictionary<Type, T> _catchTypes = new Dictionary<Type, T>();

        public TryResult(Func<T> statement) =>
            _statement = statement;

        private TryResult(Func<T> statement, IReadOnlyDictionary<Type, T> catchTypes)
            : this(statement) =>
            _catchTypes = catchTypes;

        public TryResult<T> On<TException>(T value) where TException : Exception =>
            new TryResult<T>(_statement, _catchTypes.Concat(new[] { new KeyValuePair<Type, T>(typeof(TException), value)}).ToDictionary(x => x.Key, x => x.Value));

        public T Value
        {
            get
            {
                try
                {
                    return _statement();
                }
                catch (Exception ex)
                {
                    return
                        _catchTypes.ContainsKey(ex.GetType())
                        ? _catchTypes[ex.GetType()]
                        : throw ex;
                }
            }
        }
    }
}

#endif