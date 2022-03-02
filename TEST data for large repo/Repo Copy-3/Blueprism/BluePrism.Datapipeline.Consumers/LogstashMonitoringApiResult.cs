

namespace BluePrism.Datapipeline.Logstash
{
    public class LogstashMonitoringApiResult
    {
        public LogstashMonitoringApiResult(int eventsIn, int eventsOut)
        {
            EventsIn = eventsIn;
            EventsOut = eventsOut;
        }

        /// <summary>
        /// Number of events pulled in to the Logstash pipeline.
        /// </summary>
        public int EventsIn { get; }

        /// <summary>
        /// Number of events pushed to outputs.
        /// </summary>
        public int EventsOut { get; }
    }
}
