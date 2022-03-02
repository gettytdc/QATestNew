namespace BluePrism.DataPipeline
{
    public class DataPipelineProcess
    {
        public DataPipelineProcess(string name, DataGatewayProcessStatus status, DataPipelineProcessConfig config)
        {
            Name = name;
            Status = status;
            Config = config;
        }

        public string Name { get; }
        public DataGatewayProcessStatus Status { get; }
        public DataPipelineProcessConfig Config { get; }

    }
}
