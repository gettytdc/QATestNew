#If UNITTESTS Then


Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports Moq
Imports NUnit.Framework
Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens

Namespace WebApis.RequestHandling
    Public Class OAuth2JwtBearerTokenAuthenticationHandlerTests
        Inherits OAuth2AuthenticationHandlerTests

        <SetUp>
        Public Overrides Sub SetUp()
            MyBase.SetUp()
        End Sub

        <Test>
        Public Overrides Sub CanHandle_ExpectedAuthenticationType_ShouldReturnTrue()
            Assert.That(ClassUnderTest.CanHandle(Authentication), Iz.True)
        End Sub

        <Test>
        Public Overrides Sub CanHandle_UnexpectedAuthenticationType_ShouldReturnFalse()
            Dim authentication = New OAuth2ClientCredentialsAuthentication(AuthenticationCredential, "api",
                                                          New Uri("http://www.authserver.com"))
            Assert.That(ClassUnderTest.CanHandle(authentication), Iz.False)
        End Sub

        <Test>
        Public Overrides Sub Handle_ExpectedAuthenticationType_ShouldAddHeaderToRequest()
            AssertThatAuthorizationHeaderIsAddedToRequest()
        End Sub

        <Test>
        Public Overrides Sub GetCredentialParameters_ShouldBeEmpty()
            AssertCredentialParametersAreEmpty()
        End Sub

        Protected Overrides Function CreateAuthenticationConfiguration() As IAuthentication
            Dim jwt As New JwtConfiguration("http://www.authserver.com", "api", "",
                                            3600, AuthenticationCredential)
            Return New OAuth2JwtBearerTokenAuthentication(jwt, New Uri("http://www.authserver.com"))
        End Function

        Protected Overrides Function CreateClassUnderTest() As IAuthenticationHandler
            Dim accessTokenRequesterMock As New Mock(Of IOAuth2JwtBearerTokenAccessTokenRequester)

            Return New OAuth2JwtBearerTokenAuthenticationHandler(AccessTokenPoolMock.Object,
                                                              CredentialHelperMock.Object,
                                                              accessTokenRequesterMock.Object)
        End Function
    End Class

End Namespace

#End If

