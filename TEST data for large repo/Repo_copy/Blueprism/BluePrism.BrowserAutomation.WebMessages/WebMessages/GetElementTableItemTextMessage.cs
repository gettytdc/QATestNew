namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    public class GetElementTableItemTextMessage : WebMessage<GetElementTableItemTextMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetTableCellText;
        }

        public GetElementTableItemTextMessage()
        {
        }

        public GetElementTableItemTextMessage(Guid elementId, int rowNumber, int columnNumber)
        {
            Data = new GetElementTableItemTextMessageBody()
            {
                ElementId = elementId,
                RowNumber = rowNumber,
                ColumnNumber = columnNumber
            };
        }
    }

    public class GetElementTableItemTextResponse : WebMessage<string>
    {
        public string CellText => Data;
    }

    public class GetElementTableItemTextMessageBody
    {
        public Guid ElementId { get; set; }
        public int RowNumber { get; set; }
        public int ColumnNumber { get; set; }
    }
}
