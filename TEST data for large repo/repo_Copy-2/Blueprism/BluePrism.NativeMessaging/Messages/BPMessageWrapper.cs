using BluePrism.NativeMessaging.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BluePrism.NativeMessaging.Messages
{
    public class BPMessageWrapper
    {
        [JsonProperty("data")]
        public IBPMessage Data { get; }

        public BPMessageWrapper(IBPMessage data)
        {
            Data = data;
        }

        public JObject GetJObject()
        {
            return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(this));
        }
    }
}
