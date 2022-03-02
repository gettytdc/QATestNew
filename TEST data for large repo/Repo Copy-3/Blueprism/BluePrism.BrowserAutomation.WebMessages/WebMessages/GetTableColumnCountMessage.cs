namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    public class GetTableColumnCountMessage : WebMessage<GetTableColumnCountMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetTableColumnCount;
        }

        public GetTableColumnCountMessage()
        {
        }

        public GetTableColumnCountMessage(Guid elementId, int rowIndex)
        {
            Data = new GetTableColumnCountMessageBody()
            {
                ElementId = elementId,
                RowIndex = rowIndex
            };
        }
    }

    public class GetTableColumnCountResponse : WebMessage<int>
    {
        public int ColumnCount => Data;
    }

    public class GetTableColumnCountMessageBody
    {
        public Guid ElementId { get; set; }
        public int RowIndex { get; set; }
    }
}
