

using BluePrism.Common.Security;
using System;

namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// Interface to a Logstash Process.
    /// </summary>
    public interface ILogstashProcess
    {
        void Start(LogstashProcessSettings logstashConfiguration, SafeString secretStorePassword);
        void Stop();

        (LogstashProcessState state, string stateMessage) State { get; }

        event EventHandler<(LogstashProcessState state, string stateMessage)> StateChanged;
    }
}
