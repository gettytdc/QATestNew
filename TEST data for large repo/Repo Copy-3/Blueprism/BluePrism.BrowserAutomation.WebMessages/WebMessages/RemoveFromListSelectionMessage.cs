namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class RemoveFromListSelectionMessage : WebMessage<RemoveFromListSelectionMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.RemoveFromListSelection;
        }

        public RemoveFromListSelectionMessage()
        {
        }

        public RemoveFromListSelectionMessage(Guid elementId, int index, string name)
        {
            Data = new RemoveFromListSelectionMessageBody
            {
                ElementId = elementId,
                Index = index,
                Name = name
            };
        }
    }

    public class RemoveFromListSelectionMessageBody
    {
        public Guid ElementId { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
    }
}
