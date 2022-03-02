using CefSharp;

namespace BluePrism.ExternalLoginBrowser
{
    public interface IExternalLoginRequestHandler : IRequestHandler
    {
        event LoginCompletedHandler LoginCompleted;
        event LoginFailedHandler LoginFailed;
    }
}
