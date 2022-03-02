using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using BluePrism.LoginAgent.Utilities;
using TimeoutException = System.TimeoutException;

namespace LoginAgentService
{    
    /// <summary>
    /// Class representing the Login Agent service.
    /// Primarily, this maintains a child process which is running a Blue Prism
    /// resource when no user is logged on.
    /// </summary>
    public partial class LoginAgentService : ServiceBase
    {
        #region - Member variables -

        private readonly EventLogger _eventLogger;

        // The process under which the resource PC is running.
        Process _resourcePC;

        // The configuration of this service.
        Config _config;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new object representing the Login Agent service.
        /// </summary>
        public LoginAgentService()
        {
            InitializeComponent();
            CanHandleSessionChangeEvent = true;
            _eventLogger = new EventLogger("LoginAgentService");
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The process operating the resource PC on behalf of this service.
        /// </summary>
        private Process ResourcePC
        {
            get
            {
                return _resourcePC;
            }
            set
            {
                if (_resourcePC != null)
                {
                    _resourcePC.OutputDataReceived -= HandleProcessOutput;
                    _resourcePC.ErrorDataReceived -= HandleProcessOutput;
                    _resourcePC.Exited -= HandleProcessExited;
                    _resourcePC = null;
                }
                if (value != null)
                {
                    value.OutputDataReceived += HandleProcessOutput;
                    value.ErrorDataReceived += HandleProcessOutput;
                    value.Exited += HandleProcessExited;
                }
                _resourcePC = value;
            }
        }

        /// <summary>
        /// Checks if the user is currently logged on or not, using WMI.
        /// </summary>
        protected virtual bool IsUserLoggedOn
        {
            get
            {
                ManagementScope scope =
                    new ManagementScope("\\\\localhost", new ConnectionOptions());

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    scope, new ObjectQuery("select * from Win32_ComputerSystem"));

                foreach (ManagementObject result in searcher.Get())
                {
                    string username = result["UserName"] as string;
                    if (username != null)
                        return true;
                }
                return false;
            }
        }

        #endregion

        #region - Event Handler Methods -
        
        /// <summary>
        /// Handles the service starting, ensuring that a resource PC is started if
        /// no user is currently logged on.
        /// </summary>
        /// <param name="args">The arguments for the service.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                LoadConfiguration(args);

                if (!IsUserLoggedOn)
                {
                    _eventLogger.WriteLogEntry("User is not logged on - starting resource");
                    StartResourcePC(_config);
                }
                else
                {
                    _eventLogger.WriteLogEntry("User is logged on - skipping resource");
                }
            }
            catch (Exception e)
            {
                _eventLogger.WriteLogEntry(EventLogEntryType.Error, e.Message);
            }
        }

        /// <summary>
        /// Loads and validates the service configuration.
        /// Passed in path argument takes precedence over the config default path.
        /// </summary>
        /// <param name="args">The arguments for the service.</param>
        private void LoadConfiguration(string[] args)
        {
            
            string cfgFile = args.Length > 0 ? args[0] : Config.DefaultConfigFile;
            _eventLogger.WriteLogEntry("Starting login agent with config file: {0}", cfgFile);

            _config = Config.Load(cfgFile);

            _config.Validate();

            _config.StartupArguments.Add(new Config.Argument("loginagent"));
        }

        /// <summary>
        /// Handles the service being stopped, ensuring that the resource PC is
        /// stopped
        /// </summary>
        protected override void OnStop()
        {
            StopResourcePC();
        }

        /// <summary>
        /// Handles the session changing - ie. a session logging on or off.
        /// </summary>
        /// <param name="desc">The description of the session change.</param>
        protected override void OnSessionChange(SessionChangeDescription desc)
        {
            base.OnSessionChange(desc);
            switch (desc.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    _eventLogger.WriteLogEntry(EventLogEntryType.Information, "Session Logon Event");
                    StopResourcePC();
                    break;

                case SessionChangeReason.SessionLogoff:
                    _eventLogger.WriteLogEntry(EventLogEntryType.Information, "Session Logoff Event");
                    StartResourcePC(_config);
                    break;
                case SessionChangeReason.SessionLock:
                    _eventLogger.WriteLogEntry(EventLogEntryType.Information, "Session Lock Event");
                    break;
                case SessionChangeReason.SessionUnlock:
                    _eventLogger.WriteLogEntry(EventLogEntryType.Information, "Session Unlock Event");
                    break;
                default:
                    _eventLogger.WriteLogEntry(EventLogEntryType.Information, "other event - " + desc.Reason.ToString());
                    break;
            }
        }

        /// <summary>
        /// Handles any output from the process by writing an info windows event.
        /// </summary>
        void HandleProcessOutput(object sender, DataReceivedEventArgs e)
        {
            _eventLogger.WriteLogEntry(EventLogEntryType.Information, e.Data);
        }

        /// <summary>
        /// Handles the resource PC process exiting by writing an entry to the event
        /// log.
        /// </summary>
        void HandleProcessExited(object sender, EventArgs e)
        {
            EventLogEntryType tp = (
                ResourcePC.ExitCode == 0
                ? EventLogEntryType.Information
                : EventLogEntryType.Warning
            );
            _eventLogger.WriteLogEntry(
                tp, "Process exited with exit code: {0}", ResourcePC.ExitCode);
        }

        #endregion
                

        #region - Other Methods -


        /// <summary>
        /// Starts the resource PC specified by the given configuration object.
        /// </summary>
        /// <param name="cfg">The configuration describing how the resource PC should
        /// be started (eg. its working directory, command line params etc.)</param>
        private void StartResourcePC(Config cfg)
        {
            if (ResourcePC == null || ResourcePC.HasExited)
            {
                ResourcePC = Process.Start(cfg.StartInfo);
            }
        }

        /// <summary>
        /// Stops the resource PC process managed by this service, if it is currently
        /// running. No-op if it is not.
        /// </summary>
        private void StopResourcePC()
        {
            if (ResourcePC == null || ResourcePC.HasExited)
                return;

            try
            {
                const int timeout = 45000;
                using (var t = new TcpClient())
                {
                    t.SendTimeout = timeout;
                    t.ReceiveTimeout = timeout;
                    Connect(t, IPAddress.Loopback, _config.ResourcePort, timeout);
                    Stream s = t.GetStream();
                    if (_config.UseSsl)
                    {
                        var ss = new SslStream(s, false,
                            (sender, certificate, chain, errors) => (errors == SslPolicyErrors.None),
                            null);
                        ss.AuthenticateAsClient(Environment.MachineName);
                        s = ss;
                    }

                    using (var sw = new StreamWriter(s, Encoding.ASCII))
                    using (var sr = new StreamReader(s, Encoding.ASCII))
                    {
                        sw.WriteLine("shutdown waitforsessions");
                        sw.Flush();
                        var line = sr.ReadLine();
                        if (line == null || !line.StartsWith("OK"))
                            throw new TimeoutException("Unexpected reply asking resource pc to exit");
                    }
                }
                //Ensure the process exits.
                if (!ResourcePC.WaitForExit(timeout))
                    throw new TimeoutException("Timeout waiting for resource pc to exit");
            }
            catch
            {
                //If all else fails kill the resourcepc
                ResourcePC.Kill();
            }
        }

        /// <summary>
        /// Attempts to connect the given TCP Client to the specified host/port,
        /// timing out after <paramref name="timeout"/> milliseconds.
        /// </summary>
        /// <param name="tcp">The client to connect</param>
        /// <param name="host">The host to connect to</param>
        /// <param name="port">The port on the host to connect to</param>
        /// <param name="timeout">The number of milliseconds to wait before timing
        /// out and failing the connection.</param>
        private void Connect(TcpClient tcp, IPAddress host, int port, int timeout)
        {
            IAsyncResult ar = tcp.BeginConnect(host, port, null, null);
            using (WaitHandle wh = ar.AsyncWaitHandle)
            {
                if (!wh.WaitOne(timeout, false))
                    throw new TimeoutException("Timeout connecting to resource pc");
                tcp.EndConnect(ar);
            }
        }

        #endregion


    }

}
