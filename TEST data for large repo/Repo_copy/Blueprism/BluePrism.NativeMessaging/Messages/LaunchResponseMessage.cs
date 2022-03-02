using BluePrism.NativeMessaging.Interfaces;
using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
    public class LaunchResponseMessage : IResponseMessage
    {
        [JsonProperty("name")]
        public const string Name = "Launch";
        [JsonProperty("urls")]
        public string Urls { get; }

        public LaunchResponseMessage(string urls)
        {
            Urls = urls;
        }
    }
}
