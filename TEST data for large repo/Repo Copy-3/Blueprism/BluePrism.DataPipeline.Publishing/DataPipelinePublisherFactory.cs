using BluePrism.AutomateAppCore;
using BluePrism.Core.Network;


namespace BluePrism.DataPipeline.Publishing
{
    public class DataPipelinePublisherFactory : IDataPipelinePublisherFactory
    {
        string _hostname;
        string _connectionString;

        public DataPipelinePublisherFactory(IDNSService dns)
        {
            _hostname = dns.GetHostName();
            _connectionString = clsOptions.DBConnectionSetting.GetConnectionString();
        }

        public IDataPipelinePublisher CreateDataPipelinePublisher()
        {
            // if publishing events to database is enabled, return a database publisher.
            return new DatabasePublisher(new JSONEventSerialiser(), _hostname, _connectionString);

            // if publishing to message queue, return a message queue publisher.
            // ...

            // if neither are enabled... return a null publisher?
        }
    }
}
