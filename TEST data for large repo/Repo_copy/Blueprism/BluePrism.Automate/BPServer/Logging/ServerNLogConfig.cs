using BluePrism.BPCoreLib;
using NLog;
using System.IO;
using BluePrism.AutomateAppCore;
using NLog.Config;

namespace BluePrism.BPServer.Logging
{
    public static class ServerNLogConfig
    {
        private const string ConfigurationFileName = "Server.NLog.config";

        /// <summary>
        /// Configures logging based on configuration file. This should be called immediately on
        /// application startup.
        /// </summary>
        public static void Configure()
        {
            string filePath = Path.Combine(clsFileSystem.CommonAppDataDirectory, ConfigurationFileName);
            if (File.Exists(filePath))
            {
                LogManager.Configuration = new XmlLoggingConfiguration(filePath, new LogFactory
                {
                    ThrowConfigExceptions = false
                });
            }
        }

        /// <summary>
        /// Initialises the name of the application within NLog's GlobalDiagnostics content. This is used
        /// in the log configuration to generate separate log files for each application (referenced in
        /// NLog.config file as ${gdc:item=AppName}). Additional properties are also added for 
        /// event log sources. This method should be called as soon as possible during application
        /// startup and should be called again with the name of the current configuration when the server
        /// starts. 
        /// </summary>
        public static void SetAppProperties(bool runningAsService, string configurationName = null)
        {
            string name = runningAsService ? "Server Service" : "Server";
            if (configurationName != null)
            {
                name += $" - {configurationName}";
            }
            string eventLogSource = name;
            GlobalDiagnosticsContext.Set("AppName", name);
            GlobalDiagnosticsContext.Set("AppEventLogSource", eventLogSource);
            
            // Used by separate BP Analytics event log target
            string analyticsSource = EventLogHelper.GetAnalyticsSource(configurationName);
            GlobalDiagnosticsContext.Set("AnalyticsSource", analyticsSource);
        }
    }
}