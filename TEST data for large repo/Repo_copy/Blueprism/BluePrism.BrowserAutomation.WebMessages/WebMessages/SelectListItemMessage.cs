namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class SelectListItemMessage : WebMessage<SelectListItemMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.SelectListItem;
        }

        public SelectListItemMessage()
        {
        }

        public SelectListItemMessage(Guid elementId, int index, string name)
        {
            Data = new SelectListItemMessageBody
            {
                ElementId = elementId,
                Index = index,
                Name = name
            };
        }
    }

    public class SelectListItemMessageBody
    {
        public Guid ElementId { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
    }
}
