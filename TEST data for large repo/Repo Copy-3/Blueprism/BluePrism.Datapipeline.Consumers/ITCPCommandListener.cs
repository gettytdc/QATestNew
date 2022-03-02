using System;

namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// Listens for commands sent to the Logstash process manager.
    /// </summary>
    public interface ITCPCommandListener
    {
        bool IsRunning { get; }

        int ListenPort { get; }

        void Start();
        void Stop();

        event EventHandler<CommandEventArgs> CommandReceived;
    }
}