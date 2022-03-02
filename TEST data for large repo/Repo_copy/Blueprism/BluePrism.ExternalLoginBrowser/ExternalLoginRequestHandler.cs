using System.Linq;
using BluePrism.AutomateAppCore;
using System.Net;
using CefSharp;
using CefSharp.Handler;

namespace BluePrism.ExternalLoginBrowser
{
    internal class ExternalLoginRequestHandler : DefaultRequestHandler, IExternalLoginRequestHandler
    {
        private readonly string _endUrl;

        private const int HttpStatusCodeUnauthorized = 401;
        private const int HttpStatusCodeInternalServerError = 500;
        private const int HttpStatusCodeGatewayTimeout = 504;

        public event LoginCompletedHandler LoginCompleted;
        public event LoginFailedHandler LoginFailed;
        
        public ExternalLoginRequestHandler(string endUrl)
        {
            _endUrl = endUrl;
        }

        public override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            var headers = request.Headers;
            var locale = Options.Instance.CurrentLocale;
            headers.Add("Accept-Language", locale);
            request.Headers = headers;

            if (request.Url.StartsWith(_endUrl))
            {
                var responseBody = request
                                        .PostData
                                        .Elements
                                        .FirstOrDefault(e => e.Type == PostDataElementType.Bytes)
                                        ?.GetBody();
               
                LoginCompleted?.Invoke(this, new LoginCompletedEventArgs(responseBody));

                return CefReturnValue.Continue;
            }
            
            return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);
        }

        public override bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCodeUnauthorized:
                    LoginFailed?.Invoke(this, new LoginFailedEventArgs(HttpStatusCode.Unauthorized));
                    break;
                case HttpStatusCodeGatewayTimeout:
                    LoginFailed?.Invoke(this, new LoginFailedEventArgs(HttpStatusCode.GatewayTimeout));
                    break;
                case HttpStatusCodeInternalServerError:
                    LoginFailed?.Invoke(this, new LoginFailedEventArgs(HttpStatusCode.InternalServerError));
                    break;
            }
            return base.OnResourceResponse(browserControl, browser, frame, request, response);
        }
    }
}
