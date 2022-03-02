using BluePrism.Datapipeline.Logstash.Configuration.PreprocessorTasks;
using System.Collections.Generic;
using System.Linq;


namespace BluePrism.Datapipeline.Logstash.Configuration
{
    public class ConfigurationPreprocessor : IConfigurationPreprocessor
    {

        private List<ConfigurationPreprocessorTask> _preprocessorTasks;

        public ConfigurationPreprocessor(List<ConfigurationPreprocessorTask> preprocessorTasks)
        {
            
            _preprocessorTasks = preprocessorTasks;
        }


        public string ProcessConfiguration(string configuration)
        {
            return _preprocessorTasks.Aggregate(configuration, (config, x) => x.ProcessConfiguration(config));
        }

    }
}
