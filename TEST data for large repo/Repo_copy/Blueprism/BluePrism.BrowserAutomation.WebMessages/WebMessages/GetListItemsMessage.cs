namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    using System.Collections.Generic;
    using Data;

    public class GetListItemsMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetListItems;
        }

        public GetListItemsMessage()
        {
        }

        public GetListItemsMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetListItemsResponse : WebMessage<ListItem[]>
    {
        public IReadOnlyCollection<ListItem> Items => Data;
    }
}
