using System;
using BluePrism.BPServer.Enums;
using NLog;

namespace BluePrism.BPServer.Utility
{
    internal static class Extensions
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        /// <returns>Null if the given LoggingLevel is not recognised by NLog</returns>
        internal static LogLevel ToNLogLevel(this LoggingLevel lvl)
        {
            try
            {
                return LogLevel.FromOrdinal((int)lvl);
            }
            catch(ArgumentException)
            {
                Log.Error($"ToNLogLevel - Invalid LoggingLevel {lvl}, defaulting to Warn.");
                return LogLevel.Warn;
            }
        }
    }
}
