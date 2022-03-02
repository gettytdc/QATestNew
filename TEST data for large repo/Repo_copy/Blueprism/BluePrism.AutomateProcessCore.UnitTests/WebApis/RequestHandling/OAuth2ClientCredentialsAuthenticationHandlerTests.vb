#If UNITTESTS Then


Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports Moq
Imports NUnit.Framework
Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens

Namespace WebApis.RequestHandling
    Public Class OAuth2ClientCredentialsAuthenticationHandlerTests
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
            Dim authentication = New BearerTokenAuthentication(AuthenticationCredential)
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
            Return New OAuth2ClientCredentialsAuthentication(AuthenticationCredential,
                                                           "",
                                                           New Uri("http://www.authserver.com"))
        End Function

        Protected Overrides Function CreateClassUnderTest() As IAuthenticationHandler
            Dim accessTokenRequesterMock As New Mock(Of IOAuth2ClientCredentialsAccessTokenRequester)

            Return New OAuth2ClientCredentialsAuthenticationHandler(AccessTokenPoolMock.Object,
                                                              CredentialHelperMock.Object,
                                                              accessTokenRequesterMock.Object)
        End Function


    End Class

End Namespace

#End If

