using System;

namespace BluePrism.NamedPipes
{
    public delegate void MessageReceivedDelegate(object sender, MessageReceivedDelegateEventArgs args);

    public class MessageReceivedDelegateEventArgs : EventArgs
    {
        public object Message { get; }

        public MessageReceivedDelegateEventArgs(object message) => Message = message;
    }   
}
