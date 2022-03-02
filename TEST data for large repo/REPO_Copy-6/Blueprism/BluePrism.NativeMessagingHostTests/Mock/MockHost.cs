using BluePrism.NativeMessaging.Implementations;
using Newtonsoft.Json.Linq;

namespace BluePrism.NativeMessagingHostTests.Mock
{
    internal class MockNativeMessagingHost : BrowserHost
    {
        public JObject MessageSent { get; set; }

        public override JObject Read() => new JObject();

        public override bool SendMessage(JObject data)
        {
            MessageSent = data;               
            return true;
        }
    }
}
