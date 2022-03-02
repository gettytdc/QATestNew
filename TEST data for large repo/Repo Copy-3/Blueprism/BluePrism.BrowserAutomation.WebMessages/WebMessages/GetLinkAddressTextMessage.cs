

namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetLinkAddressTextMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetLinkAddressText;
        }

        public GetLinkAddressTextMessage()
        {
        }

        public GetLinkAddressTextMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetLinkAddressTextMessageResponse : WebMessage<string>
    {
        public string Name => Data;
    }
}
