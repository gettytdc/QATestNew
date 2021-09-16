using System;
using System.ServiceModel;
using System.Threading;
using BluePrism.ClientServerResources.Core.Config;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Exceptions;
using BluePrism.ClientServerResources.Core.Interfaces;
using BluePrism.ClientServerResources.Wcf.Endpoints;
using BluePrism.ClientServerResources.Wcf.Properties;
using BluePrism.Core.Utility;
using NLog;

namespace BluePrism.ClientServerResources.Wcf
{

    public class InstructionalConnectionWCFHost : IInstructionalHostController
    {
        private readonly ServiceHost _serviceHost;
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly NotificationServices _notificationServices = new NotificationServices();
        private readonly ConnectionConfig _config;

        private bool _disposedValue = false;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly int _callbackSendTimeout;

        public InstructionalConnectionWCFHost(ConnectionConfig config, int callbackSendTimeout)
        {
            try
            {
                Log.Info($"WCF callback connection initialized");
                _config = config ?? throw new ArgumentException(nameof(config));
                _callbackSendTimeout = callbackSendTimeout;

                if (_serviceHost is null)
                {
                    _serviceHost = ConfigureServiceHost(config);
                }

                _serviceHost.Open();
            }
            catch (Exception ex)
            {
                Log.Error($"WCF Callback host initialistion error: {ex.GetType()} - {ex.Message}");
                throw new InvalidOperationException($"WCF Callback Startup Error - {ex.Message}");
            }
        }

        private ServiceHost ConfigureServiceHost(ConnectionConfig config)
        {
            var binding = new WSDualHttpBinding { Name = ConnectionConfig.BindingName };

            Log.Info($"WCF Callback config : {config.HostName}:{config.Port} {config.Mode} ");
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
                    binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
                    break;
                default:
                    throw new InvalidInstructionalConnectionException(string.Format(Core.Properties.Resources.InvalidInstructionalConnectionMode, _config.Mode));
            }

            var protocol = "http";
            binding.TransactionFlow = false;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.UseDefaultWebProxy = false;
            binding.SendTimeout = new TimeSpan(0, 0, 0, 0, _callbackSendTimeout);
            binding.ReceiveTimeout = TimeSpan.MaxValue;

            var hostname = _config.HostName;
                
            if (string.IsNullOrEmpty(hostname))
            {
                binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
                hostname = "0";
            }
            else
            {
                binding.HostNameComparisonMode = HostNameComparisonMode.Exact;
            }

            var address = $"{protocol}://{IPAddressHelper.EscapeForURL(hostname)}:{_config.Port}/{ConnectionConfig.EndpointName}";

            var uri = new Uri(address);
            var serviceHost = new ServiceHost(_notificationServices, uri);
            var endpoint = serviceHost.AddServiceEndpoint(typeof(INotificationServices), binding, address);

            if (_config.Mode.Equals(InstructionalConnectionModes.Windows) ||
                _config.Mode.Equals(InstructionalConnectionModes.Certificate))
            {
                var hostName = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;
                var ident = EndpointIdentity.CreateSpnIdentity($"HTTP/{hostName}:{_config.Port}/{ConnectionConfig.EndpointName}");
                endpoint.Address = new EndpointAddress(uri, ident);
            }

            if (_config.Mode.Equals(InstructionalConnectionModes.Certificate))
            {
                CredentialFactory.CreateCertificateCredentials(ref serviceHost, _config);
            }
            return serviceHost;
        }

        protected virtual void ShutdownServiceHost()
        {
            _serviceHost.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _autoResetEvent.Dispose();
                    _serviceHost.Close();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public override string ToString() => _config.ToString();

        public void ResourceStatusChanged(ResourcesChangedData data)
        {
            if (data == null) { throw new ArgumentException(nameof(data)); }
            _notificationServices.ResourceStatus(Guid.Empty, data);
        }

        public void SessionCreated(SessionCreatedData data)
        {
            if (data == null) { throw new ArgumentException(nameof(data)); }
            _notificationServices.SessionCreated(Guid.Empty, data);
        }

        public void SessionDelete(SessionDeletedData data)
        {
            if (data == null) { throw new ArgumentException(nameof(data)); }
            _notificationServices.DeleteSession(Guid.Empty, data);
        }

        public void SessionEnd(SessionEndData data)
        {
            if (data == null) { throw new ArgumentException(nameof(data)); }
            _notificationServices.SessionEnd(Guid.Empty, data);
        }

        public void SessionStart(SessionStartedData data)
        {
            if (data == null) { throw new ArgumentException(nameof(data)); }
            _notificationServices.SessionStarted(Guid.Empty, data);
        }

        public void SessionStop(SessionStopData data)
        {
            if (data == null)
            { throw new ArgumentException(nameof(data)); }
            _notificationServices.SessionStop(Guid.Empty, data);
        }

        public void SessionVariablesUpdated(SessionVariablesUpdatedData data)
        {
            if (data == null)
            { throw new ArgumentException(nameof(data)); }
            _notificationServices.SessionVariablesUpdated(Guid.Empty, data);
        }

    }
}
