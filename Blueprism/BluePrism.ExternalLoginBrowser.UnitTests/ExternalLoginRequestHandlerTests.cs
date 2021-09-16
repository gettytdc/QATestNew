using System.Net;
using System.Text;
using System.Collections.Specialized;
using CefSharp;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using BluePrism.AutomateAppCore;

namespace BluePrism.ExternalLoginBrowser.UnitTests
{
    [TestFixture]
    public class ExternalLoginRequestHandlerTests
    {
        private IExternalLoginRequestHandler _requestHandler;
        private Mock<IWebBrowser> _webBrowserMock;
        private Mock<IBrowser> _browserMock;
        private Mock<IFrame> _frameMock;
        private Mock<IRequest> _requestMock;
        private Mock<IRequestCallback> _callbackMock;
        private const string EndUrl = "http://localhost/automate";
        private NameValueCollection _headers;

        [SetUp]
        public void SetUp()
        {
            _requestHandler = new ExternalLoginRequestHandler(EndUrl);
            _webBrowserMock = new Mock<IWebBrowser>();
            _browserMock = new Mock<IBrowser>();
            _frameMock = new Mock<IFrame>();
            _callbackMock = new Mock<IRequestCallback>();
            _requestMock = new Mock<IRequest>();

            var locale = Options.Instance;
            locale.Init(ConfigLocator.Instance());
            locale.CurrentLocale = "en-US";
            _headers = new NameValueCollection();
            _requestMock.Setup(x => x.Headers).Returns(_headers);
        }

        [Test]
        public void OnBeforeResourceLoad_RequestStartsWithEndUrl_ShouldRaiseLoginCompletedEventWithResponseBody()
        {
            bool loginEventHandlerCalled = false;
            LoginCompletedEventArgs loginEventArgs = null;
            var mockPostData = new Mock<IPostDataElement>();

            mockPostData.Setup(x => x.Type).Returns(PostDataElementType.Bytes);
            mockPostData.Setup(x => x.Bytes).Returns(Encoding.UTF8.GetBytes("Some body"));
            _requestMock.Setup(x => x.Url).Returns($"{EndUrl}?param=1)");
            _requestMock.Setup(x => x.PostData.Elements).Returns(new[] { mockPostData.Object });

            _requestHandler.LoginCompleted += 
                (_, e) => {
                            loginEventArgs = e as LoginCompletedEventArgs;
                            loginEventHandlerCalled = true;
                          };

            _requestHandler.OnBeforeResourceLoad(_webBrowserMock.Object, _browserMock.Object, _frameMock.Object, _requestMock.Object, _callbackMock.Object);

            _headers.Count.Should().Be(1);
            _headers["Accept-Language"].Should().Be("en-US");

            loginEventHandlerCalled.Should().BeTrue();
            loginEventArgs.Should().NotBeNull();
            loginEventArgs.ResponseBody.Should().Be("Some body");
        }

        [Test]
        public void OnBeforeResourceLoad_RequestStartsWithEndUrl_ShouldContinue()
        {
            var mockPostData = new Mock<IPostDataElement>();

            mockPostData.Setup(x => x.Type).Returns(PostDataElementType.Bytes);
            mockPostData.Setup(x => x.Bytes).Returns(Encoding.UTF8.GetBytes("Some body"));
            _requestMock.Setup(x => x.Url).Returns($"{EndUrl}?param=1)");
            _requestMock.Setup(x => x.PostData.Elements).Returns(new[] { mockPostData.Object });

            var result = _requestHandler.OnBeforeResourceLoad(_webBrowserMock.Object, _browserMock.Object, _frameMock.Object, _requestMock.Object, _callbackMock.Object);

            _headers.Count.Should().Be(1);
            _headers["Accept-Language"].Should().Be("en-US");

            result.Should().Be(CefReturnValue.Continue);
        }

        [Test]
        public void OnBeforeResourceLoad_RequestStartsWithEndUrlButBodyContainsNoBytes_ShouldRaiseLoginCompletedEventWithEmptyResponseBody()
        {
            bool loginEventHandlerCalled = false;
            LoginCompletedEventArgs loginEventArgs = null;
            var mockPostData = new Mock<IPostDataElement>();

            mockPostData.Setup(x => x.Type).Returns(PostDataElementType.File);
            mockPostData.Setup(x => x.Bytes).Returns(Encoding.UTF8.GetBytes("Some body"));
            _requestMock.Setup(x => x.Url).Returns($"{EndUrl}?param=1)");
            _requestMock.Setup(x => x.PostData.Elements).Returns(new[] { mockPostData.Object });

            _requestHandler.LoginCompleted +=
                (_, e) => {
                    loginEventArgs = e as LoginCompletedEventArgs;
                    loginEventHandlerCalled = true;
                };

            _requestHandler.OnBeforeResourceLoad(_webBrowserMock.Object, _browserMock.Object, _frameMock.Object, _requestMock.Object, _callbackMock.Object);

            _headers.Count.Should().Be(1);
            _headers["Accept-Language"].Should().Be("en-US");

            loginEventHandlerCalled.Should().BeTrue();
            loginEventArgs.Should().NotBeNull();
            string.IsNullOrEmpty(loginEventArgs.ResponseBody).Should().BeTrue();
        }

        [Test]
        public void OnBeforeResourceLoad_RequestDoesNotStartWithEndUrl_ShouldNotRaiseLoginCompletedEvent()
        {
            bool loginEventHandlerCalled = false;

            _requestMock.Setup(x => x.Url).Returns($"http://someotherurl");

            _requestHandler.LoginCompleted +=
                (_, e) => {
                    loginEventHandlerCalled = true;
                };

            _requestHandler.OnBeforeResourceLoad(_webBrowserMock.Object, _browserMock.Object, _frameMock.Object, _requestMock.Object, _callbackMock.Object);

            _headers.Count.Should().Be(1);
            _headers["Accept-Language"].Should().Be("en-US");

            loginEventHandlerCalled.Should().BeFalse();
        }

       [Test]
        public void OnBeforeResourceLoad_RequestDoesNotStartWithEndUrl_ShouldContinue()
        {
           _requestMock.Setup(x => x.Url).Returns($"http://someotherurl");
            var result = _requestHandler.OnBeforeResourceLoad(_webBrowserMock.Object, _browserMock.Object, _frameMock.Object, _requestMock.Object, _callbackMock.Object);
            result.Should().Be(CefReturnValue.Continue);
            _headers.Count.Should().Be(1);
            _headers["Accept-Language"].Should().Be("en-US");
        }
        [Test]
        public void OnResourceResponse_RequestReturnsUnauthorized_ShouldRaiseLoginFailedEventOnceWithUnauthorizedStatusCodeResponse()
        {
            bool loginFailedHandlerCalled = false;
            LoginFailedEventArgs loginFailedEventArgs = null;
            int countLoginFailedInvoke = 0;

            var responseMock = new Mock<IResponse>();
            responseMock.Setup(x => x.StatusCode).Returns((int)HttpStatusCode.Unauthorized);

            _requestHandler.LoginFailed +=
                (_, e) =>
                {
                    loginFailedHandlerCalled = true;
                    loginFailedEventArgs = e as LoginFailedEventArgs;
                    countLoginFailedInvoke += 1;
                };

            _requestHandler.OnResourceResponse(_webBrowserMock.Object, _browserMock.Object, _frameMock.Object, _requestMock.Object, responseMock.Object);
            loginFailedHandlerCalled.Should().BeTrue();
            loginFailedEventArgs.Should().NotBeNull();
            loginFailedEventArgs.HttpStatusCode.Should().Be(HttpStatusCode.Unauthorized);
            countLoginFailedInvoke = 1;
        }

        [Test]
        public void OnResourceResponse_RequestReturnsTimeout_ShouldRaiseLoginFailedEventOnceWithGatewayTimeoutStatusCodeResponse()
        {
            bool loginFailedHandlerCalled = false;
            LoginFailedEventArgs loginFailedEventArgs = null;
            int countLoginFailedInvoke = 0;

            var responseMock = new Mock<IResponse>();
            responseMock.Setup(x => x.StatusCode).Returns((int)HttpStatusCode.GatewayTimeout);

            _requestHandler.LoginFailed +=
                (_, e) =>
                {
                    loginFailedHandlerCalled = true;
                    loginFailedEventArgs = e as LoginFailedEventArgs;
                    countLoginFailedInvoke += 1;
                };

            _requestHandler.OnResourceResponse(_webBrowserMock.Object, _browserMock.Object, _frameMock.Object, _requestMock.Object, responseMock.Object);
            loginFailedHandlerCalled.Should().BeTrue();
            loginFailedEventArgs.Should().NotBeNull();
            loginFailedEventArgs.HttpStatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
            countLoginFailedInvoke = 1;
            
        }

        [Test]
        public void OnResourceResponse_RequestReturnsOK_ShouldNotRaiseLoginFailedEvent()
        {
            bool loginFailedHandlerCalled = false;
            LoginFailedEventArgs loginFailedEventArgs = null;
            int countLoginFailedInvoke = 0;

            var responseMock = new Mock<IResponse>();
            responseMock.Setup(x => x.StatusCode).Returns((int)HttpStatusCode.OK);

            _requestHandler.LoginFailed +=
                (_, e) =>
                {
                    loginFailedHandlerCalled = true;
                    loginFailedEventArgs = e as LoginFailedEventArgs;
                    countLoginFailedInvoke += 1;
                };

            _requestHandler.OnResourceResponse(_webBrowserMock.Object, _browserMock.Object, _frameMock.Object, _requestMock.Object, responseMock.Object);
            loginFailedHandlerCalled.Should().BeFalse();
            loginFailedEventArgs.Should().BeNull();
            countLoginFailedInvoke = 0;
        }

	}
}
