using System;

namespace BluePrism.BrowserAutomation.Events
{
    public delegate void NativeMessagingHostNotFoundDelegate(object sender, NativeMessagingHostNotFoundEventArgs args);

    public class NativeMessagingHostNotFoundEventArgs : EventArgs
    {
        public bool HostNotFound { get; set; }

        public NativeMessagingHostNotFoundEventArgs(bool hostNotFound) => HostNotFound = hostNotFound;
    }
}
