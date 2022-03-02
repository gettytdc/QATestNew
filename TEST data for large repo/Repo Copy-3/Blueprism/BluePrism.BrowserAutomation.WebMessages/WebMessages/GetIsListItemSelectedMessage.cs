namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetIsListItemSelectedMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetIsListItemSelected;
        }

        public GetIsListItemSelectedMessage()
        {
        }

        public GetIsListItemSelectedMessage(Guid elementId)
        {
            Data = elementId;

        }
    }


    public class GetIsListItemSelectedResponse : WebMessage<bool>
    {
        public bool IsSelected => Data;
    }
}
