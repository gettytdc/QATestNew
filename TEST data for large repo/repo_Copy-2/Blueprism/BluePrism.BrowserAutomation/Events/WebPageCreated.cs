using System;

namespace BluePrism.BrowserAutomation.Events
{
    public delegate void WebPageCreatedDelegate(object sender, WebPageCreatedEventArgs args);

    public class WebPageCreatedEventArgs : EventArgs 
    {
        public Version ExtensionVersion { get;  }

        public WebPageCreatedEventArgs(Version extensionVersion) => ExtensionVersion = extensionVersion;
    }
}
