#If UNITTESTS Then

Imports System.Collections.Specialized
Imports System.Linq
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports FluentAssertions
Imports NUnit.Framework
Imports Moq
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace WebApis.RequestHandling

    <TestFixture, Category("Web APIs")>
    Public Class HttpRequestBuilderTests

        ''' <summary>
        ''' The web api id passed through the tests
        ''' </summary>
        Private ReadOnly mWebApiId As New Guid("ACD2D25C-5709-48EB-804A-E1EBA831C672")

        Private ReadOnly mSessionId As New Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722")

        Private mWebConnectionSettings As New WebConnectionSettings(5, 10, 5, New List(Of UriWebConnectionSettings)())

        ''' <summary>
        ''' Creates an instance of HttpRequestBuilder and uses it to build an 
        ''' HttpWebRequest for the specified Web API action
        ''' </summary>
        ''' <param name="configuration">The Web API configuration used</param>
        ''' <param name="actionName">The name of the action</param>
        ''' <param name="authenticationHandler">The authentication handler used to
        ''' initialise the HttpRequestBuilder</param>
        ''' <param name="parameters">The parameters that are set within the 
        ''' ActionContext object when handling the request</param>
        ''' <returns>The HttpWebRequest that was created by HttpRequestBuilder</returns>
        Private Function BuildRequest(configuration As WebApiConfiguration,
                                             actionName As String,
                                             session As clsSession,
                                             Optional authenticationHandler As IAuthenticationHandler = Nothing,
                                             Optional bodyContentHandler As IBodyContentGenerator = Nothing,
                                             Optional parameters As Dictionary(Of String, clsProcessValue) = Nothing) _
            As HttpRequestData

            parameters = If(parameters, New Dictionary(Of String, clsProcessValue))

            Dim context As New ActionContext(mWebApiId, configuration, actionName, parameters, session)

            If authenticationHandler Is Nothing Then
                authenticationHandler = New EmptyAuthenticationHandler()
            End If

            If bodyContentHandler Is Nothing Then
                bodyContentHandler = New TemplateContentGenerator()
            End If

            Dim builder = New TestHttpRequestBuilder()
            Dim request = builder.Build(context, authenticationHandler, bodyContentHandler)
            Return request

        End Function

        <Test>
        Public Sub Build_WithLocalPath_ShouldCombineBaseUrlAndPath()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                WithBaseUrl("https://www.myapi.org/").
                WithAction("Action1", HttpMethod.Get, "/action1").
                Build
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            request.RequestUri.AbsoluteUri.Should.Be("https://www.myapi.org/action1")

        End Sub

        <Test>
        Public Sub Build_WithHttpMethod_ShouldSpecifyMethod()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithBaseUrl("https://www.myapi.org/").
                    WithAction("Action1", HttpMethod.Post, "/action1").
                    Build
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            request.Method.Should.Be(HttpMethod.Post.Method)

        End Sub

        <Test>
        Public Sub Build_WithCommonHeaders_ShouldIncludeHeadersInRequest()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                WithHeader("Header1", "Value 1").
                WithHeader("Header2", "Value 2").
                WithAction("Action1", HttpMethod.Get, "/action1").
                Build
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            Dim expectedHeaders As New NameValueCollection From
            {
                {"Header1", "Value 1"},
                {"Header2", "Value 2"},
                {"Content-Type", "text/plain; charset=utf-8"}
            }
            request.Headers.Should.BeEquivalentTo(expectedHeaders)

        End Sub

        <Test>
        Public Sub Build_WithCommonAndActionHeaders_ShouldIncludeAllInRequest()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                WithHeader("Header1", "Value 1").
                WithAction("Action1", HttpMethod.Get, "/action1",
                            headers:={New HttpHeader("Header2", "Value 2")}).
                Build
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            Dim expectedHeaders As New NameValueCollection From {
                {"Header1", "Value 1"},
                {"Header2", "Value 2"},
                {"Content-Type", "text/plain; charset=utf-8"}
            }
            request.Headers.Should.BeEquivalentTo(expectedHeaders)

        End Sub

        <Test>
        Public Sub Build_InvalidUri_ThrowsFormatException()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithBaseUrl("https://INVALID*&^${}[]").
                    WithAction("Action1", HttpMethod.Get, "/customers").
                    Build

            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Assert.Throws(Of UriFormatException)(
                Sub() BuildRequest(apiConfiguration, "Action1", session))

        End Sub

        <Test>
        Public Sub Build_WithTokensInBaseUri_ShouldInsertParameters()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithBaseUrl("https://[auth_server]-[api_name]").
                    WithAction("Action1", HttpMethod.Get, "/customers").
                    Build

            Dim parameters = New Dictionary(Of String, clsProcessValue) From
                    {{"auth_server", New clsProcessValue("blueprism-login.com")},
                    {"api_name", New clsProcessValue("testAPI")}}
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session, parameters:=parameters)

            request.Request.RequestUri.Should.Be("https://blueprism-login.com-testAPI/customers")

        End Sub

        <Test>
        Public Sub Build_WithTokensInBaseUri_InvalidCharactersInParameter_Throws()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithBaseUrl("https://[api_name]").
                    WithAction("Action1", HttpMethod.Get, "/customers").
                    Build

            Dim parameters = New Dictionary(Of String, clsProcessValue) From
                    {{"api_name", New clsProcessValue("INVALID*&^${}[]")}}

            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)

            Assert.Throws(Of UriFormatException)(
                Sub() BuildRequest(apiConfiguration, "Action1", session, parameters:=parameters))

        End Sub

        <Test>
        Public Sub Build_WithTokensInActionUrlPath_ShouldInsertParameter()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithAction("Action1", HttpMethod.Get, "/customers/[customer-id]").
                    Build

            Dim parameters = New Dictionary(Of String, clsProcessValue) From
                {{"customer-id", New clsProcessValue("abcd-1234")}}
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session, parameters:=parameters)

            request.Request.RequestUri.AbsoluteUri.Should.EndWith("/customers/abcd-1234")

        End Sub

        <Test>
        Public Sub Build_WithTokensInHeaders_ShouldInsertParameters()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithHeader("Content-Type", "[content-type-value-1]").
                    WithHeader("Header1", "[header-value-1]").
                    WithHeader("Header2", "[header-value-2]").
                    WithAction("Action1", HttpMethod.Get, "/action1").
                    Build

            Dim parameters = New Dictionary(Of String, clsProcessValue) From {
                {"content-type-value-1", New clsProcessValue("application/json")},
                {"header-value-1", New clsProcessValue("Value 1")},
                {"header-value-2", New clsProcessValue("Value 2")}
            }
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session, parameters:=parameters).Request

            Dim expectedHeaders As New Dictionary(Of String, String) From {
                {"Content-Type", "application/json"},
                {"Header1", "Value 1"},
                {"Header2", "Value 2"}
            }

            Dim headers = request.Headers.AllKeys.ToDictionary(Function(x) x, Function(x) request.Headers(x))

            headers.ShouldBeEquivalentTo(expectedHeaders)

        End Sub

        <Test>
        Public Sub Build_WithTokensInBody_ShouldInsertParameters()

            Dim templateContent = New TemplateBodyContent("This is the body with parameters [body-value-1] and [body-value-2]")
            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithAction("Action1", HttpMethod.Get, "/action1",
                               bodyContent:=templateContent).
                    Build
            Dim parameters = New Dictionary(Of String, clsProcessValue)
            parameters.Add("body-value-1", New clsProcessValue("Value 1"))
            parameters.Add("body-value-2", New clsProcessValue("Value 2"))
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session, parameters:=parameters)
            Dim requestStream = request.Request.GetRequestStream
            requestStream.Seek(0, SeekOrigin.Begin)
            Dim reader As New StreamReader(requestStream)
            Dim content = reader.ReadToEnd
            content.Should.Be("This is the body with parameters Value 1 and Value 2")

        End Sub

        <Test>
        Public Sub Build_WithContentTypeHeader_ShouldIncludeHeader()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithHeader("Content-Type", "application/json").
                    WithAction("Action1", HttpMethod.Post, "/action1",
                                bodyContent:=New TemplateBodyContent("Some body text")).
                    Build
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            request.Headers(HttpRequestHeader.ContentType).Should.Be("application/json")

        End Sub

        <Test>
        Public Sub Build_WithCredentialTokensInActionUrlPath_ShouldInsertParameter()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithCommonAuthentication(New CustomAuthentication(New AuthenticationCredential("Bob", False, ""))).
                    WithAction("Action1", HttpMethod.Get, "/customers/[Credential.AdditionalProperties.Secret]").
                    Build

            Dim handler = New CustomAuthenticationHandler(CreateCredentialHelper(TestCredential.BobWithSecret))
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session, authenticationHandler:=handler).Request

            request.RequestUri.AbsoluteUri.Should.EndWith("/customers/theSecret")

        End Sub

        <Test>
        Public Sub Build_WithCredentialTokensInHeaders_ShouldInsertParameters()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithCommonAuthentication(New CustomAuthentication(New AuthenticationCredential("Bob", False, ""))).
                    WithHeader("Header1", "[Credential.AdditionalProperties.Secret]").
                    WithAction("Action1", HttpMethod.Get, "/action1").
                    Build

            Dim handler = New CustomAuthenticationHandler(CreateCredentialHelper(TestCredential.BobWithSecret))
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session, authenticationHandler:=handler).Request

            Dim expectedHeaders As New NameValueCollection From {
                {"Header1", "theSecret"},
                {"Content-Type", "text/plain; charset=utf-8"}
            }
            request.Headers.Should.BeEquivalentTo(expectedHeaders)

        End Sub

        <Test>
        Public Sub Build_WithCredentialTokensInBody_ShouldInsertParameters()
            Dim templateContent = New TemplateBodyContent("This is the body with parameter [Credential.AdditionalProperties.Secret]")
            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithCommonAuthentication(New CustomAuthentication(New AuthenticationCredential("Bob", False, ""))).
                    WithAction("Action1", HttpMethod.Get, "/action1",
                               bodyContent:=templateContent).
                    Build

            Dim handler = New CustomAuthenticationHandler(CreateCredentialHelper(TestCredential.BobWithSecret))
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session, authenticationHandler:=handler).Request

            Dim requestStream = request.GetRequestStream
            requestStream.Seek(0, SeekOrigin.Begin)
            Dim reader As New StreamReader(requestStream)
            Dim content = reader.ReadToEnd
            content.Should.Be("This is the body with parameter theSecret")

        End Sub

        <Test>
        Public Sub Build_WithContent_ShouldIncludeDefaultContentTypeHeader()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithAction("Action1", HttpMethod.Post, "/action1",
                               bodyContent:=New TemplateBodyContent("Some body text")).
                    Build
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            request.Headers(HttpRequestHeader.ContentType).Should.Be("text/plain; charset=utf-8")

        End Sub

        <Test>
        Public Sub Build_WhenCapturingRequestContent_ShouldIncludeRequestContent()

            Dim testContent = New TemplateBodyContent("Test content")
            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithHeader("Header1", "Value 1").
                    WithHeader("Header2", "Value 2").
                    WithAction("Action1", HttpMethod.Post, "/action1",
                               bodyContent:=testContent,
                               enableRequestOutputParameter:=True).
                    Build
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim result = BuildRequest(apiConfiguration, "Action1", session)

            Dim requestContent = result.Content
            requestContent.Should.Be(testContent.Template)

        End Sub

        <Test>
        Public Sub Build_WithTextContent_ShouldSetContentLength()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithAction("Action1", HttpMethod.Post, "/action1",
                        bodyContent:=New TemplateBodyContent("12345")).
                    Build
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            request.ContentLength.Should.Be(5)

        End Sub


        <Test>
        Public Sub Build_ShouldApplyTimeoutSetting()

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                WithBaseUrl("https://www.myapi.org/").
                WithAction("Action1", HttpMethod.Get, "/action1").
                WithConfigurationSettings(New WebApiConfigurationSettings(12, 13)).
                Build
            Dim session = New clsSession(mSessionId, 1, mWebConnectionSettings)
            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request
            request.Timeout.Should().Be(12000)

        End Sub


        <Test>
        Public Sub Test_BuildRequest_NoUriSpecificInCache_SetsCachedDefaultSettings()

            Dim webConnectionSettings = New WebConnectionSettings(
                2, 5, 2, {New UriWebConnectionSettings("https://www.somethingelse.com", 7, 8, 9)})
            Dim session = New clsSession(mSessionId, 1, webConnectionSettings)

            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithBaseUrl("https://www.myapi.org/").
                    WithAction("Action1", HttpMethod.Get, "/action1").
                    WithConfigurationSettings(New WebApiConfigurationSettings(12, 13)).
                    Build

            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request
            Dim sp = ServicePointManager.FindServicePoint(request.RequestUri)

            request.Timeout.Should().Be(12000)

            sp.ConnectionLimit.Should().Be(2)
            sp.MaxIdleTime.Should().Be(5 * 1000)
            sp.ConnectionLeaseTimeout.Should().Be(2 * 1000)

        End Sub

        <Test>
        Public Sub Test_BuildRequest_SetsCachedUriSpecificSetting()

            Dim webConnectionSettings = New WebConnectionSettings(
                2, 5, 2, {New UriWebConnectionSettings("https://www.myapi.org", 7, 8, 9)})
            Dim session = New clsSession(mSessionId, 1, webConnectionSettings)
            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithBaseUrl("https://www.myapi.org/").
                    WithAction("Action1", HttpMethod.Get, "/action1").
                    WithConfigurationSettings(New WebApiConfigurationSettings(12, 13)).
                    Build

            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            Dim sp = ServicePointManager.FindServicePoint(request.RequestUri)

            request.Timeout.Should().Be(12000)

            sp.ConnectionLimit.Should().Be(7)
            sp.ConnectionLeaseTimeout.Should().Be(8 * 1000)
            sp.MaxIdleTime.Should().Be(9 * 1000)

        End Sub

        <Test>
        Public Sub Test_BuildRequest_IncludingResource_SetsCachedUriSpecificSetting()

            Dim webConnectionSettings = New WebConnectionSettings(
                2, 5, 2, {New UriWebConnectionSettings("https://www.myapi.org", 7, 8, 9)})
            Dim session = New clsSession(mSessionId, 1, webConnectionSettings)
            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithBaseUrl("https://www.myapi.org/action1").
                    WithAction("Action1", HttpMethod.Get, "/action1").
                    WithConfigurationSettings(New WebApiConfigurationSettings(12, 13)).
                    Build

            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            Dim sp = ServicePointManager.FindServicePoint(request.RequestUri)

            request.Timeout.Should().Be(12000)

            sp.ConnectionLimit.Should().Be(7)
            sp.ConnectionLeaseTimeout.Should().Be(8 * 1000)
            sp.MaxIdleTime.Should().Be(9 * 1000)

        End Sub


        <Test>
        Public Sub Test_BuildRequest_EmptyConnectionLease_AppliesDefaultValue()

            Dim webConnectionSettings = New WebConnectionSettings(
                2, 9, Nothing, {New UriWebConnectionSettings("https://www.myapi.org", 7, 8, 9)})
            Dim session = New clsSession(mSessionId, 1, webConnectionSettings)
            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithBaseUrl("https://www.otherapi.org/").
                    WithAction("Action1", HttpMethod.Get, "/action1").
                    WithConfigurationSettings(New WebApiConfigurationSettings(12, 13)).
                    Build

            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            Dim sp = ServicePointManager.FindServicePoint(request.RequestUri)

            request.Timeout.Should().Be(12000)

            sp.ConnectionLimit.Should().Be(2)
            sp.ConnectionLeaseTimeout.Should().Be(-1)
            sp.MaxIdleTime.Should().Be(9 * 1000)

        End Sub

        <Test>
        Public Sub Test_BuildRequest_EmptyConnectionLeaseInCachedUriSpecificSetting_AppliesDefaultVlaue()

            Dim webConnectionSettings = New WebConnectionSettings(
                2, 5, 2, {New UriWebConnectionSettings("https://www.myapi.org", 7, Nothing, 9)})
            Dim session = New clsSession(mSessionId, 1, webConnectionSettings)
            Dim apiConfiguration = New WebApiConfigurationBuilder().
                    WithBaseUrl("https://www.myapi.org/").
                    WithAction("Action1", HttpMethod.Get, "/action1").
                    WithConfigurationSettings(New WebApiConfigurationSettings(12, 13)).
                    Build

            Dim request = BuildRequest(apiConfiguration, "Action1", session).Request

            Dim sp = ServicePointManager.FindServicePoint(request.RequestUri)

            request.Timeout.Should().Be(12000)

            sp.ConnectionLimit.Should().Be(7)
            sp.ConnectionLeaseTimeout.Should().Be(-1)
            sp.MaxIdleTime.Should().Be(9 * 1000)

        End Sub

        '<Test>
        'Public Sub Test_ApplySettingsToServicePoint_NoConnectionLease_AppliesCorrectValues()
        '    Dim requestUri = New Uri("https://www.myapi.org/action1")
        '    Dim sp = ServicePointManager.FindServicePoint(requestUri)
        '    Dim builder = New HttpRequestBuilder

        '    builder.ApplySettingsToServicePoint(5, 10, Nothing, requestUri)

        '    Assert.AreEqual(sp.MaxIdleTime, 5 * 1000)
        '    Assert.AreEqual(sp.ConnectionLimit, 10)
        '    Assert.AreEqual(sp.ConnectionLeaseTimeout, -1)
        'End Sub


        Private Shared Function CreateCredentialHelper(credential As TestCredential) As IAuthenticationCredentialHelper
            Dim credentialHelper = New Mock(Of IAuthenticationCredentialHelper)
            credentialHelper.Setup(Function(h) h.GetCredential(It.IsAny(Of AuthenticationCredential),
                                                               It.IsAny(Of Dictionary(Of String, clsProcessValue)),
                                                               It.IsAny(Of Guid))).
                Returns(credential)
            Return credentialHelper.Object
        End Function

        ''' <summary>
        ''' Test implementation used to intercept request creation so that a mock
        ''' HttpWebRequest object can be used. The main thing we're working around
        ''' here is that writing to the request stream normally involves opening
        ''' a connection (ConnectStream) to the host. This is replaced with an in-memory
        ''' MemoryStream that a) prevents network connection from opening and b) 
        ''' allows us to inspect the content after the request has been built.
        ''' </summary>
        Private Class TestHttpRequestBuilder
            Inherits HttpRequestBuilder

            Protected Overrides Function Create(uri As Uri) As HttpWebRequest

                Dim mock = MockRequestHelper.Create
                mock.Setup(Function(r) r.RequestUri).Returns(uri)
                Return mock.Object

            End Function

        End Class

    End Class

End Namespace

#End If
