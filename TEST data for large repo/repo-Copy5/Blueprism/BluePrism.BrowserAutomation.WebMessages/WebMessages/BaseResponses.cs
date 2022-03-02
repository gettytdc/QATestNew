namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    using System.Drawing;
    using Utilities.Functional;

    public abstract class BaseElementResponse : WebMessage<Guid?>
    {
        public Guid? ElementId => Data;
    }

    public abstract class BaseElementsResponse : WebMessage<Guid[]>
    {
        public Guid[] ElementIds => Data;
    }

    public class BoundsMessageBody
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public abstract class BaseBoundsResponse : WebMessage<BoundsMessageBody>
    {
        public Rectangle Bounds =>
            Data?.Map(x => new Rectangle((int)x.X, (int)x.Y, (int)x.Width, (int)x.Height))
            ?? new Rectangle(-1, -1, 0, 0);
    }
}
