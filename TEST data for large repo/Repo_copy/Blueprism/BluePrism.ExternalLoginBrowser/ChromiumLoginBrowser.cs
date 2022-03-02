using System;
using CefSharp;
using CefSharp.WinForms;

namespace BluePrism.ExternalLoginBrowser
{
    internal class ChromiumLoginBrowser : IChromiumLoginBrowser
    {
        public ChromiumLoginBrowser(ChromiumWebBrowser browser, IExternalLoginRequestHandler externalLoginRequestHandler)
        {
            WebBrowser = browser;

            WebBrowser.RequestHandler = externalLoginRequestHandler;

            WebBrowser.LoadingStateChanged += (_, e) 
                => LoadingStateChanged?.Invoke(this, e);

            WebBrowser.IsBrowserInitializedChanged += (_, e) 
                => IsBrowserInitializedChanged?.Invoke(this, e);

            WebBrowser.LoadError += (_, e) 
                => LoadError?.Invoke(this, e);

            WebBrowser.TitleChanged += (_, e) 
                => TitleChanged?.Invoke(this, e);

            externalLoginRequestHandler.LoginCompleted += (_, e)
                => LoginCompleted?.Invoke(this, e);

            externalLoginRequestHandler.LoginFailed += (_, e)
                => LoginFailed?.Invoke(this, e);

        }

        public bool UseWaitCursor { get => WebBrowser.UseWaitCursor; set => WebBrowser.UseWaitCursor = value; }
                
        public ChromiumWebBrowser WebBrowser { get; }

        public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;
        public event EventHandler<IsBrowserInitializedChangedEventArgs> IsBrowserInitializedChanged;
        public event EventHandler<LoadErrorEventArgs> LoadError;
        public event EventHandler<TitleChangedEventArgs> TitleChanged;
        public event LoginCompletedHandler LoginCompleted;
        public event LoginFailedHandler LoginFailed;

        public void Focus()
            => WebBrowser.Focus();        

        public void Dispose()
            => WebBrowser.Dispose();

        public void Load(string url)
            => WebBrowser.Load(url);
        
    }
}
