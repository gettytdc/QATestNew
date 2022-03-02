

namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// Creates ILogstashProcess objects
    /// </summary>
    public interface ILogstashProcessFactory
    {
        /// <summary>
        /// Creates a logstash process instance
        /// </summary>
        /// <returns></returns>
        ILogstashProcess CreateProcess();
    }
}
