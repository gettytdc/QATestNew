namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    using Data;

    public class GetSliderRangeMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetSliderRange;
        }

        public GetSliderRangeMessage()
        {
        }

        public GetSliderRangeMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetSliderRangeResponse : WebMessage<SliderRange>
    {
        public SliderRange Range => Data;
    }
}
