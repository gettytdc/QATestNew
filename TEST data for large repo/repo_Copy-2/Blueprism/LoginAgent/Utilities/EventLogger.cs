using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace BluePrism.LoginAgent.Utilities
{
    public class EventLogger
    {
        private readonly string _source;
     
        public EventLogger(string eventLogSource)
        {
            _source = eventLogSource;
        }

        public void WriteLogEntry(string formatMessage, params object[] arguments)
        {
            WriteLogEntry(string.Format(formatMessage, arguments));
        }
                
        public void WriteLogEntry(EventLogEntryType type, string formatMessage, params object[] arguments)
        {
            WriteLogEntry(type, string.Format(formatMessage, arguments));
        }

        public void WriteLogEntry(string message)
        {
            WriteLogEntry(EventLogEntryType.Information, message);
        }

        private void WriteLogEntry(EventLogEntryType type, string message)
        {
            var eventLogName = "Blue Prism";

            if (!EventLog.SourceExists(_source))
            {
                var sourceData = new EventSourceCreationData(_source, eventLogName);
                EventLog.CreateEventSource(sourceData);
            }
            else
            {
                eventLogName = EventLog.LogNameFromSourceName(_source, ".");
            }

            EventLog eventLog = new EventLog(eventLogName, ".", _source);
            eventLog.WriteEntry(message, type);
        }


    }
}
