using System;
using System.Net;

namespace BluePrism.ExternalLoginBrowser
{
    public class LoginFailedEventArgs : EventArgs
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public LoginFailedEventArgs(HttpStatusCode httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;
        }
    }

    public delegate void LoginFailedHandler(object sender, LoginFailedEventArgs e);
    
}
