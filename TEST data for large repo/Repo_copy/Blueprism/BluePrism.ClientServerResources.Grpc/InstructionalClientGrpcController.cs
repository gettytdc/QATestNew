using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.BPCoreLib.DependencyInjection;
using BluePrism.ClientServerResources.Core.Config;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Events;
using BluePrism.ClientServerResources.Core.Exceptions;
using BluePrism.ClientServerResources.Core.Interfaces;
using BluePrism.ClientServerResources.Grpc.Properties;
using BluePrism.Common.Security;
using Grpc.Core;
using InstructionalConnection;
using NLog;

namespace BluePrism.ClientServerResources.Grpc
{
    public class InstructionalClientGrpcController : IInstructionalClientController
    {
        private readonly string _address;
        private readonly ITokenRegistration _tokenRegisterer;
        private readonly Guid _clientId = Guid.NewGuid();
        private readonly RegisterClientCallback _registerClientCallback;
        private readonly ConnectionConfig _config;
        private IDisposable _reconnectTimer;
        private Task _callbackParentTask;
        private Channel _channel;
        private CancellationTokenSource _tokenSource;
        private InstructionalConnectionService.InstructionalConnectionServiceClient _instructionalConnectionServiceClient;
        private bool _disposedValue;
        private bool _manualReconnectOverride = false;
        private bool _isChannelDown = true;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public event EventHandler<ResourcesChangedEventArgs> ResourceStatus;
        public event EventHandler<SessionCreateEventArgs> SessionCreated;
        public event EventHandler<SessionDeleteEventArgs> SessionDeleted;
        public event EventHandler<SessionEndEventArgs> SessionEnd;
        public event EventHandler<SessionStartEventArgs> SessionStarted;
        public event EventHandler<SessionStopEventArgs> SessionStop;
        public event EventHandler<SessionVariableUpdatedEventArgs> SessionVariableUpdated;
        public event InvalidResponseEventHandler ErrorReceived;
        

        public InstructionalClientGrpcController(ConnectionConfig config)
        {
#if DEBUG
            global::System.Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
#endif
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _address = $"{_config.HostName}:{_config.Port}";
            Log.Debug($"gRpc Connection address: {_address}");

            _registerClientCallback = new RegisterClientCallback(KeepAliveTimeMS, KeepAliveTimeoutMS);

            _tokenRegisterer = DependencyResolver.Resolve<ITokenRegistration>();

            _channel = CreateChannel(_address);
            _instructionalConnectionServiceClient = new InstructionalConnectionService.InstructionalConnectionServiceClient(_channel);

            _registerClientCallback.ResourceStatus          += (s, e) => ResourceStatus?.Invoke(s, e);
            _registerClientCallback.SessionCreated          += (s, e) => SessionCreated?.Invoke(s, e);
            _registerClientCallback.SessionDeleted          += (s, e) => SessionDeleted?.Invoke(s, e);
            _registerClientCallback.SessionEnd              += (s, e) => SessionEnd?.Invoke(s, e);
            _registerClientCallback.SessionStop             += (s, e) => SessionStop?.Invoke(s, e);
            _registerClientCallback.SessionStarted          += (s, e) => SessionStarted?.Invoke(s, e);
            _registerClientCallback.SessionVariableUpdated  += (s, e) => SessionVariableUpdated?.Invoke(s, e);
            _registerClientCallback.ErrorReceived           += (s, e) => ErrorReceived?.Invoke(e);

            Log.Info($"gRPC connection initialized");
        }
        
        public int TokenTimeoutInSeconds { get; set; } = 10;
        public int ReconnectIntervalSeconds { get; set; } = 30;
        public int KeepAliveTimeMS { get; set; } = 15000;
        public int KeepAliveTimeoutMS { get; set; } = 10000;       

        private Channel CreateChannel(string address)
        {
            ChannelCredentials credentials;
            var options = new List<ChannelOption>() {
                new ChannelOption("grpc.keepalive_time_ms", KeepAliveTimeMS),
                new ChannelOption("grpc.keepalive_timeout_ms", KeepAliveTimeoutMS)
            };

            switch (_config.Mode)
            {
                case InstructionalConnectionModes.None:
                    throw new InvalidInstructionalConnectionException(Resources.InstructionalClientGrpcController_CallbackConfigurationSecurityModeNotSet);
                case InstructionalConnectionModes.Insecure:
                    Log.Debug("gRPC Connection Mode: Insecure");
                    credentials = ChannelCredentials.Insecure;
                    break;
                case InstructionalConnectionModes.Certificate:
                    // TLS handshake is the simplest form of auth gRPC supports:
                    // https://grpc.io/docs/guides/auth/#supported-auth-mechanisms
                    Log.Debug("gRPC Connection Mode: Certificate");
                    var cert = _config.ClientCertificate.ToCertificate();
                    credentials = new SslCredentials(cert.ToPemCertificate());
                    options.Add(
                        new ChannelOption(
                            ChannelOptions.DefaultAuthority,
                            cert.GetCertificateAuthority()));
                    break;
                case InstructionalConnectionModes.Windows:
                    throw new NotImplementedException(string.Format(Resources.InstructionalClientGrpcController_AuthenticationModeIsNotSupportedByGrpc, _config.Mode));
                default:
                    throw new InvalidInstructionalConnectionException(string.Format(Core.Properties.Resources.InvalidInstructionalConnectionMode, _config.Mode));
            }

            return new Channel(address, credentials, options);
        }

        private void OnFailedHandshake(Google.Protobuf.WellKnownTypes.Any message)
        {
            if (message.TryUnpack<FailedOperationMessage>(out var msg))
            {
                var statusCode = (RegisterClientStatusCode)msg.StatusCode;

                switch (statusCode)
                {
                    case RegisterClientStatusCode.ClientExists:
                        DeRegister();
                        ErrorReceived?.Invoke(new FailedCallbackOperationEventArgs(msg.Message, msg.ErrMsg));
                        break;
                    case RegisterClientStatusCode.StreamError:
                    case RegisterClientStatusCode.ResponseError:
                    case RegisterClientStatusCode.InvalidToken:
                        ErrorReceived?.Invoke(new FailedCallbackOperationEventArgs(msg.Message, msg.ErrMsg));
                        break;
                }
            }
        }

        private void OnStreamClosed(Task task)
        {
            _isChannelDown = true;
            if (task.IsFaulted)
            {
                Log.Info(task.Exception, "Callback client task errored.");
            }
            
            // Start the reconnect timer. Runs for x seconds or until manual intervention
            _manualReconnectOverride = false;
            _reconnectTimer = Observable.Interval(TimeSpan.FromSeconds(1))
                .TakeWhile(i => i <= ReconnectIntervalSeconds)
                .TakeUntil(i => _manualReconnectOverride)
                .Finally(Reconnect)
                .Subscribe();
        }

        private void Reconnect()
        {
            _reconnectTimer.Dispose();
            // Disposal of this class disposes of _reconnectTimer
            // triggering this Reconnect function. So exit it out if that has happened
            if (_channel.ShutdownToken.IsCancellationRequested)
            {
                return;
            }
            try
            {                
                if (_channel.State == ChannelState.Shutdown)
                {
                    _channel = CreateChannel(_address);
                    _instructionalConnectionServiceClient = new InstructionalConnectionService.InstructionalConnectionServiceClient(_channel);
                }
                RegisterClient();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "gRPC reconnect failed.");
                throw;
            }
        }

        public void RegisterClient()
        {
            var token = _tokenRegisterer.RegisterTokenWithExpiry(TokenTimeoutInSeconds);
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException(nameof(token));
            }
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();

            _callbackParentTask = Task.Run(async () =>
            {
                using (var stream = _instructionalConnectionServiceClient.RegisterClient(cancellationToken: _tokenSource.Token))
                {
                    Log.Debug("Attempting to connect gRPC channel");

                    _registerClientCallback.Attach(stream.ResponseStream, stream.RequestStream, _tokenSource.Token);

                    var initialResponse = await _registerClientCallback.HandshakeAsync(_clientId, token);

                    if (initialResponse.Success)
                    {
                        Log.Debug("gRPC channel successfully connected.");
                        _isChannelDown = false;
                        await _registerClientCallback.Run();
                        await stream.RequestStream.CompleteAsync();
                    }
                    else
                    {
                        Log.Debug($"gRPC handshake failed - {initialResponse.Error}");
                        OnFailedHandshake(initialResponse.Message);
                    }
                }
            }, _tokenSource.Token)
            .ContinueWith((t) => OnStreamClosed(t));
        }

        public async void DeRegister()
        {
            try
            {
                if (_instructionalConnectionServiceClient != null)
                {
                    var deRegisterResponse = await _instructionalConnectionServiceClient.DeRegisterAsync(new DeRegisterClientRequest()
                    {
                        ClientId = _clientId.ToString()
                    });

                    if(!deRegisterResponse.Success)
                    {
                        Log.Debug($"Failed to deregister client from gRPC callback channel. {deRegisterResponse.Error}");
                    }
                }
            }
            catch (RpcException)
            {
                // No action required if channel no longer expects this
            }         
        }
               
        public void EnsureConnected()
        {
            // set the flag to kick off a reconnect attempt
            _manualReconnectOverride = true;

            if (_isChannelDown) {
                throw new InvalidInstructionalConnectionException();
            }
        }

        public void Connect()
        {
        }

        public void Close()
        {
            // cancel listener thread
            _tokenSource.Cancel();
            _channel.ShutdownAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Close();
                    _tokenSource?.Dispose();
                    _reconnectTimer?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
