using BluePrism.BPCoreLib;

namespace BluePrism.Datapipeline.Logstash
{
    public class LogstashProcessFactory : ILogstashProcessFactory
    {
        private readonly IEventLogger _eventLogger;

        public LogstashProcessFactory(IEventLogger logger)
        {
            _eventLogger = logger;
        }

        public ILogstashProcess CreateProcess()
        {
            return new LogstashProcess(new ProcessFactory(), 
                                    new JavaProcessHelper(), 
                                    _eventLogger ?? new NLogEventLogger());
        } 
    }
}
