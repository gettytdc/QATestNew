

namespace BluePrism.Datapipeline.Logstash
{
    public class LogstashProcessStateResult
    {
        public LogstashProcessStateResult(LogstashProcessState state, string message)
        {
            State = state;
            StateMessage = message;
        }

        public LogstashProcessState State { get; }
        public string StateMessage { get; }

    }
}
