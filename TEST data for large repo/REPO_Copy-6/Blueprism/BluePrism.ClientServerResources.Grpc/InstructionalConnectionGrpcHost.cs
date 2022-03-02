using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BluePrism.ClientServerResources.Core.Config;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Exceptions;
using BluePrism.ClientServerResources.Core.Interfaces;
using BluePrism.ClientServerResources.Grpc.Events;
using BluePrism.ClientServerResources.Grpc.Interfaces;
using BluePrism.ClientServerResources.Grpc.Properties;
using BluePrism.ClientServerResources.Grpc.Services;
using BluePrism.Common.Security;
using Grpc.Core;
using InstructionalConnection;
using NLog;

namespace BluePrism.ClientServerResources.Grpc
{
    public class InstructionalConnectionGrpcHost : IInstructionalHostController
    {
        private readonly Server _server;
        private readonly IInstructionalConnectionService _connection;
        private readonly IList<Guid> _connectedClients = new List<Guid>();
        private readonly ConnectionConfig _config;
        private bool _disposedValue;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public InstructionalConnectionGrpcHost(IInstructionalConnectionService instructionalConnectionService,
                                               IGrpcServiceFactory grpcServiceFactory, ConnectionConfig config)
        {
#if DEBUG
            global::System.Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
#endif
            try
            {
                Log.Info($"gRPC connection initialized");
                _config = config ?? throw new ArgumentException(nameof(config));
                _connection = instructionalConnectionService ?? throw new ArgumentException(nameof(instructionalConnectionService));
                _server = grpcServiceFactory?.CreateServer() ?? throw new ArgumentException(nameof(grpcServiceFactory));
                _connection.ClientRegistered += OnClientRegistered;
                _server.Services.Add(InstructionalConnectionService.BindService(_connection as InstructionalConnectionServiceImpl));

                var serverCredentials = ServerCredentials.Insecure;
                switch (_config.Mode)
                {
                    case InstructionalConnectionModes.None:
                        Log.Error($"Invalid instructional connection credential mode: None");
                        throw new InvalidInstructionalConnectionException(Resources.InstructionalClientGrpcController_CallbackConfigurationSecurityModeNotSet);
                    case InstructionalConnectionModes.Insecure:
                        break;
                    case InstructionalConnectionModes.Certificate:
                        serverCredentials = CreateServerCredentials(config);
                        break;                    
                    case InstructionalConnectionModes.Windows:
                        throw new NotImplementedException(string.Format(Resources.InstructionalClientGrpcController_AuthenticationModeIsNotSupportedByGrpc, _config.Mode));
                    default:
                        throw new InvalidInstructionalConnectionException(string.Format(Core.Properties.Resources.InvalidInstructionalConnectionMode, _config.Mode));
                }

                _server.Ports.Add(new ServerPort(config.HostName, config.Port, serverCredentials));
                _server.Start();

            }
            catch(Exception ex)
            {
                Log.Error($"grpc host initialistion error: {ex.GetType()} - {ex.Message}");
                throw new InvalidOperationException($"gRPC Startup Error - {ex.Message}");
            }
        }

        private ServerCredentials CreateServerCredentials(ConnectionConfig config)
        {
            try
            {
                var store = new CertificateStoreService();
                using (var cert = store.GetCertificateByName(
                    config.CertificateName,
                    config.ServerStore,
                    StoreLocation.LocalMachine))
                {
                    var keypair = new KeyCertificatePair(
                        cert.ToPemCertificate(),
                        cert.PrivateKeyToPkcs1());
                    return new SslServerCredentials(new[] { keypair });
                }
            }
            catch(Exception ex) when (ex is System.Security.Cryptography.CryptographicException || ex is InvalidOperationException)
            {
                Log.Error(ex, "Failed to create server certificate pair");
                throw new InvalidInstructionalConnectionException(Resources.PemCryptographicError, ex);

            }
            catch(Exception ex)
            {
                Log.Error(ex,"Failed to create server certificate pair");
                throw;
            }
        }

        public void ResourceStatusChanged(ResourcesChangedData data)
        {
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            _connection.EnqueueMessage(data);
            Log.Debug($"Enqueued Status Change");
        }

        public void SessionCreated(SessionCreatedData data)
        {
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            _connection.EnqueueMessage(data);
            Log.Debug($"Enqueued Session Created");
        }

        public void SessionDelete(SessionDeletedData data)
        {
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            _connection.EnqueueMessage(data);
            Log.Debug($"Enqueued Session Delete");
        }

        public void SessionEnd(SessionEndData data)
        {
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            _connection.EnqueueMessage(data);
            Log.Debug($"Enqueued Session End");
        }

        public void SessionStart(SessionStartedData data)
        {
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            _connection.EnqueueMessage(data);
            Log.Debug($"Enqueued Session Start");
        }

        public void SessionStop(SessionStopData data)
        {
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            _connection.EnqueueMessage(data);
            Log.Debug($"Enqueued Session Stop");
        }

        public void SessionVariablesUpdated(SessionVariablesUpdatedData data)
        {
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            _connection.EnqueueMessage(data);
        }

        private void OnClientRegistered(object sender, ClientRegisteredEventArgs e)
        {
            //expand this more when the comms are working.
            _connectedClients.Add(e.ClientId);
            Log.Debug($"Client {e.ClientId} was registered");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Log.Debug("gRpc connection disposed");
                    _connection.ClientRegistered -= OnClientRegistered;
                    _connection.Dispose();
                    _server.ShutdownAsync();
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override string ToString() => _config.ToString();
    }
}
