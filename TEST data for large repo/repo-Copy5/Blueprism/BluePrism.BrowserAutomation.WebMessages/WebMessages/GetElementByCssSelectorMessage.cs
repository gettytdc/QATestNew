namespace BluePrism.BrowserAutomation.WebMessages
{
    public class GetElementByCssSelectorMessage : WebMessage<string>
    {
        public override MessageType MessageType => MessageType.GetElementByCssSelector;

        public GetElementByCssSelectorMessage()
        {
            //Needed by JsonConvert.SerializeObject
        }

        public GetElementByCssSelectorMessage(string path)
        {
            Data = path;
        }
    }

    public class GetElementByCssSelectorResponse : BaseElementResponse
    {
    }
}
