#If UNITTESTS Then

Imports System.Net
Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports FluentAssertions
Imports Moq
Imports NUnit.Framework
Imports BluePrism.Common.Security

Namespace WebApis.RequestHandling

    Public Class BearerTokenAuthenticationHandlerTests

        ''' <summary>
        ''' Shared test credential
        ''' </summary>
        Private Shared ReadOnly Credential1 As New TestCredential("Frank", "FrankM", "secureaf")
        ''' <summary>
        ''' Name of parameter used through tests
        ''' </summary>
        Private Const CredentialParameterName = "Bearer Token Auth Credential"
        ''' <summary>
        ''' The web api id passed through the tests
        ''' </summary>
        Private ReadOnly mWebApiId As New Guid("ACD2D25C-5709-48EB-804A-E1EBA831C672")

        Private ReadOnly mSessionId As New Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722")

        <Test>
        Public Sub Handle_SelectedCredential_ShouldSetHeader()

            Dim credential = New AuthenticationCredential(Credential1.Name, False, CredentialParameterName)
            Dim authentication As New BearerTokenAuthentication(credential)

            Dim request = Handle(Credential1, authentication)

            request.Credentials.Should.BeNull()
            AssertExpectedAuthorizationHeader(request, Credential1)

        End Sub

        <Test>
        Public Sub Handle_WithCredentialParameter_ShouldSetHeader()

            Dim credential = New AuthenticationCredential("", True, CredentialParameterName)
            Dim authentication As New BearerTokenAuthentication(credential)
            Dim parameters As New Dictionary(Of String, clsProcessValue) From {{CredentialParameterName, New clsProcessValue(Credential1.Name)}}

            Dim request = Handle(Credential1, authentication, parameters)

            request.Credentials.Should.BeNull()
            AssertExpectedAuthorizationHeader(request, Credential1)

        End Sub

        Private Sub AssertExpectedAuthorizationHeader(request As HttpWebRequest, credential As ICredential)
            Dim header = request.Headers(HttpRequestHeader.Authorization)
            header.Should.NotBeNullOrWhiteSpace()
            header.Should.Be($"Bearer {credential.Password.AsString}")
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

            Dim requestMock = MockRequestHelper.Create()
            Dim configuration = New WebApiConfigurationBuilder().
                WithCommonAuthentication(authentication).
                WithAction("Action1", HttpMethod.Get, "/api/action1").
                Build()

            Dim authenticationHandler As New BearerTokenAuthenticationHandler(credentialHelperMock.Object)

            If parameters Is Nothing Then parameters = New Dictionary(Of String, clsProcessValue)
            Dim session As New clsSession(mSessionId, 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)()))
            Dim context As New ActionContext(mWebApiId, configuration, "Action1", parameters, session)

            authenticationHandler.Handle(requestMock.Object, context)

            Return requestMock.Object

        End Function

    End Class

End Namespace

#End If