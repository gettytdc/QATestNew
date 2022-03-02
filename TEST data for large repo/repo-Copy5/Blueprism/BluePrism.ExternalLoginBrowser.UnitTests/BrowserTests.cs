using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using IdentityModel.OidcClient.Browser;
using Moq;
using NUnit.Framework;

namespace BluePrism.ExternalLoginBrowser.UnitTests
{
    [TestFixture]
    public class BrowserTests
    {
        private Mock<IBrowserFormFactory> _formFactoryMock;
        private Mock<IChromiumLoginBrowserFactory> _browserFactoryMock;
        private Mock<IBrowserForm> _browserFormMock;
        private Mock<IChromiumLoginBrowser> _chromiumWebBrowserMock;
        private Browser _browser;

        [SetUp]
        public void SetUp()
        {
            _browserFormMock = new Mock<IBrowserForm>();
            _chromiumWebBrowserMock = new Mock<IChromiumLoginBrowser>();
            _formFactoryMock = new Mock<IBrowserFormFactory>();
                        
            _formFactoryMock
                .Setup(x => x.Create(It.IsAny<IChromiumLoginBrowser>()))
                .Returns(_browserFormMock.Object);

            _browserFactoryMock = new Mock<IChromiumLoginBrowserFactory>();
            _browserFactoryMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_chromiumWebBrowserMock.Object);


            _browser = new Browser(_formFactoryMock.Object, _browserFactoryMock.Object);
        }

        [Test]
        public async Task Invoke_LoginCompleted_ShouldReturnSuccess()
        {
            var invoke = _browser.InvokeAsync(new BrowserOptions("http://someurl", "http://someotherurl"));
            _browserFormMock.Raise(m => m.LoginCompleted += null, new LoginCompletedEventArgs("some response"));

            var result = await invoke;

            result.ResultType.Should().Be(BrowserResultType.Success);

        }

        [Test]
        public async Task Invoke_LoginCompleted_ShouldReturnResponse()
        {
            var invoke = _browser.InvokeAsync(new BrowserOptions("http://someurl", "http://someotherurl"));
            _browserFormMock.Raise(m => m.LoginCompleted += null, new LoginCompletedEventArgs("some response"));

            var result = await invoke;

            result.Response.Should().Be("some response");
        }

        [Test]
        public async Task Invoke_FormClosed_ShouldReturnUserCancel()
        {
            var invoke = _browser.InvokeAsync(new BrowserOptions("http://someurl", "http://someotherurl"));
            
            _browserFormMock.Raise(m => m.FormClosed += null, new FormClosedEventArgs(CloseReason.None));

            var result = await invoke;

            result.ResultType.Should().Be(BrowserResultType.UserCancel);
        }
        
        [Test]
        public async Task Invoke_LoadError_ShouldReturnUnknownError()
        {
            var invoke = _browser.InvokeAsync(new BrowserOptions("http://someurl", "http://someotherurl"));

            _chromiumWebBrowserMock.Raise(m => m.LoadError += null,
                new CefSharp.LoadErrorEventArgs(null, null, CefSharp.CefErrorCode.AddressInvalid, "error message", "http://someotherurl"));

            var result = await invoke;

            result.ResultType.Should().Be(BrowserResultType.UnknownError);
        }

        [Test]
        public async Task Invoke_LoginFailedWithUnauthorized_ShouldReturnHttpError()
        {
            var invoke = _browser.InvokeAsync(new BrowserOptions("http://someurl", "http://someotherurl"));
            _browserFormMock.Raise(m => m.LoginFailed += null, new LoginFailedEventArgs(System.Net.HttpStatusCode.Unauthorized));

            var result = await invoke;

            result.ResultType.Should().Be(BrowserResultType.HttpError);
        }

        [Test]
        public async Task Invoke_LoginFailedWithGatewayTimeout_ShouldReturnTimeout()
        {
            var invoke = _browser.InvokeAsync(new BrowserOptions("http://someurl", "http://someotherurl"));
            _browserFormMock.Raise(m => m.LoginFailed += null, new LoginFailedEventArgs(System.Net.HttpStatusCode.GatewayTimeout));

            var result = await invoke;

            result.ResultType.Should().Be(BrowserResultType.Timeout);
        }

        [Test]
        public async Task Invoke_LoginFailedWithAllOtherStatusCodes_ShouldReturnUnknownError()
        {
            var invoke = _browser.InvokeAsync(new BrowserOptions("http://someurl", "http://someotherurl"));
            _browserFormMock.Raise(m => m.LoginFailed += null, new LoginFailedEventArgs(
                It.Is< System.Net.HttpStatusCode>(
                    c => c != System.Net.HttpStatusCode.GatewayTimeout && c != System.Net.HttpStatusCode.Unauthorized)));

            var result = await invoke;

            result.ResultType.Should().Be(BrowserResultType.UnknownError);
        }


        [Test]
        public async Task Invoke_LoginFailed_ShouldReturnResponse()
        {
            var invoke = _browser.InvokeAsync(new BrowserOptions("http://someurl", "http://someotherurl"));
            _browserFormMock.Raise(m => m.LoginFailed += null, new LoginFailedEventArgs(System.Net.HttpStatusCode.Unauthorized));

            var result = await invoke;

            result.Response.Should().Be(System.Net.HttpStatusCode.Unauthorized.ToString());
        }
    }
}
