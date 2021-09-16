using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using BluePrism.Data;

namespace BluePrism.DataPipeline
{
    public class DataPipelinePublisher : IDataPipelinePublisher
    {
        private readonly string _hostName = Dns.GetHostName();
        private readonly IEventSerialiser _serializer;

        public DataPipelinePublisher(IEventSerialiser serializer)
        {
            _serializer = serializer;
        }

        public void PublishToDataPipeline(IDatabaseConnection connection, IList<DataPipelineEvent> pipelineEvents)
        {
            foreach (var e in pipelineEvents)
            {
                var serialisedEvent = _serializer.SerialiseEvent(e);
                AddEventToDatabase(connection, e.EventType, serialisedEvent);
            }
        }

        private void AddEventToDatabase(IDatabaseConnection connection, EventType eventType, string serialisedEvent)
        {
            var sql =
                "insert into BPADataPipelineInput(eventtype, eventdata, publisher) values(@eventtype, @eventdata, @publisher);";

            using (var cmd = new SqlCommand(sql))
            {
                cmd.Parameters.AddWithValue("eventtype", eventType);
                cmd.Parameters.AddWithValue("eventdata", serialisedEvent);
                cmd.Parameters.AddWithValue("publisher", _hostName);
                connection.Execute(cmd);
            }
        }
    }
}
