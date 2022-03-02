namespace BluePrism.Logging
{
    using System.Collections.Generic;
    using NLog;

    public class LoggerFactory : ILoggerFactory
    {
        private readonly IDictionary<string, NLog.ILogger> _loggers = new Dictionary<string, NLog.ILogger>();

        public NLog.ILogger GetLogger(string name) =>
            _loggers.ContainsKey(name)
            ? _loggers[name]
            : _loggers[name] = LogManager.GetLogger(name);
    }
}
