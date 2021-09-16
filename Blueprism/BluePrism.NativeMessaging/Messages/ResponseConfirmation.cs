using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BluePrism.NativeMessaging.Interfaces;

namespace BluePrism.NativeMessaging.Messages
{
    public class ResponseConfirmation
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public JObject Data { get; set; }

        public ResponseConfirmation(JObject data)
        {
            Data = data;
            Message = "Confirmation of received data";
        }

        public ResponseConfirmation(string message)
        {
            Message = message;
        }

        public ResponseConfirmation(IResponseMessage message)
        {
            Message = JsonConvert.SerializeObject(message);
        }

        public JObject GetJObject()
        {
            return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(this));
        }
    }
}
