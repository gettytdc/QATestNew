using System.Collections.Generic;

namespace BluePrism.DataPipeline
{
    public class DataPipelineEvent
    {
        public DataPipelineEvent(EventType eventType)
        {
            EventType = eventType;
        }

        public EventType EventType { get; }

        public Dictionary<string, object> EventData { get; set; } = new Dictionary<string, object>();
    }
}
