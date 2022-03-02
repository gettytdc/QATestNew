namespace BluePrism.BrowserAutomation.WebMessages
{
    using System.Drawing;
    using Utilities.Functional;

    public class GetCursorPositionMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetCursorPosition;
        }
    }

    public class GetCursorPositionResponse : WebMessage<GetCursorPositionResponseBody>
    {
        public Point CursorPosition =>
            Data?.Map(x => new Point(x.X, x.Y))
            ?? new Point(-1, -1);
    }

    public class GetCursorPositionResponseBody
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
