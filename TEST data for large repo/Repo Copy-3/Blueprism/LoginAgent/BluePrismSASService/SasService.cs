using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using BluePrism.LoginAgent.Utilities;
using System.Security.Principal;

namespace BluePrism.LoginAgent.Sas
{
    public partial class SasService : ServiceBase
    {
        readonly EventLogger _eventLogger;

        ServiceConfiguration _serviceConfiguration;
       
        /// <summary>
        /// Class data for connection to robot.
        /// </summary>
        SasListener _sasListener = null;

        public SasService()
        {
            InitializeComponent();
            _eventLogger = new EventLogger("BluePrismSASService");
        }

        protected override void OnStart(string[] args)
        {            
            try
            {   
                _eventLogger.WriteLogEntry(EventLogEntryType.Information, $"Starting SAS service with config file: {ServiceConfiguration.DefaultConfigFile}");

                _serviceConfiguration = ServiceConfiguration.LoadConfigFromDefaultLocation();

                CheckServiceRunningUnderSystemAccount();
                OverrideGroupPolicy();
                SetUpSasListener();

            }
            catch (Exception e)
            {
                _eventLogger.WriteLogEntry(EventLogEntryType.Error, $"Error starting SAS service. {e}");
            }               
            
        }

        protected override void OnStop()
        {
            _eventLogger.WriteLogEntry(EventLogEntryType.Information, "Shutting down SAS Service");
            _sasListener.ShutDown();
            OverrideGroupPolicy();
        }

        private void CheckServiceRunningUnderSystemAccount()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                if (!identity.IsSystem)
                    _eventLogger.WriteLogEntry(EventLogEntryType.Warning,
                        $"Service is not running as a local system account. Current identity :  {identity.Name}");
            }
        }

        private void OverrideGroupPolicy()
        {
            var groupPolicyOverride = new GroupPolicyOverride(_serviceConfiguration.OverrideLegalMessageGroupPolicy,
                                                              _serviceConfiguration.OverrideSasGroupPolicy, 
                                                              _eventLogger);
            groupPolicyOverride.SetGroupPolicy();
        }

        private void SetUpSasListener()
        {
            if (!_serviceConfiguration.SendSecureAttentionSequence) return;

            _sasListener = new SasListener(_eventLogger);
            Task.Factory.StartNew(() => _sasListener.ListenForMessage("BluePrismSASPipe", ProcessSasPipeMessage));
        }


        string ProcessSasPipeMessage(string message)
        {
            if (message.Length < 3) return string.Empty;
            return (message.StartsWith("SAS", StringComparison.Ordinal)) ? SendSecureAttentionSequence() : string.Empty;
            
        }

        string SendSecureAttentionSequence()
        {
            _eventLogger.WriteLogEntry(EventLogEntryType.Information, "Sending SAS");
            SasLibrary.SendSAS(false);
            return "OK";
        }            


    }


}
