using NLog;

namespace BluePrism.BPServer.Logging
{
    /// <summary>
    /// Logs data from published dashboards - the default log configuration includes a
    /// target that logs these messages to a separate "BP Analytics" Windows event log 
    /// </summary>
    public static class AnalyticsLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Log(string message)
        {
            Logger.Info(message);
        }
    }
}