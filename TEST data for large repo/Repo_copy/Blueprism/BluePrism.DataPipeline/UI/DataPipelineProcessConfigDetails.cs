namespace BluePrism.DataPipeline
{
    public class DataPipelineProcessConfigDetails
    {
        public DataPipelineProcessConfigDetails() : this(0) { }

        private DataPipelineProcessConfigDetails(int id)
        {
            Id = id;
        }

        public readonly int Id;
        public string Name { get; set; }
        public string LogstashConfigFile { get; set; }
        public bool IsCustom { get; set; }

        public static DataPipelineProcessConfigDetails FromConfig(DataPipelineProcessConfig config)
        {
            if (config is null) return null;
            return new DataPipelineProcessConfigDetails(config.Id) {Name = config.Name, LogstashConfigFile = config.LogstashConfigFile, IsCustom = config.IsCustom };
        }

        public static DataPipelineProcessConfig ToConfig(DataPipelineProcessConfigDetails config)
        {
            if (config is null) return null;
            return new DataPipelineProcessConfig(config.Id,config.Name, config.LogstashConfigFile, config.IsCustom);
        }
    }
}
