using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Services;
using System.Runtime.Serialization.Formatters;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Timers;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Logging;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.BPCoreLib;
using BluePrism.BPCoreLib.Collections;
using BluePrism.BPServer.Enums;
using BluePrism.BPServer.Properties;
using BluePrism.BPServer.ServerBehaviours;
using BluePrism.Common.Security;
using BluePrism.Core.Analytics;
using BluePrism.Core.Utility;
using BluePrism.Datapipeline.Logstash;
using BluePrism.Datapipeline.Logstash.Configuration;
using BluePrism.Datapipeline.Logstash.Wrappers;
using BluePrism.Scheduling;
using BluePrism.Server.Domain.Models;
using BluePrism.WorkQueueAnalysis.Classes;
using Microsoft.Win32;
using NLog;
using BluePrism.AuthenticationServerSynchronization;
using BluePrism.BPCoreLib.DependencyInjection;
using System.Threading.Tasks;

namespace BPServer
{

    /// Project: BPServer
    /// Class: clsBPServer
    /// <summary>Encapsulation of a Blue Prism Server. This class incorporates the
    /// functionality common to both the standalone executable and Windows Service
    /// version of the server.</summary>
    public class clsBPServer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The number of milliseconds to wait between database checks within the
        /// scheduler. It will check the specified number of milliseconds to see if
        /// the scheduler data has changed since the last check.
        /// </summary>
        private const int SchedulerDBCheckInterval = 10 * 1000; // every 10 seconds

        private const string LogstashRegistryKey = @"SOFTWARE\Blue Prism Limited\Logstash";

        internal int ConnectedClients;

        private bool mWCF = false;

        /// <summary>
        /// True when the server is running.
        /// </summary>
        public bool Running
        {
            get { return mRunning; }
            private set
            {
                mRunning = value;
                clsServer.RunningOnServer = value;
            }
        }
        private bool mRunning;

        /// <summary>
        /// When running, this is our service host.
        /// </summary>
        ServiceHost mServiceHost;

        /// <summary>
        /// When running, this is our registered channel.
        /// </summary>
        TcpChannel mChannel;

        /// <summary>
        /// When running, these are our handlers for listening to .NET remoting
        /// stuff.
        /// </summary>
        TrackingHandler mTrackingHandler;
        DynamicProperty mDynamicProperty;

        /// <summary>
        /// The scheduler to run within this instance of BPServer.
        /// </summary>
        private IScheduler mScheduler;
        
        private IResourceConnectionManager _resourceConnectionManager;

        /// <summary>
        /// MI data auto-refresh timer
        /// </summary>        
        private Timer mMIAutoRefresh;
        private readonly CertificateExpiryChecker _certificateExpiryChecker;

        private LogstashProcessManager _logstashProcessManager;

        private SnapshotManager _snapshotManager;

        private MessageBus _messageBus;

        private readonly IDependencyResolver _dependencyResolver = DependencyResolver.GetScopedResolver();

        /// <summary>
        /// Constructor
        /// </summary>
        public clsBPServer()
        {
            Running = false;
            RemotingConfiguration.RegisterActivatedServiceType(typeof(clsServer));
            // Turn off custom errors, so exceptions get reported back in full.
            RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

            mMIAutoRefresh = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
            mMIAutoRefresh.Elapsed += mMIAutoRefresh_Elapsed;
            mMIAutoRefresh.AutoReset = false;

            _certificateExpiryChecker = new CertificateExpiryChecker();
            _certificateExpiryChecker.Update += CertificateExpiryChecker_Update;
        }

        /// <summary>
        /// Event raised when an error message is raised by the server
        /// </summary>
        public event StatusHandler Err;
        /// <summary>
        /// Event raised when a warning message is raised by the server
        /// </summary>
        public event StatusHandler Warn;
        /// <summary>
        /// Event raised when an informational message is raised by the server
        /// </summary>
        public event StatusHandler Info;
        /// <summary>
        /// Event raised when a verbose message is raised by the server
        /// </summary>
        public event StatusHandler Verbose;
        /// <summary>
        /// Event raised when an analytics message is raised by the server
        /// </summary>
        public event StatusHandler Analytics;

        /// <summary>
        /// Delegate describing a status message handler
        /// </summary>
        /// <param name="message">The message which is being sent</param>
        /// <param name="level">The level at which the logging request was sent.</param>
        public delegate void StatusHandler(string message, LoggingLevel level);

        /// <summary>
        /// Fires a status message to the given handler, if any listeners are
        /// registered on that handler
        /// </summary>
        /// <param name="handler">The handler delegate to fire the status message to.
        /// </param>
        /// <param name="message">The message to pass to the handler</param>
        /// <param name="level">The level at which the logging request was sent.</param>
        private void FireStatus(StatusHandler handler, string message, LoggingLevel level)
        {
            handler?.Invoke(message, level);
        }

        private void CertificateExpiryChecker_Update(string message, LoggingLevel level)
        {
            if (level == LoggingLevel.Information)
                FireStatus(Info, message, level);
            if (level == LoggingLevel.Warning)
                FireStatus(Warn, message, level);
            if (level == LoggingLevel.Error)
                FireStatus(Err, message, level);
        }

        /// <summary>
        /// Fires a status message to the given handler, if any listeners are
        /// registered on that handler
        /// </summary>
        /// <param name="handler">The handler delegate to fire the status message to.
        /// </param>
        /// <param name="message">The message to pass to the handler, with formatting
        /// placeholders</param>
        /// <param name="level">The level at which the logging request was sent.</param>
        /// <param name="args">The arguments to insert into the formatted message.</param>
        private void FireStatus(
            StatusHandler handler, string message, LoggingLevel level, params object[] args)
        {
            FireStatus(handler, String.Format(message, args), level);
        }

        /// <summary>
        /// Used internally to record an error message.
        /// </summary>
        /// <param name="msg">The message.</param>
        internal void OnErr(string msg) { FireStatus(Err, msg, LoggingLevel.Error); }

        /// <summary>
        /// Used internally to record an error message.
        /// </summary>
        /// <param name="msg">The message with formatting placeholders.</param>
        /// <param name="args">The arguments to insert into the mesage</param>
        internal void OnErr(
            string msg, params object[] args)
        { FireStatus(Err, msg, LoggingLevel.Error, args); }

        /// <summary>
        /// Used internally to record a warning message.
        /// </summary>
        /// <param name="msg">The message.</param>
        internal void OnWarn(string msg) { FireStatus(Warn, msg, LoggingLevel.Warning); }

        /// <summary>
        /// Used internally to record a warning message.
        /// </summary>
        /// <param name="msg">The message with formatting placeholders.</param>
        /// <param name="args">The arguments to insert into the mesage</param>
        internal void OnWarn(
            string msg, params object[] args)
        { FireStatus(Warn, msg, LoggingLevel.Warning, args); }

        /// <summary>
        /// Used internally to record an info message.
        /// </summary>
        /// <param name="msg">The message.</param>
        internal void OnInfo(string msg) { FireStatus(Info, msg, LoggingLevel.Information); }

        /// <summary>
        /// Used internally to record an info message.
        /// </summary>
        /// <param name="msg">The message with formatting placeholders.</param>
        /// <param name="args">The arguments to insert into the mesage</param>
        internal void OnInfo(
            string msg, params object[] args)
        { FireStatus(Info, msg, LoggingLevel.Information, args); }

        /// <summary>
        /// Used internally to record a verbose message.
        /// </summary>
        /// <param name="msg">The message.</param>
        internal void OnVerbose(string msg) { FireStatus(Verbose, msg, LoggingLevel.Verbose); }

        /// <summary>
        /// Used internally to record a verbose message.
        /// </summary>
        /// <param name="msg">The message with formatting placeholders.</param>
        /// <param name="args">The arguments to insert into the mesage</param>
        internal void OnVerbose(
            string msg, params object[] args)
        { FireStatus(Verbose, msg, LoggingLevel.Verbose, args); }

        /// <summary>
        /// Used internally to record an analytics message.
        /// </summary>
        /// <param name="msg">The message.</param>
        private void OnAnalytics(string msg) { FireStatus(Analytics, msg, LoggingLevel.Analytics); }

        /// <summary>
        /// Handles an incoming status message from the scheduler.
        /// </summary>
        /// <param name="msg">The message.</param>
        private void SchedulerStatus(string msg)
        {
            OnInfo("Scheduler: " + msg);
        }

        private void LogInfo(string msg)
        {
            Log.Info(msg);
        }

        private List<string> ServerEncryptionSchemes(MachineConfig.ServerConfig config)
        {
            var nonFIPSEncryptMessage = new List<string>();
            foreach (KeyValuePair<string, clsEncryptionScheme> encryptionScheme in config.EncryptionKeys)
            {
                if (!clsFIPSCompliance.CheckForFIPSCompliance(encryptionScheme.Value.Algorithm))
                {
                    var encryptName = encryptionScheme.Value.Algorithm;
                    var errorMessage = string.Format(Resources.InvalidEncryptionScheme0InUseBy1,
                                                     encryptName, Resources.ServerEncryptionKeyStore);
                    nonFIPSEncryptMessage.Add(errorMessage);
                }
            }
            return nonFIPSEncryptMessage;
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            if (!Running)
            {
                OnWarn("Request made to stop the server - it is not running");
                return;
            }

            // Stop the scheduler first so it can terminate any tasks / sessions
            // cleanly and report to the database why they have been terminated
            if (mScheduler != null && mScheduler.IsRunning())
            {
                EnableSchedulerChangeListening(false);
                mScheduler.Stop();
            }

            _resourceConnectionManager?.Dispose();

            _logstashProcessManager?.Dispose();
            _snapshotManager?.Dispose();

            // Stop the MI refresh timer
            mMIAutoRefresh?.Stop();
            _certificateExpiryChecker?.Stop();

            Running = false;

            if (!mWCF)
            {
                StopDotNetRemoteListener();
            }
            else
            {
                mServiceHost.Close();
            }

            _messageBus?.Stop();

            // Disable auditing.
            app.gAuditingEnabled = false;

        }

        /// <summary>
        /// Start the server
        /// </summary>
        /// <param name="cfg">The server configuration.</param>
        /// <exception cref="MissingItemException">If the connection is not set in
        /// the given configuration</exception>
        /// <exception cref="InvalidStateException">If the connection was invalid, or
        /// the licence could not be loaded</exception>
        /// <exception cref="NotSupportedException">If the server was attempted to be
        /// started on an environment covered by an NHS Licence</exception>
        public void Start(MachineConfig.ServerConfig cfg)
        {
            if (Running) throw new AlreadyExistsException(
                "Cannot start server - already running");

            Options.Instance.CurrentServerConfigName = cfg.Name;

            clsDBConnectionSetting cons = cfg.GetServerConnection();
            if (cons == null) throw new MissingItemException(
                "Database connection is not configured correctly");

            OnInfo("Connecting to database using connection '{0}'...",
                cons.ConnectionName);
            IUser user = new User(AuthMode.System, Guid.Empty, "Scheduler");
            ServerFactory.ServerInit(cfg.ConnectionMode, ref cons, cfg.EncryptionKeys, ref user);

            try
            {
                ServerFactory.CurrentConnectionValid();
            }
            catch (Exception ex)
            {
                throw new InvalidStateException("Connection not valid: {0}", ex.Message);
            }

            var sv = (app.gSv as clsServer);
            if (sv == null) throw new InvalidStateException("Server is not the correct type");

            // Set the connection mode on the sever so that it knows how to format errors
            clsServer.ConnectionMode = cfg.ConnectionMode;

            //Check that we can use Snapshot Isolation level
            if (!sv.CheckSnapshotIsolationIsEnabledInDB())
                throw new InvalidStateException($"Database does not have snapshot isolation level enabled");

            // Get license.
            string licenseErr = clsLicenseQueries.RefreshLicense();
            if (licenseErr != null)
                throw new InvalidStateException("License error: {0}", licenseErr);

            // Check if the license is valid currently
            if (!Licensing.License.IsLicensed)
                throw new NotSupportedException(
                    "A valid license could not be detected");

            // Check if we can run a server - we can't in the NHS Edition!
            if (!Licensing.License.CanUse(LicenseUse.BPServer))
                throw new NotSupportedException(
                    $"The Blue Prism Server cannot be used with the {Licensing.License.LicenseType} license");

            //Check that the dependency information is intact
            if (!DependenciesValid())
                throw new InvalidStateException("Process dependency information is not valid.");

            //Check all encryption schemes are valid
            List<string> invalidSchemes = sv.GetInvalidEncryptionSchemeNames();
            if (invalidSchemes.Count != 0)
            {
                string encErr = string.Empty;
                foreach (String name in invalidSchemes)
                {
                    if (encErr.Length > 0) encErr += ", ";
                    encErr += name;
                }
                if (encErr.Length > 0)
                    throw new InvalidStateException(string.Format("The following encryption keys could not be resolved: {0}", encErr));
            }

            //Check encryption schemes are FIPS compliant when FIPS GPO is enabled
            var nonFIPSEncryptMessage = new List<string>();
            nonFIPSEncryptMessage.AddAll(sv.DBEncryptionSchemesAreFipsCompliant());
            nonFIPSEncryptMessage.AddAll(ServerEncryptionSchemes(cfg));

            if (nonFIPSEncryptMessage.Any())
            {
                //Stop the server loading if the encryption scheme is not FIPS compliant and log the exception message

                var fipsErrorMessage = string.Join(Environment.NewLine, nonFIPSEncryptMessage);

                OnErr(fipsErrorMessage);
                Log.Error(fipsErrorMessage);
                throw new InvalidStateException(fipsErrorMessage);
            }

            // Make sure auditing is enabled...
            app.gAuditingEnabled = true;

            OnInfo("Starting listener...");
            OnInfo("Using Transport mechanism - " + cfg.ConnectionMode.ToString());

            try
            {
                // Do we want DNR or WCF
                if (cfg.ConnectionMode == ServerConnection.Mode.WCFInsecure ||
                    cfg.ConnectionMode == ServerConnection.Mode.WCFSOAPMessageWindows ||
                    cfg.ConnectionMode == ServerConnection.Mode.WCFSOAPTransport ||
                    cfg.ConnectionMode == ServerConnection.Mode.WCFSOAPTransportWindows)
                {
                    mWCF = true;
                    StartWCFListener(cfg);
                }
                else
                {
                    mWCF = false;
                    StartDotNetRemoteListener(cfg);
                }

                //if using a certificate, obtain it from the store and record the expire time.  if not, then the cert expirie time is null            
                SaveEnvironmentData(cfg.Port, Options.Instance.GetCertificateExpiryDateTime());


                //Setup the resource connection manager based on the current configuration state.
                var useASCR = sv.GetPref(PreferenceNames.SystemSettings.UseAppServerConnections, false);
                if (useASCR)
                {
                    sv.InitCallbackConfig(cfg.CallbackConnectionConfig);
                }
                SetupResourceConnectionManager(user, cfg.SchedulerDisabled, SchedulerConfig.Active, useASCR);

            }
            catch
            {
                _resourceConnectionManager?.Dispose();
                if (!mWCF)
                {
                    StopDotNetRemoteListener();
                }
                else
                {
                    mServiceHost?.Abort();
                }
                throw;
            }

            // If the scheduler is set to activate, start it; otherwise, give a
            // message as to why it's not running.
            if (cfg.SchedulerDisabled)
            {
                OnInfo("Scheduler disabled on this machine... skipping");
            }
            else if (!SchedulerConfig.Active)
            {
                OnInfo("Scheduler disabled in this environment... skipping");
            }
            else
            {
                OnInfo("Starting Scheduler...");

                mScheduler = new AutomateScheduler(user, _resourceConnectionManager,
                    Environment.MachineName + ":" + cfg.Port);
                mScheduler.StatusUpdated += SchedulerStatus;
                mScheduler.AddInfoLog += LogInfo;
                EnableSchedulerChangeListening(true);

                // Get how far back to check (in seconds)...
                // Of course the scheduler works in millis, so further conversion..
                mScheduler.Start(1000 * SchedulerConfig.CheckSeconds);
            }

            if (cfg.DataPipelineProcessEnabled)
            {
                OnInfo(Resources.StartingDataPipelineProcess);

                string logstashDir = null;
                string logstashPath = null;

                using (var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    var key = view32.OpenSubKey(LogstashRegistryKey, false);
                    if (key == null) throw new BluePrismException(Resources.clsBPServer_DataPipelineComponentsNotInstalled);

                    logstashDir = key.GetValue("logstashdir") as string;
                    var executable = cfg.DataGatewaySpecificUser ? "LogstashLauncher.exe" : "logstash.bat";
                    logstashPath = Path.Combine(logstashDir, executable);
                }

                IEventLogger eventLogger = new NLogEventLogger();
                if (cfg.DataGatewayLogToConsole) eventLogger = new BpServerEventLogger(OnInfo);

                var monitorFrequency = app.gSv.GetDataPipelineSettings().MonitoringFrequency * 1000;

                if (!cfg.DataGatewaySpecificUser)
                {
                    cfg.DataGatewayDomain = String.Empty;
                    cfg.DataGatewayUser = String.Empty;
                    cfg.DataGatewayPassword = new SafeString();
                }

                try
                {
                    int port = cfg.DataPipelineProcessCommandListenerPort;


                    var configurationService = new LogstashConfigurationService(app.gSv,
                                                                                new ConfigurationPreprocessorFactory(app.gSv,
                                                                                                                    new ProcessFactory(),
                                                                                                                    logstashDir),
                                                                                new FileSystemService(),
                                                                                eventLogger,
                                                                                () => new LogstashSecretStore(new ProcessFactory(),
                                                                                                              logstashDir),
                                                                                logstashDir);

                    var logstashConfiguration = new LogstashProcessSettings()
                    {
                        ConfigurationDirectory = configurationService.ConfigFolder,
                        ConfigurationPath = configurationService.ConfigFilePath,
                        Domain = cfg.DataGatewayDomain,
                        UserName = cfg.DataGatewayUser,
                        Password = cfg.DataGatewayPassword,
                        TraceLogging = cfg.DataGatewayTraceLogging,
                        LogstashDirectory = logstashDir,
                        LogstashPath = logstashPath
                    };

                    _logstashProcessManager = new LogstashProcessManager(
                        app.gSv,
                        Dns.GetHostName(),
                        eventLogger,
                        new TCPCommandListener(port, eventLogger),
                        new LogstashProcessFactory(eventLogger),
                        configurationService,
                        () => new TimerWrapper(new Timer()),
                        monitorFrequency,
                        logstashConfiguration);

                    _logstashProcessManager.StartProcess();
                }
                catch (Exception e)
                {
                    OnErr($"Unable to start data gateways process. {e.Message}");
                }
            }
            else
            {
                OnInfo("Data pipeline process disabled. Skipping...");
            }

            if (app.gSv.IsMIReportingEnabled())
            {
                OnInfo("Starting Work Queue Analysis process.");
                _snapshotManager = new SnapshotManager(app.gSv);
            }

            // Start thread for MI refresh and certificate expiry check
            mMIAutoRefresh?.Start();
            _certificateExpiryChecker?.Start(Options.Instance.Thumbprint);

            if (cfg.AuthenticationServerBrokerConfig != null)
            {
                _messageBus = _dependencyResolver.Resolve<MessageBus>();
                var _ = Task.Run(async () =>
                {
                    OnInfo("Starting message bus");
                    await _messageBus.Start();
                    var result = _messageBus.Status == MessageBus.MessageBusStatus.Started ? "Message bus started" : "Failed to start message bus";
                    OnInfo(result);
                });

            }

            OnInfo("Server startup sequence complete.");
            Running = true;
        }

        private void SetupResourceConnectionManager(IUser user,bool schedulerDisabled,bool schedularConfigActive, bool useASCR)
        {
            try
            {
                if ( (!schedulerDisabled && schedularConfigActive) || useASCR)
                {
                        _resourceConnectionManager = ResourceConnectionManagerFactory.GetResourceConnectionManager(useASCR);
                        _resourceConnectionManager.ConnectionUser = user;
                }
            }
            catch (Autofac.Core.DependencyResolutionException currentEx)
            {
                Log.Error("Error in setting Resource Connection Manager", currentEx);
                throw (currentEx.GetBaseException());
            }

            OnInfo($"Using {_resourceConnectionManager}");
        }

        private void SaveEnvironmentData(int iPort, DateTime? certExpiryDateTime)
        {
            try
            {
                app.gSv.SaveEnvironmentData(
                    new EnvironmentData(EnvironmentType.Server,
                                        clsUtility.GetFQDN(),
                                        iPort,
                                        Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                                        DateTime.UtcNow,
                                        DateTime.UtcNow,
                                        certExpiryDateTime));
            }
            catch (Exception ex)
            {
                OnInfo($"Failed to save environment data {ex.Message}");
            }
        }

        private void ServerFaulted(object sender, EventArgs e)
        {
            OnInfo("Connection Faulted event raised");
        }

        /// <summary>
        /// Create a WCF endpoint for the server.
        /// </summary>
        /// <param name="cfg"></param>
        private void StartWCFListener(MachineConfig.ServerConfig cfg)
        {
            var databaseType = app.gSv.DatabaseType();
            var binding = new WSHttpBinding();
            binding.Name = "bpserverbinding";
            string protocol;
            switch (cfg.ConnectionMode)
            {
                case ServerConnection.Mode.WCFInsecure:
                    if (databaseType == DatabaseType.SingleSignOn)
                        throw new InvalidStateException("Unencrypted BPServer communications cannot be used with Single Signon");
                    binding.Security.Mode = SecurityMode.None;
                    binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
                    protocol = "http";
                    break;
                case ServerConnection.Mode.WCFSOAPMessageWindows:
                    binding.Security.Mode = SecurityMode.Message;
                    binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
                    protocol = "http";
                    break;
                case ServerConnection.Mode.WCFSOAPTransportWindows:
                    binding.Security.Mode = SecurityMode.TransportWithMessageCredential;
                    binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
                    protocol = "https";
                    break;
                case ServerConnection.Mode.WCFSOAPTransport:
                    if (databaseType == DatabaseType.SingleSignOn)
                        throw new InvalidStateException("Transport-secured communications without Message Credentials cannot be used with Single Signon");
                    binding.Security.Mode = SecurityMode.TransportWithMessageCredential;
                    binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
                    protocol = "https";
                    break;
                default:
                    throw new InvalidStateException("Invalid connection mode");
            }

            binding.TransactionFlow = false;
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            binding.MaxBufferPoolSize = Int32.MaxValue;
            binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            binding.ReaderQuotas.MaxBytesPerRead = 1024 * 1024;
            binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            binding.ReaderQuotas.MaxDepth = 128;

            //Default the bindTo if it hasn't been set.
            var bindTo = cfg.BindTo;
            if (string.IsNullOrEmpty(bindTo))
            {
                binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
                bindTo = "0";
            }
            else
            {
                binding.HostNameComparisonMode = HostNameComparisonMode.Exact;
            }

            var customBinding = EnableReliableSession(binding, cfg);

            var address = string.Format("{0}://{1}:{2}/bpserver", protocol, IPAddressHelper.EscapeForURL(bindTo), cfg.Port);
            var uri = new Uri(address);
            mServiceHost = new ServiceHost(typeof(clsServer), uri);
            mServiceHost.Description.Behaviors.Add(new GenericErrorHandler());
            ServiceEndpoint endpoint = mServiceHost.AddServiceEndpoint(typeof(IServer), customBinding, address);


            // If using a WCF connection mode that uses Windows Authentication we need to create an Endpoint
            // Identity. This is needed because Kerberos authentication requires the client to compare its 
            // expected endpoint identity value with the actual value the endpoint authentication process
            // returned from the service. If they match, the client is assured it has contacted the expected
            // service endpoint. As a result an expected Spn must be generated in exactly the same format on
            // the client.
            if (cfg.ConnectionMode == ServerConnection.Mode.WCFSOAPMessageWindows ||
            cfg.ConnectionMode == ServerConnection.Mode.WCFSOAPTransportWindows)
            {
                var hostName = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;
                var ident = EndpointIdentity.CreateSpnIdentity(String.Format("HTTP/{0}:{1}/BPServer", hostName, cfg.Port));
                endpoint.Address = new EndpointAddress(uri, ident);
            }
            else
            {
                endpoint.Address = new EndpointAddress(uri);
            }

            endpoint.Behaviors.Add(new WCFServerMessageInspector(this, cfg.LogTraffic));

            if (Options.Instance.WcfPerformanceLogMinutes != null)
            {
                endpoint.Behaviors.Add(new WCFAnalyticsEndpointBehavior(Options.Instance.WcfPerformanceLogMinutes.Value));
                Log.Info($"WCF Peformance Started for {Options.Instance.WcfPerformanceLogMinutes.Value} minutes(s)");
            }
            
            mServiceHost.Faulted += new EventHandler(ServerFaulted);
            
            if (cfg.ConnectionMode == ServerConnection.Mode.WCFSOAPTransport)
            {
                mServiceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Custom;
                mServiceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new NullValidator();
            }

#if DEBUG
            // Enable metadata publishing for debug builds only.
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            if (cfg.ConnectionMode == ServerConnection.Mode.WCFSOAPTransportWindows || cfg.ConnectionMode == ServerConnection.Mode.WCFSOAPTransport)
                smb.HttpsGetEnabled = true;
            else
                smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            mServiceHost.Description.Behaviors.Add(smb);
#endif

            ServiceThrottlingBehavior stb = new ServiceThrottlingBehavior();
            stb.MaxConcurrentSessions = Int32.MaxValue;
            stb.MaxConcurrentCalls = Int32.MaxValue;
            stb.MaxConcurrentInstances = Int32.MaxValue;
            mServiceHost.Description.Behaviors.Add(stb);

            try
            {
                mServiceHost.Open();
            }
            catch (AddressAccessDeniedException)
            {
                string msg = "An error occurred while trying to start the server. The account the " +
                   "service is running as ({1}) does not have the right to create services " +
                   "that listen on the server's namespace. Possible resolutions include:" +
                   "{0}1) Run the BP Server service under a local admin account." +
                   "{0}2) Run the following command to give {1} the correct rights:" +
                   "{0}netsh http add urlacl url={2}://{4}:{3}/bpserver/ user={1}{0}";
                if (cfg.BindTo == null || cfg.BindTo == "")
                {
                    // display the wildcard command
                    throw new InvalidStateException(
                   msg,
                   Environment.NewLine, WindowsIdentity.GetCurrent().Name, protocol, cfg.Port, "+");
                }
                else
                {
                    // display the address command
                    throw new InvalidStateException(
                    msg,
                    Environment.NewLine, WindowsIdentity.GetCurrent().Name, protocol, cfg.Port, cfg.BindTo);
                }

            }
            OnInfo("...listening on " + address);
        }

        private CustomBinding EnableReliableSession(WSHttpBinding binding, MachineConfig.ServerConfig config)
        {
            binding.ReliableSession.Enabled = true;
            var elements = binding.CreateBindingElements();
            var reliableSessionElement = elements.Find<ReliableSessionBindingElement>();
            if (reliableSessionElement != null)
            {
                if (config.MaxTransferWindowSize != null)
                {
                    try
                    {
                        reliableSessionElement.MaxTransferWindowSize = config.MaxTransferWindowSize.Value;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new InvalidStateException(string.Format(Resources.MaxTransferWindowSizeOutOfRange, config.MaxTransferWindowSize.Value));
                    }
                }

                if (config.MaxPendingChannels != null)
                {
                    try
                    {
                        reliableSessionElement.MaxPendingChannels = config.MaxPendingChannels.Value;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new InvalidStateException(string.Format(Resources.MaxPendingChannelsOutOfRange, config.MaxPendingChannels.Value));
                    }
                }

                if (config.Ordered != null)
                {
                    reliableSessionElement.Ordered = binding.ReliableSession.Ordered = config.Ordered.Value;
                }
            }

            var newBinding = new CustomBinding(elements)
            {
                CloseTimeout = binding.CloseTimeout,
                OpenTimeout = binding.OpenTimeout,
                ReceiveTimeout = binding.ReceiveTimeout,
                SendTimeout = binding.SendTimeout,
                Name = binding.Name,
                Namespace = binding.Namespace
            };

            return newBinding;
        }

        /// <summary>
        /// Create a .net remoting listener for the server.
        /// </summary>
        /// <param name="cfg"></param>
        private void StartDotNetRemoteListener(MachineConfig.ServerConfig cfg)
        {
            // Create and register tracking handler...
            mTrackingHandler = new TrackingHandler(this);
            TrackingServices.RegisterTrackingHandler(mTrackingHandler);

            // And the detatiled logging handler...
            if (mDynamicProperty == null)
            {
                if (cfg.LogTraffic)
                {
                    mDynamicProperty = new DynamicProperty(this);
                    Context.RegisterDynamicProperty(mDynamicProperty, null, null);
                }
            }

            IDictionary props = new Hashtable();
            props["port"] = cfg.Port;

            if (cfg.ConnectionMode == ServerConnection.Mode.DotNetRemotingSecure)
            {
                props["secure"] = true;
                props["impersonate"] = false;
            }
            else
            {
                props["secure"] = false;
            }

            if (cfg.BindTo != null)
                props["bindTo"] = cfg.BindTo;

            BinaryServerFormatterSinkProvider prov = new BinaryServerFormatterSinkProvider();
            prov.TypeFilterLevel = TypeFilterLevel.Full;

            mChannel = new TcpChannel(props, null, prov);
            ChannelServices.RegisterChannel(mChannel, (cfg.ConnectionMode == ServerConnection.Mode.DotNetRemotingSecure));

            ChannelDataStore data = (ChannelDataStore)mChannel.ChannelData;
            foreach (string uri in data.ChannelUris)
                OnInfo("*Channel URI: " + uri);
        }

        private void StopDotNetRemoteListener()
        {
            // Unregister the channel, meaning we should no longer be serving requests.
            ChannelServices.UnregisterChannel(mChannel);
            mChannel.StopListening(null);

            // Disconnect connected clients...
            foreach (clsServer s in mTrackingHandler.GetClients())
                RemotingServices.Disconnect(s);

            TrackingServices.UnregisterTrackingHandler(mTrackingHandler);
            mTrackingHandler = null;

            if (mDynamicProperty != null)
            {
                Context.UnregisterDynamicProperty(mDynamicProperty.Name, null, null);
                mDynamicProperty = null;
            }
        }

        /// <summary>
        /// Checks the status of the process dependency information. The server can
        /// only startup if this data is valid.
        /// </summary>
        /// <returns></returns>
        private Boolean DependenciesValid()
        {
            Boolean proceed = false;
            int tries = 0;

            while (!proceed)
            {
                //Limit number of attempts to 5. If the dependencies are still not valid
                //after this then throw exception.
                tries += 1;
                if (tries > 5) return false;

                switch (app.gSv.GetDependenciesStatus())
                {
                    case clsServer.DependencyStates.Invalid:
                        //Attempt to refresh dependencies
                        OnInfo("Refreshing dependency information...");
                        app.gSv.RebuildDependencies();
                        break;
                    case clsServer.DependencyStates.Building:
                        //Wait for 5 seconds and retry
                        OnInfo("Waiting for dependency information to be refreshed... try {0}", tries);
                        System.Threading.Thread.Sleep(5000);
                        break;
                    case clsServer.DependencyStates.Valid:
                        //Ok to proceed
                        proceed = true;
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the MI data refresh timer elapsed event.
        /// </summary>
        void mMIAutoRefresh_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (app.gSv.MICheckAutoRefresh()) OnInfo("MI data refreshed.");

                var pipeLineSettings = app.gSv.GetDataPipelineSettings();

                if (pipeLineSettings.SendPublishedDashboardsToDataGateways)
                {
                    app.gSv.SendPublishedDashboardsToDataGateways();
                }

                if (pipeLineSettings.SendWorkQueueAnalysisToDataGateways)
                {
                    app.gSv.SendWqaSnapshotDataToDataGateways();
                }

                if (app.gSv.IsMIReportingEnabled())
                {
                    OnVerbose("Starting Work Queue Snapshot Procedure.");
                    var numberOfqueuesSnapshotted = _snapshotManager.SnapshotWorkQueuesIfRequired();
                    if (numberOfqueuesSnapshotted > 0)
                        OnVerbose($"{numberOfqueuesSnapshotted} Work Queues have been set to snapshot data.");

                }

                app.gSv.UpdateActiveQueueMI();
                
                app.gSv.ClearInternalAuthTokens();
                Log.Debug("Cleared Internal Auth Tokens");
            }
            catch (Exception ex)
            {
                OnErr("Error refreshing MI data: {0}", ex.Message);
            }
            finally
            {
                mMIAutoRefresh.Start();
            }
        }

        /// <summary>
        /// Enables or disables the active listening for database changes within the
        /// scheduler.
        /// </summary>
        /// <param name="enable">true to enable listening, false to disable it.
        /// </param>
        private void EnableSchedulerChangeListening(bool enable)
        {
            DatabaseBackedScheduleStore store = (DatabaseBackedScheduleStore)mScheduler.Store;
            if (enable)
                store.LoadDataAndListenForChanges(SchedulerDBCheckInterval);
            else
                store.StopListeningForChanges();
        }

        /// <summary>
        /// Get the number of 'connected clients'. In reality, this is the number of
        /// remote instances of clsServer that we know about, which does not directly
        /// correspond to the number of actual clients. For example:
        ///  a) an instance of Automate will create two of these during the startup
        ///     sequence, with only one remaining alive for the entire duration of
        ///     the application's lifetime.
        ///  b) closing an instance of Automate will not immediately free up the
        ///     instance - it will only die after its lease expires, which may be
        ///     some time later.
        /// Nonetheless, a return value of 0 means nothing is connected and it is
        /// safe to stop the server. Any other value means it *may* not be.
        /// </summary>
        /// <returns>The current count.</returns>
        public int GetConnectedClients()
        {
            if (!mWCF)
            {
                if (mTrackingHandler == null)
                    return 0;
                return mTrackingHandler.GetConnectedClients();
            }
            else
            {
                return ConnectedClients;
            }
        }
    }
}
