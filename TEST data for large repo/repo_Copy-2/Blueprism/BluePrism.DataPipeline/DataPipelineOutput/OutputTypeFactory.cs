using System;

namespace BluePrism.DataPipeline.DataPipelineOutput
{
    public static class OutputTypeFactory
    {
        public static OutputType GetOutputType(string id)
        {
            switch (id)
            {
                case "file":
                    return new OutputType("File", id);
                case "http":
                    return new OutputType("HTTP Endpoint", id);
                case "database":
                    return new DatabaseOutput("Database", id);
                case "splunk":
                    return new SplunkOutput("Splunk", id);
            }
            throw new ArgumentException();
        }
    }
}