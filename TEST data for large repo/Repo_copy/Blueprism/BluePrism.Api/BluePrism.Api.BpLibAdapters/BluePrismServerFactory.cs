namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Threading;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using Extensions;
    using AutomateAppCore.Config;
    using DatabaseInstaller;
    using Logging;

    public class BluePrismServerFactory : IBluePrismServerFactory
    {
        private readonly ConnectionSettingProperties _connectionSettingProperties;
        private readonly IOptions _options;
        private readonly ILogger _logger;
        private readonly Lazy<ServerManager> _serverManager;
        private static readonly Mutex _serverCreationMutex = new Mutex();

        static BluePrismServerFactory()
        {
            app.gAuditingEnabled = true;
        }

        public BluePrismServerFactory(
            ConnectionSettingProperties connectionSettingProperties,
            IOptions options,
            ILogger<BluePrismServerFactory> logger)
        {
            _connectionSettingProperties = connectionSettingProperties;
            _options = options;
            _logger = logger;

            _serverManager = new Lazy<ServerManager>(() => GetServerManager(connectionSettingProperties));
        }

        private ServerManager GetServerManager(ConnectionSettingProperties connectionSettingProperties)
        {
            _options.AddConnection(connectionSettingProperties.ToDbConnectionSetting());
            _logger.Debug("Database connection added to options");

            ServerManager serverManager;
            var initException = default(Exception);

            try
            {
                serverManager = ServerFactory.ClientInit(connectionSettingProperties.ToDbConnectionSetting(), ref initException);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "An error occurred closing an existing Blue Prism server connection: {0}", ex.Message);

                throw;
            }

            if (serverManager == null)
            {
                _logger.Fatal(initException, "An error occurred initialising the Blue Prism server manager: {0}", initException.Message);

                throw initException;
            }

            if (serverManager.Server == null)
            {
                var loggedException =
                    serverManager.LastConnectException is DatabaseInstallerException installerException && installerException.AssociatedException != null
                        ? installerException.AssociatedException
                        : serverManager.LastConnectException;

                _logger.Fatal(loggedException, "An error occurred creating a Blue Prism server object: {0}", loggedException.Message);

                throw loggedException;
            }

            _logger.Debug("Successfully created server manager");

            return serverManager;
        }

        public IServer ClientInit()
        {
            if (!_serverCreationMutex.WaitOne(TimeSpan.FromSeconds(30)))
                throw new TimeoutException("Timeout while waiting for server creation mutex");

            try
            {
                _logger.Debug("Initializing server client");

                var serverManager = _serverManager.Value;

                var unusedUser = default(IUser);
                serverManager.OpenConnection(_connectionSettingProperties.ToDbConnectionSetting(), null,
                    ref unusedUser);

                return serverManager.Server;
            }
            finally
            {
                _serverCreationMutex.ReleaseMutex();
            }
        }
    }
}
