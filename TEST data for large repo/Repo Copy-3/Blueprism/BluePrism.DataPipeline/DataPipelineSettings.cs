using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BluePrism.DataPipeline
{
    [Serializable, DataContract(Namespace = "bp")]
    public class DataPipelineSettings
    {
        [DataMember]
        private readonly bool _writeSessionLogsToDatabase;
        [DataMember]
        private readonly bool _emitSesionLogsToDatagateways;
        [DataMember]
        private readonly int _monitoringFrequency;

        [DataMember]
        private readonly bool _sendPublishedDashboardsToDatagateways;

        [DataMember]
        private readonly List<PublishedDashboardSettings> _dashboardSettings = new List<PublishedDashboardSettings>();

        [DataMember]
        private readonly bool _sendWorkQueueAnalysisToDataGateways;

        [DataMember]
        private readonly int minIntervalInSeconds = 300;

        [DataMember]
        private readonly int maxIntervalInSeconds = 86400;

        [DataMember]
        private readonly string _credentialName = "";

        [DataMember]
        private readonly bool _useIntegratedSecurity;

        [DataMember] private readonly int _serverPort;

        public DataPipelineSettings() { }

        public DataPipelineSettings(bool writeSessionLogsToDatabase, bool emitSessionLogsToDatagateways,
                                    int monitoringFrequency, bool sendPublishedDashboardsToDatagateways, List<PublishedDashboardSettings> dashboardSettings,
                                    bool sendWorkQueueAnalysisToDataGateways, string databaseUserCredentialName,
                                    bool useIntegratedSecurity,int sqlServerPort)
        {
            _writeSessionLogsToDatabase = writeSessionLogsToDatabase;
            _emitSesionLogsToDatagateways = emitSessionLogsToDatagateways;
            _monitoringFrequency = monitoringFrequency;
            _sendPublishedDashboardsToDatagateways = sendPublishedDashboardsToDatagateways;
            _dashboardSettings = dashboardSettings;
            _sendWorkQueueAnalysisToDataGateways = sendWorkQueueAnalysisToDataGateways;
            _credentialName = databaseUserCredentialName;
            _useIntegratedSecurity = useIntegratedSecurity;
            _serverPort = sqlServerPort;
        }

        public bool WriteSessionLogsToDatabase => _writeSessionLogsToDatabase;
        public bool SendSessionLogsToDataGateways => _emitSesionLogsToDatagateways;
        public int MonitoringFrequency => _monitoringFrequency;
        public bool SendPublishedDashboardsToDataGateways => _sendPublishedDashboardsToDatagateways;

        public List<PublishedDashboardSettings> PublishedDashboardSettings => _dashboardSettings;
        public bool SendWorkQueueAnalysisToDataGateways => _sendWorkQueueAnalysisToDataGateways;

        public string DatabaseUserCredentialName => _credentialName;
        public bool UseIntegratedSecurity => _useIntegratedSecurity;
        public int ServerPort => _serverPort;

        public void Validate()
        {
            if (!_writeSessionLogsToDatabase && !_emitSesionLogsToDatagateways)
                throw new ArgumentOutOfRangeException(Properties.Resources.dataPipelineSettings_InvalidSettings);

            if (_dashboardSettings.Any(x => x.PublishToDataGatewayInterval < minIntervalInSeconds || x.PublishToDataGatewayInterval > maxIntervalInSeconds))
                throw new ArgumentOutOfRangeException(Properties.Resources.dataPipelineSettings_InvalidDashboardInterval);

            if (!UseIntegratedSecurity && string.IsNullOrWhiteSpace(DatabaseUserCredentialName))
                throw new ArgumentException(Properties.Resources.dataPipelineSettings_InvalidSettingsNoValidUserCredentialsDefined);

            if(_serverPort <= 0)
                throw new ArgumentOutOfRangeException(Properties.Resources.dataPipelineSettings_InvalidPortSelection);

        }
    }
}
