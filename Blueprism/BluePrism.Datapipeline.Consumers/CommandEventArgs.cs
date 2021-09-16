using BluePrism.DataPipeline;

namespace BluePrism.Datapipeline.Logstash
{
    public class CommandEventArgs
    {
        public CommandEventArgs(DataPipelineProcessCommand command)
        {
            Command = command;   
        }

        public DataPipelineProcessCommand Command { get; private set; }
    }
}
