#If UNITTESTS Then


Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports Moq
Imports NUnit.Framework
Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens
Imports BluePrism.AutomateProcessCore.WebApis
Imports System.Net.Http
Imports System.Net

Namespace WebApis.RequestHandling
    Public MustInherit Class OAuth2AuthenticationHandlerTests

        Private mAccessToken As AccessToken
        Private mContext As ActionContext
        Private mCredential As ICredential = TestCredential.Barry
        Private mConfiguration As WebApiConfiguration
        Private mRequest As HttpWebRequest

        Protected ClassUnderTest As IAuthenticationHandler
        Protected AuthenticationCredential As AuthenticationCredential
        Protected CredentialHelperMock As Mock(Of IAuthenticationCredentialHelper)
        Protected AccessTokenPoolMock As Mock(Of IAccessTokenPool)
        Protected Authentication As IAuthentication

        Public Overridable Sub SetUp()
            mAccessToken = New AccessToken("acccestoken123", Nothing, True)
            AccessTokenPoolMock = New Mock(Of IAccessTokenPool)
            AuthenticationCredential = New AuthenticationCredential(mCredential.Name, False, "")

            CredentialHelperMock = New Mock(Of IAuthenticationCredentialHelper)
            CredentialHelperMock.
                Setup(Function(x) x.GetCredential(It.IsAny(Of AuthenticationCredential),
                                                  It.IsAny(Of Dictionary(Of String, clsProcessValue)),
                                                  It.IsAny(Of Guid))).
                Returns(mCredential)


            AccessTokenPoolMock.
                Setup(Function(x) x.GetAccessToken(It.IsAny(Of ActionContext),
                                                   It.IsAny(Of ICredential),
                                                   It.IsAny(Of IAccessTokenRequester))).
                Returns(mAccessToken)

            Authentication = CreateAuthenticationConfiguration()

            mConfiguration = New WebApiConfigurationBuilder().
                WithCommonAuthentication(Authentication).
                WithAction("Action1", HttpMethod.Get, "/api/action1").
                Build()

            mContext = New ActionContext(Guid.NewGuid, mConfiguration, "Action1", New Dictionary(Of String, clsProcessValue),
                                         New clsSession(Guid.NewGuid, 105,
                                                        New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)())))
            mRequest = WebRequest.CreateHttp("http://www.webapi.com")

            ClassUnderTest = CreateClassUnderTest()
        End Sub

        Protected MustOverride Function CreateAuthenticationConfiguration() As IAuthentication

        Protected MustOverride Function CreateClassUnderTest() As IAuthenticationHandler

        Public MustOverride Sub CanHandle_UnexpectedAuthenticationType_ShouldReturnFalse()

        Public MustOverride Sub CanHandle_ExpectedAuthenticationType_ShouldReturnTrue()

        Public MustOverride Sub Handle_ExpectedAuthenticationType_ShouldAddHeaderToRequest()

        Public MustOverride Sub GetCredentialParameters_ShouldBeEmpty()


        Protected Sub AssertThatAuthorizationHeaderIsAddedToRequest()
            ClassUnderTest.Handle(mRequest, mContext)
            Assert.That(mRequest.Headers(HttpRequestHeader.Authorization),
                        Iz.EqualTo($"Bearer {mAccessToken.AccessToken}"))
        End Sub

        Protected Sub AssertCredentialParametersAreEmpty()
            Dim handler = CreateClassUnderTest()
            Dim configuration = New WebApiConfigurationBuilder().
                WithCommonAuthentication(New BasicAuthentication(New AuthenticationCredential("", False, ""), False)).
                WithAction("Action1", HttpMethod.Get, "/api/action1").
                Build()

            Dim context As New ActionContext(Guid.NewGuid(), configuration, "Action1", New Dictionary(Of String, clsProcessValue),
                                             New clsSession(Guid.NewGuid, 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)())))
            Assert.IsEmpty(handler.GetCredentialParameters(context))
        End Sub


    End Class

End Namespace

#End If
