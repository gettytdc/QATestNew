

namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// The API for gathering data on the execution of a Logstash Process.
    /// </summary>
    public interface ILogstashMonitoringApi
    {
        LogstashMonitoringApiResult QueryApi();
    }
}
