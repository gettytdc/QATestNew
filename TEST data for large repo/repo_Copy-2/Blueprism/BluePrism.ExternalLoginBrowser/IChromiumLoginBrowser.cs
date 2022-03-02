using System;
using CefSharp;
using CefSharp.WinForms;

namespace BluePrism.ExternalLoginBrowser
{
    public interface IChromiumLoginBrowser : IDisposable
    {
        ChromiumWebBrowser WebBrowser { get; }

        void Focus();

        void Load(string url);
        
        bool UseWaitCursor { get; set; }               
                
        event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;

        event EventHandler<IsBrowserInitializedChangedEventArgs> IsBrowserInitializedChanged;

        event EventHandler<LoadErrorEventArgs> LoadError;

        event LoginCompletedHandler LoginCompleted;

        event LoginFailedHandler LoginFailed;

        event EventHandler<TitleChangedEventArgs> TitleChanged;

    }
}
