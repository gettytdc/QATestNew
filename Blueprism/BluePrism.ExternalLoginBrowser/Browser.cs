using System.Threading;
using System.Threading.Tasks;
using BluePrism.Core.Utility;
using IdentityModel.OidcClient.Browser;

namespace BluePrism.ExternalLoginBrowser
{
    public class Browser : IBrowser
    {
        private readonly IBrowserFormFactory _formFactory;
        private readonly IChromiumLoginBrowserFactory _loginBrowserFactory;
        
        private BrowserResult _browserResult =
            new BrowserResult() { ResultType = BrowserResultType.UserCancel, Response = null };

        public Browser(IBrowserFormFactory formFactory, IChromiumLoginBrowserFactory loginBrowserFactory)
        {
            _formFactory = formFactory;
            _loginBrowserFactory = loginBrowserFactory;
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var browser = _loginBrowserFactory.Create(options.StartUrl, options.EndUrl))
            using (var browserForm = _formFactory.Create(browser))
            using (var signal = new SafeSemaphoreSlim(0, 1))
            {
                browserForm.HideFormUntilLoaded();

                browserForm.LoginCompleted += (o, e) =>
                {
                    _browserResult = new BrowserResult() { ResultType = BrowserResultType.Success, Response = e.ResponseBody };
                    signal.TryRelease();
                };

                browserForm.LoginFailed += (o, e) =>
                {
                    var resultType = BrowserResultType.UnknownError;
                    if (e.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        resultType = BrowserResultType.HttpError;
                    }
                    else if (e.HttpStatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                    {
                        resultType = BrowserResultType.Timeout;
                    }

                    _browserResult = new BrowserResult() { ResultType = resultType, Response = e.HttpStatusCode.ToString() };
                    signal.TryRelease();
                };

                browserForm.FormClosed += (o, e) =>
                {
                    _browserResult = new BrowserResult() { ResultType = BrowserResultType.UserCancel, Response = string.Empty };
                    signal.TryRelease();
                };

                browser.LoadError += (o, e) =>
                {
                    _browserResult = new BrowserResult() { ResultType = BrowserResultType.UnknownError, Response = string.Empty };
                    signal.TryRelease();
                };

                browserForm.Show();

                await signal.WaitAsync(cancellationToken);

                return _browserResult;
            }
        }
    }
}
