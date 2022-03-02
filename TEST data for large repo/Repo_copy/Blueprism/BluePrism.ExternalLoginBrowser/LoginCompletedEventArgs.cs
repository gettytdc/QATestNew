using System;

namespace BluePrism.ExternalLoginBrowser
{
    public class LoginCompletedEventArgs : EventArgs
    {
        public LoginCompletedEventArgs(string responseBody)
        {
            ResponseBody = responseBody;
        }
        public string ResponseBody { get; }
    }

    public delegate void LoginCompletedHandler(object sender, LoginCompletedEventArgs e);
    
}
