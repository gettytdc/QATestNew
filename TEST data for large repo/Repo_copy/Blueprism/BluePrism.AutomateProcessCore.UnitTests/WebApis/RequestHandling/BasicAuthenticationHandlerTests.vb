#If UNITTESTS

Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports FluentAssertions
Imports Moq
Imports NUnit.Framework
Imports BluePrism.Common.Security

Namespace WebApis.RequestHandling

    Public Class BasicAuthenticationHandlerTests

        ''' <summary>
        ''' Name of parameter used through tests
        ''' </summary>
        Private Const CredentialParameterName = "Basic Auth Credential"
        ''' <summary>
        ''' The web api id passed through the tests
        ''' </summary>
        Private ReadOnly mWebApiId As New Guid("ACD2D25C-5709-48EB-804A-E1EBA831C672")
        ''' <summary>
        ''' The session id passed through the tests
        ''' </summary>
        Private ReadOnly mSessionId As New Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722")


        <Test>
        Public Sub Handle_NonPreemptiveWithSelectedCredential_ShouldSetRequestCredential()

            Dim credential = New AuthenticationCredential(TestCredential.Frank.Name, False, CredentialParameterName)
            Dim authentication As New BasicAuthentication(credential, False)

            Dim request = Handle(TestCredential.Frank, authentication)

            AssertExpectedRequestCredentials(request, TestCredential.Frank)

        End Sub

        <Test>
        Public Sub Handle_NonPreemptiveWithCredentialParameter_ShouldSetRequestCredential()

            Dim credential = New AuthenticationCredential("", True, CredentialParameterName)
            Dim authentication As New BasicAuthentication(credential, False)
            Dim parameters As New Dictionary(Of String, clsProcessValue) From {{CredentialParameterName, New clsProcessValue(TestCredential.Frank.Name)}}

            Dim request = Handle(TestCredential.Frank, authentication, parameters:=parameters)

            AssertExpectedRequestCredentials(request, TestCredential.Frank)

        End Sub

        <Test>
        Public Sub Handle_PreemptiveWithSelectedCredential_ShouldSetHeader()

            Dim credential = New AuthenticationCredential(TestCredential.Frank.Name, False, CredentialParameterName)
            Dim authentication As New BasicAuthentication(credential, True)

            Dim request = Handle(TestCredential.Frank, authentication)

            request.Credentials.Should.BeNull()
            AssertExpectedAuthorizationHeader(request, TestCredential.Frank)

        End Sub

        <Test>
        Public Sub Handle_PreemptiveWithCredentialParameter_ShouldSetHeader()

            Dim credential = New AuthenticationCredential("", True, CredentialParameterName)
            Dim authentication As New BasicAuthentication(credential, True)
            Dim parameters As New Dictionary(Of String, clsProcessValue) From {{CredentialParameterName, New clsProcessValue(TestCredential.Frank.Name)}}

            Dim request = Handle(TestCredential.Frank, authentication, parameters)

            request.Credentials.Should.BeNull()
            AssertExpectedAuthorizationHeader(request, TestCredential.Frank)

        End Sub

        <Test>
        Public Sub Handle_WithInvalidAuthentication_ShouldThrow()

            Dim authentication As New EmptyAuthentication()
            Dim action As Action = Sub() Handle(Nothing, authentication)
            action.ShouldThrow(Of ArgumentException)

        End Sub

        Private Sub AssertExpectedAuthorizationHeader(request As HttpWebRequest, credential As ICredential)
            Dim header = request.Headers(HttpRequestHeader.Authorization)
            header.Should.NotBeNullOrWhiteSpace()
            Dim expectedHeader = BasicAuthenticationHelper.GetAuthorizationHeaderValue(
                credential.Username, credential.Password, Encoding.ASCII)
            header.Should.Be(expectedHeader)
        End Sub

        ''' <summary>
        ''' Shared test method that creates handler object, mocking the 
        ''' ICredentialStore dependency and setting it up to return the 
        ''' specified credential object. Then executes the Handle method
        ''' and returns the HttpWebRequest object.
        ''' </summary>
        ''' <param name="credential">The credential object that the credential store should
        ''' return. If null, then the store will not be set up to return a credential.</param>
        ''' <param name="authentication">The authentication configuration used in the test</param>
        ''' <param name="parameters">The parameters that are set within the ActionContext object
        ''' when handling the request</param>
        ''' <returns>The request object created by HttpRequestBuilder</returns>
        Private Function Handle(credential As TestCredential,
                                authentication As IAuthentication,
                                Optional parameters As Dictionary(Of String, clsProcessValue) = Nothing
                                ) As HttpWebRequest

            Dim credentialHelperMock As New Mock(Of IAuthenticationCredentialHelper)
            If credential IsNot Nothing Then
                credentialHelperMock.Setup(Function(s) s.GetCredential(Moq.It.IsAny(Of AuthenticationCredential),
                                                                       Moq.It.IsAny(Of Dictionary(Of String, clsProcessValue)),
                                                                       Moq.It.IsAny(Of Guid))).
                                                         Returns(credential)
            End If

            Dim handler As New BasicAuthenticationHandler(credentialHelperMock.Object)
            Dim requestMock = MockRequestHelper.Create()
            Dim configuration = New WebApiConfigurationBuilder().
                WithCommonAuthentication(authentication).
                WithAction("Action1", HttpMethod.Get, "/api/action1").
                Build()

            If parameters Is Nothing Then parameters = New Dictionary(Of String, clsProcessValue)
            Dim context As New ActionContext(mWebApiId, configuration, "Action1",
                                             parameters,
                                             New clsSession(mSessionId, 1000, New WebConnectionSettings(5, 5, 5,
                                                                                                        New List(Of UriWebConnectionSettings)())))
            handler.Handle(requestMock.Object, context)

            Return requestMock.Object

        End Function

        Private Sub AssertExpectedRequestCredentials(request As HttpWebRequest, credential As TestCredential)

            Dim requestCredentials = DirectCast(request.Credentials, NetworkCredential)
            requestCredentials.Should.NotBeNull()
            requestCredentials.UserName.Should.Be(credential.Username)
            requestCredentials.SecurePassword.AsString.Should.Be(credential.Password.AsString)

        End Sub

    End Class

End Namespace

#End If