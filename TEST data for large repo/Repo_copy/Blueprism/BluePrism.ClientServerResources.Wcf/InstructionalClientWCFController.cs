using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.ClientServerResources.Core.Config;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Events;
using BluePrism.ClientServerResources.Core.Exceptions;
using BluePrism.ClientServerResources.Core.Interfaces;
using BluePrism.ClientServerResources.Wcf.Endpoints;
using BluePrism.ClientServerResources.Wcf.Properties;
using BluePrism.Common.Security;
using NLog;

namespace BluePrism.ClientServerResources.Wcf
{
    public class InstructionalClientWCFController : IInstructionalClientController
    {
        private const int DisconnectWait = 20000;
        private const int ReconnectWait = 5000;

        private readonly Guid _clientId = Guid.NewGuid();
        private readonly ITokenRegistration _tokenRegisterer;
        private bool _disposedValue = false;
        private string _address;
        private readonly INotificationServiceCallBack _callbackservice = new NotificationServiceCallBack();
        private readonly ConnectionConfig _config;
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _registerEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _unregisterEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _checkEvent = new AutoResetEvent(false);
        private INotificationServices _notificationServices;
        private ChannelFactory<INotificationServices> _duplexChannelFactory;
        private bool _clientRegistered = false;
        private bool _isChannelDown = true;
        private Task _registerClientTask;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public event EventHandler<ResourcesChangedEventArgs> ResourceStatus;
        public event EventHandler<SessionCreateEventArgs> SessionCreated;
        public event EventHandler<SessionDeleteEventArgs> SessionDeleted;
        public event EventHandler<SessionEndEventArgs> SessionEnd;
        public event EventHandler<SessionStartEventArgs> SessionStarted;
        public event EventHandler<SessionStopEventArgs> SessionStop;
        public event EventHandler<SessionVariableUpdatedEventArgs> SessionVariableUpdated;
        public event InvalidResponseEventHandler ErrorReceived;

        public CallbackConnectionStatus CallbackChannelStatus { get; private set; }

        public InstructionalClientWCFController(ConnectionConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _tokenRegisterer = BPCoreLib.DependencyInjection.DependencyResolver.Resolve<ITokenRegistration>();

            ConfigServerAddress();
            _callbackservice.ResourceStatus         += (s, e) => ResourceStatus?.Invoke(s, e);
            _callbackservice.SessionCreated         += (s, e) => SessionCreated?.Invoke(s, e);
            _callbackservice.SessionDeleted         += (s, e) => SessionDeleted?.Invoke(s, e);
            _callbackservice.SessionEnd             += (s, e) => SessionEnd?.Invoke(s, e);
            _callbackservice.SessionStop            += (s, e) => SessionStop?.Invoke(s, e);
            _callbackservice.SessionStarted         += (s, e) => SessionStarted?.Invoke(s, e);
            _callbackservice.SessionVariableUpdated += (s, e) => SessionVariableUpdated?.Invoke(s, e);
        }

        public bool IsConnected => _notificationServices is object;
        public int TokenTimeoutInSeconds { get; set; }
        public int ReconnectIntervalSeconds { get; set; }
        public int KeepAliveTimeMS { get; set; }
        public int KeepAliveTimeoutMS { get; set; }

        private void ConfigServerAddress()
        {
            var protocol = "http";
            _address = $"{protocol}://{_config.HostName}:{_config.Port}/{ConnectionConfig.EndpointName}";
            Log.Debug($"WCF Callback Connection Config : {_config.HostName}, {_config.Port}, {_config.Mode}");
        }

        public void Connect()
        {
            if (IsConnected)
            {
                throw new InvalidOperationException(Resources.InstructionalClientWCFController_AlreadyConneted);
            }
            Log.Debug($"WCF Callback Connect");
            CreateChannel();
            _resetEvent.WaitOne();
        }


        public void RegisterClient()
        {
            var token = _tokenRegisterer.RegisterTokenWithExpiry(TokenTimeoutInSeconds);
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("no valid token supplied", nameof(token));
            }

            if (!IsConnected)
            {
                throw new InvalidOperationException(Resources.InstructionalClientWCFController_NotConnected);
            }

            _registerClientTask = Task.Run(() =>
            {
                try
                {
                    var resp = _notificationServices.RegisterClient(_clientId, token);
                    if (!resp.Success)
                    {
                        Log.Error("Duplex Channel WCF Register Client Failed - Closing down callback channel");
                        AbortDuplexChannel();
                    }
                    else
                    {
                        Log.Debug("Duplex Channel WCF Register Client Success - Callback channel up");
                        _clientRegistered = true;
                        _isChannelDown = false;
                        _registerEvent.Set();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Duplex Channel WCF Register Client Failed with exception - Closing down callback channel");
                    AbortDuplexChannel();
                }
            });

        }

        public void EnsureConnected()
        {
            // Open if working, or trying to connect 
            if (!IsDuplexOpen())
            {
                _isChannelDown = true;
                Log.Debug("Duplex Channel WCF is down - Attempting Reconnect...");
                Reconnect();
                _registerClientTask.Wait(2000);
            }
            else
            {
                // Check we're online, if this fails the duplex will abort and reconnect attempted next time
                Log.Debug("Duplex Channel WCF Service Status Check...");
                GetServiceStatus();
                _isChannelDown = !_checkEvent.WaitOne(ReconnectWait);
            }

            
            if (_isChannelDown)
            {
                throw new InvalidInstructionalConnectionException();
            }
        }

        public void DeRegister()
        {
            UnregisterClient();
            _unregisterEvent.WaitOne();
        }


        private void UnregisterClient()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException(Resources.InstructionalClientWCFController_NotConnected);
            }

            Task.Run(() =>
            {
                try
                {
                    var resp = _notificationServices.UnRegisterClient(_clientId);
                    if (!resp.Success)
                    {
                        // the user likely doesnt care about this scenario anyway. 
                        ErrorReceived?.Invoke(new FailedCallbackOperationEventArgs(
                            resp.Message,
                            resp.Error));
                    }
                    else
                    {
                        _clientRegistered = false;
                    }
                }
                catch (CommunicationException)
                {
                    // No action required if channel no longer expects this
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Duplex Channel WCF Client Unregister Failed - Closing down callback channel");
                    AbortDuplexChannel();
                }
                finally
                {
                    _unregisterEvent.Set();
                }
            });
        }

        private void GetServiceStatus()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException(Resources.InstructionalClientWCFController_NotConnected);
            }

            Task.Run(() =>
            {
                try
                {
                    var resp = _notificationServices.GetStatus(_clientId);
                    if (!resp.Success)
                    {
                        Log.Error("Duplex Channel WCF Service Status Failed - Closing down callback channel");
                        AbortDuplexChannel();
                    }
                    else
                    {
                        Log.Debug("Duplex Channel WCF Service Status Success - Callback channel still up");
                        _checkEvent.Set();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Duplex Channel WCF Get Status Failed - Closing down callback channel");
                    AbortDuplexChannel();
                }
            });

        }

        private bool IsDuplexOpen()
        {
            try
            {
                Monitor.Enter(_duplexChannelFactory);
                return _duplexChannelFactory.State == CommunicationState.Opened;
            }
            finally
            {
                Monitor.Exit(_duplexChannelFactory);
            }
        }

        private void AbortDuplexChannel()
        {
            _isChannelDown = true;
            try
            {
                Monitor.Enter(_duplexChannelFactory);
                _duplexChannelFactory.Abort();
            }
            finally
            {
                Monitor.Exit(_duplexChannelFactory);
            }
        }

        private void Reconnect()
        {
            CreateChannel();
            _resetEvent.WaitOne();

            if (_clientRegistered)
            {
                // De-register any old versions of our client id.
                UnregisterClient();

                // Make sure we aren't picking up a previous signal
                _unregisterEvent.Reset();

                // If we don't get a timely response, we've likely failed
                if (_unregisterEvent.WaitOne(ReconnectWait) && IsDuplexOpen())
                {
                    RegisterClient();
                    _registerEvent.WaitOne(ReconnectWait);
                }
            }
            else
            {
                RegisterClient();
                _registerEvent.WaitOne(ReconnectWait);
            }
        }

        protected Binding ConfigureServiceHost()
        {
            var binding = new WSDualHttpBinding() { Name = ConnectionConfig.BindingName };
            binding.Security.Mode = WSDualHttpSecurityMode.None;

            switch (_config.Mode)
            {
                case InstructionalConnectionModes.None:
                    Log.Error($"Invalid instructional connection credential mode: None");
                    throw new InvalidInstructionalConnectionException(Resources.InstructionalClientWCFController_CallbackConfigurationSecurityModeNotSet);
                case InstructionalConnectionModes.Certificate:
                    binding.Security.Mode = WSDualHttpSecurityMode.Message;
                    binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                    break;
                case InstructionalConnectionModes.Windows:
                    binding.Security.Mode = WSDualHttpSecurityMode.Message;
                    binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
                    break;
                case InstructionalConnectionModes.Insecure:
                    binding.Security.Mode = WSDualHttpSecurityMode.None;
                    binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
                    break;
                default:
                    throw new InvalidInstructionalConnectionException(string.Format(Core.Properties.Resources.InvalidInstructionalConnectionMode, _config.Mode));
            }

            binding.TransactionFlow = false;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.UseDefaultWebProxy = false;
            binding.SendTimeout = new TimeSpan(0, 0, 0, 0, KeepAliveTimeoutMS);
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            return binding;
        }

        protected void CreateChannel()
        {
            var instanceContext = new InstanceContext(_callbackservice);
            _duplexChannelFactory = new DuplexChannelFactory<INotificationServices>(instanceContext, ConfigureServiceHost(), _address);
            _duplexChannelFactory.Credentials.SupportInteractive = false;

            if (_config.Mode.Equals(InstructionalConnectionModes.Certificate))
            {
                _duplexChannelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                _duplexChannelFactory.Credentials.ClientCertificate.Certificate
                        = _config.ClientCertificate.ToCertificate() ?? throw new InvalidProgramException("certificate should not be null");
            }

            Log.Debug($"WCF Callback Creating Channel - {_duplexChannelFactory.Endpoint.Address}");
            Task.Run(() =>
            {
                // the channel has to be created on a thread otherwise things go very wrong
                Monitor.Enter(_duplexChannelFactory);
                _notificationServices = _duplexChannelFactory.CreateChannel();
                Monitor.Exit(_duplexChannelFactory);
                _resetEvent.Set();
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _resetEvent.Dispose();
                    _checkEvent.Dispose();
                    _registerEvent.Dispose();
                    _unregisterEvent.Dispose();
                    Close();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Close()
        {
            if (IsDuplexOpen())
            {
                _isChannelDown = true;
                Monitor.Enter(_duplexChannelFactory);
                try
                {
                    using (var resetEvent = new AutoResetEvent(false))
                    {
                        _duplexChannelFactory.Closed += (s, e) =>
                        {
                            Log.Info("WCF callback channel has been closed");
                            resetEvent.Set();
                        };
                        _duplexChannelFactory.Close();
                        resetEvent.WaitOne(DisconnectWait);
                    }
                }
                catch
                {
                    _duplexChannelFactory.Abort();
                }
                Monitor.Exit(_duplexChannelFactory);
            }

        }
    }
}
