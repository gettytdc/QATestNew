using System;

namespace BluePrism.BrowserAutomation.Events
{
    public class PipeMessageReceived
    {
        public delegate void PipeMessageReceivedDelegate(object sender, PipeMessageReceivedDelegateEventArgs args);

        public class PipeMessageReceivedDelegateEventArgs : EventArgs
        {
            public string Message { get; }

            public PipeMessageReceivedDelegateEventArgs(string message) => Message = message;
        }
    }
}
