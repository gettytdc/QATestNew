using Newtonsoft.Json;

namespace BluePrism.NamedPipes.Responses
{
    public class ConnectionResponse
    {
        [JsonConstructor]
        public ConnectionResponse(string pipeId) => PipeId = pipeId;

        public string PipeId { get; }
    }
}
