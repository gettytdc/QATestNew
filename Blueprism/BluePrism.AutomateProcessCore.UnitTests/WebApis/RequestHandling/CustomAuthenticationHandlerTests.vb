#If UNITTESTS

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

    Public Class CustomAuthenticationHandlerTests

        ''' <summary>
        ''' Name of parameter used through tests
        ''' </summary>
        Private Const CredentialParameterName = "Custom Auth Credential"
        ''' <summary>
        ''' The web api id passed through the tests
        ''' </summary>
        Private ReadOnly mWebApiId As New Guid("ACD2D25C-5709-48EB-804A-E1EBA831C672")
        Private ReadOnly mSessionId As New Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722")

        <Test>
        Public Sub Handle_WithInvalidAuthentication_ShouldThrow()

            Dim handler As New CustomAuthenticationHandler(Nothing)
            Dim requestMock = MockRequestHelper.Create()
            Dim configuration = New WebApiConfigurationBuilder().
                WithCommonAuthentication(New EmptyAuthentication()).
                WithAction("Action1", HttpMethod.Get, "/api/action1").
                Build()
            Dim session As New clsSession(
                               mSessionId, 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)))
            Dim context As New ActionContext(
                mWebApiId, configuration, "Action1", New Dictionary(Of String, clsProcessValue), session)

            Dim action As Action = Sub() handler.Handle(requestMock.Object, context)
            action.ShouldThrow(Of ArgumentException)

        End Sub

        <Test>
        Public Sub GetCredentialParameters_ShouldHaveAllParameters()
            Dim credentialHelper = New Mock(Of IAuthenticationCredentialHelper)
            credentialHelper.Setup(Function(c) c.GetCredential(It.IsAny(Of AuthenticationCredential),
                                                               It.IsAny(Of Dictionary(Of String, clsProcessValue)),
                                                               It.IsAny(Of Guid))).
                                      Returns(Function()
                                                  Dim testCred = New TestCredential("testCred", "bob", "secret")
                                                  testCred.Properties.Add("Key", New SafeString("theKeyValue"))
                                                  testCred.Properties.Add("SecretAnswer", New SafeString("aPig"))
                                                  Return testCred
                                              End Function)

            Dim handler As New CustomAuthenticationHandler(credentialHelper.Object)
            Dim configuration = New WebApiConfigurationBuilder().
                WithCommonAuthentication(New CustomAuthentication(New AuthenticationCredential("", False, ""))).
                WithAction("Action1", HttpMethod.Get, "/api/action1").
                Build()
            Dim session As New clsSession(
                mSessionId, 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)()))

            Dim context As New ActionContext(mWebApiId, configuration, "Action1", New Dictionary(Of String, clsProcessValue), session)
            Dim params = handler.GetCredentialParameters(context)

            params.Count.Should().Be(4)
            CStr(params("Credential.Username")).Should().Be("bob")
            CType(params("Credential.Password"), SafeString).Should().Be(New SafeString("secret"))
            CType(params("Credential.AdditionalProperties.Key"), SafeString).Should().Be(New SafeString("theKeyValue"))
            CType(params("Credential.AdditionalProperties.SecretAnswer"), SafeString).Should().Be(New SafeString("aPig"))
        End Sub

    End Class

End Namespace

#End If