namespace BluePrism.Api
{
    using System.Linq;
    using NLog;

    public static class LoggingConfiguration
    {
        public static bool TraceLoggingIsEnabled() => LogManager.Configuration.LoggingRules.SelectMany(x => x.Levels).Any(x => x == LogLevel.Trace);
    }
}
