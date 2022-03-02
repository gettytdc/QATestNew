namespace BluePrism.BrowserAutomation.Events
{
    using System;

    public delegate void ElementHoverDelegate(object sender, ElementHoverArgs args);

    public class ElementHoverArgs : EventArgs
    {
        public Guid ElementId { get; }
        public IWebElement Element { get; }

        public ElementHoverArgs(Guid elementId)
            : this(elementId, null)
        {
        }

        public ElementHoverArgs(Guid elementId, IWebElement element)
        {
            ElementId = elementId;
            Element = element;
        }
    }

}