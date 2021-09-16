using System.Collections.Generic;

namespace BluePrism.DataPipeline.UI
{
    public class DataPipelineSettingsDetails
    {
        #region "public properties"

        public bool WriteSessionLogsToDatabase { get; set; }

        public bool EmitSessionLogsToDatagateways { get; set; }

        public int MonitoringFrequency { get; set; }

        public bool SendPublishedDashboardsToDatagateways { get; set; }

        public List<PublishedDashboardSettings> PublishedDashboardSettings { get; set; } = new List<PublishedDashboardSettings>();

        public bool SendWorkQueueAnalysisToDatagateways { get; set; }

        public string DatabaseSqlUser { get; set; } = string.Empty;

        public bool UseIntegratedSecurity { get; set; }

        public int ServerPort { get; set; }

        #endregion

        #region "public methods"
        public static DataPipelineSettingsDetails FromSettings(DataPipelineSettings settings)
        {
            if (settings is null) return null;
            return new DataPipelineSettingsDetails() {
                WriteSessionLogsToDatabase = settings.WriteSessionLogsToDatabase,
                EmitSessionLogsToDatagateways = settings.SendSessionLogsToDataGateways,
                MonitoringFrequency = settings.MonitoringFrequency,
                SendPublishedDashboardsToDatagateways = settings.SendPublishedDashboardsToDataGateways,
                PublishedDashboardSettings = settings.PublishedDashboardSettings,
                SendWorkQueueAnalysisToDatagateways = settings.SendWorkQueueAnalysisToDataGateways,
                DatabaseSqlUser = settings.DatabaseUserCredentialName,
                UseIntegratedSecurity  = settings.UseIntegratedSecurity,
                ServerPort = settings.ServerPort
            };
        }

        public static DataPipelineSettings ToSettings(DataPipelineSettingsDetails settings)
        {
            if (settings is null) return null;
            return new DataPipelineSettings(settings.WriteSessionLogsToDatabase, settings.EmitSessionLogsToDatagateways,
                                            settings.MonitoringFrequency, settings.SendPublishedDashboardsToDatagateways,
                                            settings.PublishedDashboardSettings, settings.SendWorkQueueAnalysisToDatagateways,
                                            settings.DatabaseSqlUser,settings.UseIntegratedSecurity,settings.ServerPort);
        }
        #endregion
    }
}
