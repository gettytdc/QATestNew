namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class AddToListSelectionMessage : WebMessage<AddToListSelectionMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.AddToListSelection;
        }

        public AddToListSelectionMessage()
        {
        }

        public AddToListSelectionMessage(Guid elementId, int index, string name)
        {
            Data = new AddToListSelectionMessageBody
            {
                ElementId = elementId,
                Index = index,
                Name = name
            };
        }
    }

    public class AddToListSelectionMessageBody
    {
        public Guid ElementId { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
    }
}
