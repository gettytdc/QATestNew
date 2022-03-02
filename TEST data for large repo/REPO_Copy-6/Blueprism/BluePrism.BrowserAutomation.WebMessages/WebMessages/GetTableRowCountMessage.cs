namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    public class GetTableRowCountMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetTableRowCount;
        }

        public GetTableRowCountMessage()
        {
        }

        public GetTableRowCountMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetTableRowCountResponse : WebMessage<int>
    {
        public int RowCount => Data;
    }
}
