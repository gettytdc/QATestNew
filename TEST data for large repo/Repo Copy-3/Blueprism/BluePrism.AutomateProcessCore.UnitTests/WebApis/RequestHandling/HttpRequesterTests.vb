#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports NUnit.Framework
Imports Moq
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis
Imports System.Net.Http
Imports System.IO
Imports BluePrism.UnitTesting.TestSupport

Namespace WebApis.RequestHandling
    Public Class HttpRequesterTests

        Private mAuthenticationMock As Mock(Of IAuthenticationHandler)
        Private mContext As ActionContext
        Private mHttpWebResponseMock As Mock(Of HttpWebResponse)
        Private mHttpRequestMock As Mock(Of HttpWebRequest)
        Private mRequester As HttpRequester
        Private mAuthenticationHandlerMock As Mock(Of IAuthenticationHandler)
        Private mBodyContentHandlerMock As Mock(Of IBodyContentGenerator)
        Private mWebException As WebException
        Private mWebExceptionResponseMock As Mock(Of HttpWebResponse)
        Private Const TestContent = "Test Content"
        Private ReadOnly mSession As New clsSession(
            New Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722"), 20,
            New WebConnectionSettings(5, 5, 10,
                                      {New UriWebConnectionSettings("https://www.madeUp.com", 10, 10, 10)}))

        <SetUp>
        Public Sub SetUp()

            mHttpWebResponseMock = New Mock(Of HttpWebResponse)
            mHttpRequestMock = New Mock(Of HttpWebRequest)

            mHttpRequestMock.Setup(Function(m) m.Method).Returns("POST")
            mHttpRequestMock.Setup(Function(m) m.RequestUri).Returns(New Uri("http://www.google.com"))
            mHttpRequestMock.Setup(Function(m) m.Headers).Returns(New WebHeaderCollection())

            SetServicePoint()

            Dim mRequestBuilderMock = New Mock(Of IHttpRequestBuilder)
            mRequestBuilderMock.
                Setup(Function(x) x.Build(It.IsAny(Of ActionContext),
                                          It.IsAny(Of IAuthenticationHandler),
                                          It.IsAny(Of IBodyContentGenerator))).
                Returns(New HttpRequestData(mHttpRequestMock.Object, TestContent))

            mAuthenticationHandlerMock = New Mock(Of IAuthenticationHandler)
            mAuthenticationHandlerMock.
                Setup(Function(x) x.CanHandle(It.IsAny(Of IAuthentication))).
                Returns(True)

            mBodyContentHandlerMock = New Mock(Of IBodyContentGenerator)
            mBodyContentHandlerMock.
                Setup(Function(x) x.CanHandle(It.IsAny(Of IBodyContent))).
                Returns(True)

            mRequester = New HttpRequester({mAuthenticationHandlerMock.Object},
                                           {mBodyContentHandlerMock.Object},
                                            mRequestBuilderMock.Object)

            Dim configuration = New WebApiConfigurationBuilder().
                                        WithCommonAuthentication(New EmptyAuthentication()).
                                        WithAction("Action1", HttpMethod.Get, "/api/action1",
                                                   enableRequestOutputParameter:=True).
                                        Build()

            mContext = New ActionContext(Guid.NewGuid, configuration,
                                         "Action1", New Dictionary(Of String, clsProcessValue), mSession)


            mWebExceptionResponseMock = New Mock(Of HttpWebResponse)
            mWebExceptionResponseMock.
                SetupGet(Function(x) x.StatusCode).
                Returns(HttpStatusCode.Unauthorized)

            mWebExceptionResponseMock.
                Setup(Function(x) x.GetResponseStream()).
                Returns(New MemoryStream())

            mWebException =
                New WebException("Some message", New Exception(),
                                 WebExceptionStatus.UnknownError,
                                 mWebExceptionResponseMock.Object)


        End Sub

        <Test>
        Public Sub GetResponse_RequestThrowsUnauthorizedExceptionAndHandlerSetToRetryOnce_ShouldRetry()

            mHttpRequestMock.
                SetupSequence(Function(x) x.GetResponse()).
                Throws(mWebException).
                Returns(mHttpWebResponseMock.Object)

            mAuthenticationHandlerMock.
                SetupGet(Function(x) x.RetryAttemptsOnUnauthorizedException).
                Returns(1)

            mRequester.GetResponse(mContext)
            mHttpRequestMock.Verify(Function(x) x.GetResponse(), Times.Exactly(2))

        End Sub

        <Test>
        Public Sub GetResponse_RequestThrowsUnauthorizedExceptionTwiceAndHandlerSetToRetryOnce_ShouldThrowException()

            mHttpRequestMock.
                SetupSequence(Function(x) x.GetResponse()).
                Throws(mWebException).
                Throws(mWebException)

            mAuthenticationHandlerMock.
                SetupGet(Function(x) x.RetryAttemptsOnUnauthorizedException).
                Returns(1)

            Assert.Throws(Of WebRequestException)(Function() mRequester.GetResponse(mContext))

        End Sub

        <Test>
        Public Sub GetResponse_RequestThrowsUnauthorizedExceptionAndHandlerSetToRetryOnce_ShouldCallBeforeRetry()

            mHttpRequestMock.
                    SetupSequence(Function(x) x.GetResponse()).
                    Throws(mWebException).
                    Returns(mHttpWebResponseMock.Object)

            mAuthenticationHandlerMock.
                SetupGet(Function(x) x.RetryAttemptsOnUnauthorizedException).
                Returns(1)


            mRequester.GetResponse(mContext)
            mAuthenticationHandlerMock.Verify(Sub(x) x.BeforeRetry(mContext), Times.Exactly(1))

        End Sub

        <Test>
        Public Sub GetResponse_RequestThrowsUnauthorizedExceptionTwiceAndHandlerSetToRetryTwice_ShouldCallBeforeRetryTwice()

            mHttpRequestMock.
                    SetupSequence(Function(x) x.GetResponse()).
                    Throws(mWebException).
                    Throws(mWebException).
                    Returns(mHttpWebResponseMock.Object)

            mAuthenticationHandlerMock.
                SetupGet(Function(x) x.RetryAttemptsOnUnauthorizedException).
                Returns(2)


            mRequester.GetResponse(mContext)
            mAuthenticationHandlerMock.Verify(Sub(x) x.BeforeRetry(mContext), Times.Exactly(2))

        End Sub

        <Test>
        Public Sub GetResponse_RequestThrowsUnauthorizedExceptionAndHandlerNotSetToRetry_ShouldThrowException()

            mHttpRequestMock.
                SetupSequence(Function(x) x.GetResponse()).
                Throws(mWebException).
                Returns(mHttpWebResponseMock.Object)

            mAuthenticationHandlerMock.
                SetupGet(Function(x) x.RetryAttemptsOnUnauthorizedException).
                Returns(0)

            Assert.Throws(Of WebRequestException)(Function() mRequester.GetResponse(mContext))

        End Sub

        <Test>
        Public Sub GetResponse_RequestThrowsOtherWebExceptionAndHandlerSetToRetry_ShouldThrowException()

            mWebExceptionResponseMock.
                SetupGet(Function(x) x.StatusCode).
                Returns(HttpStatusCode.ServiceUnavailable)

            mHttpRequestMock.
                SetupSequence(Function(x) x.GetResponse()).
                Throws(mWebException).
                Returns(mHttpWebResponseMock.Object)

            mAuthenticationHandlerMock.
                SetupGet(Function(x) x.RetryAttemptsOnUnauthorizedException).
                Returns(1)

            Assert.Throws(Of WebRequestException)(Function() mRequester.GetResponse(mContext))

        End Sub

        <Test>
        Public Sub GetResponse_RequestThrowsNonWebExceptionAndHandlerSetToRetry_ShouldThrowException()

            mHttpRequestMock.
                SetupSequence(Function(x) x.GetResponse()).
                Throws(Of NotImplementedException)().
                Returns(mHttpWebResponseMock.Object)

            mAuthenticationHandlerMock.
                SetupGet(Function(x) x.RetryAttemptsOnUnauthorizedException).
                Returns(1)

            Assert.Throws(Of NotImplementedException)(Function() mRequester.GetResponse(mContext))

        End Sub

        Private Sub SetServicePoint()
            Dim servicePoint = ServicePointManager.FindServicePoint(mHttpRequestMock.Object.RequestUri)
            ReflectionHelper.SetPrivateField(GetType(HttpWebRequest), "_ServicePoint", mHttpRequestMock.Object, servicePoint)
        End Sub

        <Test>
        Public Sub GetResponse_DoNotSendRequest_HasCorrectValue()
            Dim configuration = New WebApiConfigurationBuilder().
                                        WithBaseUrl("http://www.google.com").
                                        WithCommonAuthentication(New EmptyAuthentication()).
                                        WithAction("Action1", HttpMethod.Get, "/api/action1",
                                                   disableSendingOfRequest:=True).
                                        Build()

            Assert.IsNull(mRequester.GetResponse(mContext).Response)
        End Sub
    End Class

End Namespace
#End If
