
using System.Data.SqlClient;


namespace BluePrism.DataPipeline.Publishing
{
    /// <summary>
    /// Publishes events to the BPADataPipelineInput table in the Blue Prism database.
    /// </summary>
    public class DatabasePublisher : IDataPipelinePublisher
    {
        string _publisherName;
        string _connectionString;
        IEventSerialiser _eventSerialiser;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventSerialiser">Serialiser to serialise the event data into the required format</param>
        /// <param name="hostname">Hostname of the machine publishing the events</param>
        /// <param name="connectionString">Connection string to the Blue Prism database</param>
        public DatabasePublisher(IEventSerialiser eventSerialiser, string hostname, string connectionString)
        {
            _eventSerialiser = eventSerialiser;
            _publisherName = hostname;
            _connectionString = connectionString;
        }


        public void PublishEvent(DataPipelineEvent dataPipelineEvent)
        {
            string serialisedEvent = _eventSerialiser.SerialiseEvent(dataPipelineEvent);
            PublishEventToPipeline(dataPipelineEvent.EventType, serialisedEvent);
        }


        private void PublishEventToPipeline(EventType eventType, string serialisedEvent)
        {
            string sql = "insert into BPADataPipelineInput(eventtype, eventdata, publisher) values(@eventtype, @eventdata, @publisher);";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("eventtype", eventType);
                    cmd.Parameters.AddWithValue("eventdata", serialisedEvent);
                    cmd.Parameters.AddWithValue("publisher", _publisherName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
