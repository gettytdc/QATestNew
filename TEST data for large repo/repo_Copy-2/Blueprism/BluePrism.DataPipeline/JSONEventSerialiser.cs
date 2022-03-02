using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace BluePrism.DataPipeline
{
    /// <summary>
    /// Serialises a data pipeline event to JSON
    /// </summary>
    public class JSONEventSerialiser : IEventSerialiser
    {
        public string SerialiseEvent(DataPipelineEvent dataPipelineEvent)
        {
            return JsonConvert.SerializeObject( dataPipelineEvent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Context = new StreamingContext(StreamingContextStates.All, "datagateway") });
        }
    }
}
