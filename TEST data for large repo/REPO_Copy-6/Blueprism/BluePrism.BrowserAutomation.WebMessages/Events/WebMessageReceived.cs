using System;

namespace BluePrism.BrowserAutomation.WebMessages.Events
{
    public delegate void WebMessageReceivedDelegate(object sender, WebMessageReceivedDelegateEventArgs args);

    public class WebMessageReceivedDelegateEventArgs : EventArgs
    {
        public WebMessageWrapper Message { get; }

        public WebMessageReceivedDelegateEventArgs(WebMessageWrapper message) => Message = message;
    }
}
