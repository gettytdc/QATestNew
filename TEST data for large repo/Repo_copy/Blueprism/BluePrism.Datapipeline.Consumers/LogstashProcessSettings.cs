using BluePrism.Common.Security;

namespace BluePrism.Datapipeline.Logstash
{

    public class LogstashProcessSettings
    {
        public string ConfigurationPath { get; set; }
        public string ConfigurationDirectory { get; set; }
        public string Domain { get; set; }
        public string UserName { get; set; }
        public SafeString Password { get; set; }
        public bool TraceLogging { get; set; }
        public string LogstashPath { get; set; }
        public string LogstashDirectory { get; set; }
    }
}
