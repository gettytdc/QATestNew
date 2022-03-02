#If UNITTESTS Then
Imports System.Net.Http
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace DataContractRoundTrips.Generators

    Public Class WebApiTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim authenticationCredential =
                New AuthenticationCredential("Test Credential", True, "My Parameter Name")

            Dim builder As New WebApiConfigurationBuilder()
            SetUpWebApiCongurationBuilder(builder)


            Dim basicAuth = New BasicAuthentication(authenticationCredential, True)
            Dim configBasicAuth = builder.
                                            WithCommonAuthentication(basicAuth).
                                            Build()
            Dim basicAuthWebApi As New WebApi(Guid.NewGuid, "API Basic Auth", True, configBasicAuth)
            Yield Create("Full With Basic Auth", basicAuthWebApi)


            Dim emptyAuth = New EmptyAuthentication()
            Dim configEmptyAuth = builder.
                                            WithCommonAuthentication(emptyAuth).
                                            Build()
            Dim emptyAuthWebApi As New WebApi(Guid.NewGuid, "API Empty Auth", True, configEmptyAuth)
            Yield Create("Full With Empty Auth", emptyAuthWebApi)


            Dim bearerTokenAuth = New BearerTokenAuthentication(authenticationCredential)
            Dim configBearerTokenAuth = builder.
                                                    WithCommonAuthentication(bearerTokenAuth).
                                                    Build()
            Dim bearerTokenAuthWebApi As New WebApi(Guid.NewGuid, "API Bearer Token Auth",
                                                    True, configBearerTokenAuth)
            Yield Create("Full With Bearer Token Auth", bearerTokenAuthWebApi)


            Dim oAuth2ClientCredentialsAuth =
                New OAuth2ClientCredentialsAuthentication(authenticationCredential,
                                                            "scope",
                                                            New Uri("http://testAuth.com"))
            Dim configOAuth2ClientCredentialsAuth = builder.
                                                             WithCommonAuthentication(oAuth2ClientCredentialsAuth).
                                                             Build()
            Dim oAuth2ClientCredentialsAuthWebApi As New WebApi(Guid.NewGuid,
                                                                "API OAuth 2 Client Credentials Auth",
                                                                True, configOAuth2ClientCredentialsAuth)
            Yield Create("Full With OAuth2 Client Credentials Auth", oAuth2ClientCredentialsAuthWebApi)


            Dim oAuth2JwtBearerAuth = New OAuth2JwtBearerTokenAuthentication(
                                            New JwtConfiguration(
                                                "http://testAuth.com",
                                                "apiScope",
                                                "example.human@exampleemail.com",
                                                120,
                                                authenticationCredential),
                                            New Uri("http://testAuth.com/token"))
            Dim configOAuth2JwtBearerAuth = builder.
                                                WithCommonAuthentication(oAuth2JwtBearerAuth).
                                                Build()
            Dim oAuth2JwtBearerAuthWebApi As New WebApi(Guid.NewGuid, "API OAuth 2 Jwt Bearer Auth",
                                                        True, configOAuth2JwtBearerAuth)
            Yield Create("Full With OAuth2 Jwt Bearer Token Auth", oAuth2JwtBearerAuthWebApi)

            Dim customAuth = New CustomAuthentication(authenticationCredential)
            Dim configCustomAuth = builder.WithCommonAuthentication(customAuth).Build()
            Dim customAuthWebApi As New WebApi(Guid.NewGuid, "Custom Auth",
                                                        True, configCustomAuth)
            Yield Create("Full With Custom Auth", customAuthWebApi)
        End Function

        ''' <summary>
        ''' Helper method to set up a WebAPI configuration builder that can be used
        ''' to generate a full WebApiConfiguration to be used in test cases. Note
        ''' that the common authentication is not set and needs to be set elsewhere
        ''' </summary>
        ''' <param name="builder">The configuration builder to set up</param>
        Private Sub SetUpWebApiCongurationBuilder(builder As WebApiConfigurationBuilder)

            Dim sharedParameters = {New ActionParameter(
                1, "Shared Parameter 1", "Description1", DataType.text, True, New clsProcessValue("Default 1"))}
            Dim codeProperties As New CodeProperties("global code", 
                                                     CodeLanguage.CSharp,
                                                     { "Namespace1", "Namespace2"}, 
                                                     {"System.dll", "System.Data.dll"})
            builder.
                WithCommonCode(codeProperties).
                WithHeader("Shared Header 1", "Value 1", 1).
                WithHeader("Shared Header 2", "Value 2", 2).
                WithParameters(sharedParameters).
                WithAction("Action 1", HttpMethod.Get, "/action1",
                               description:="Action 1 description",
                               id:=1,
                               enabled:=True,
                               enableRequestOutputParameter:=True,
                               disableSendingOfRequest:=True,
                               bodyContent:=New TemplateBodyContent("<action1Request />"),
                               headers:={New HttpHeader(3, "Header 1", "Value 1")},
                               parameters:={New ActionParameter(
                                   2, "Parameter 1", "Description", DataType.text, True, New clsProcessValue("param 1 default"))},
                                outputParameterConfiguration:=New OutputParameterConfiguration({New JsonPathOutputParameter("test", "Description", "$.test", DataType.date)}, "code")
                               ).
                WithAction("Action 2", HttpMethod.Get, "/action2",
                               description:="Action 2 description",
                               id:=2,
                               enabled:=True,
                               bodyContent:=New TemplateBodyContent("{""action2Request"" : 2}"),
                               headers:={New HttpHeader(4, "Header 1", "Value 1")},
                               parameters:={New ActionParameter(
                                   3, "Parameter 2", "Description", DataType.text, True, New clsProcessValue("param 1 default"))},
                               outputParameterConfiguration:=New OutputParameterConfiguration({New CustomCodeOutputParameter("output1", "Description", DataType.date)}, "code")
                               ).
                WithAction("Action 3", HttpMethod.Get, "/action3",
                           description:="Action 3 description",
                           id:=3,
                           enabled:=True,
                           bodyContent:=New NoBodyContent()
                           ).
                WithAction("Action 4", HttpMethod.Get, "/action4",
                           description:="Action 4 description",
                           id:=4,
                           enabled:=True,
                           bodyContent:=New SingleFileBodyContent("Single file parameter name"),
                           outputParameterConfiguration:=New OutputParameterConfiguration({New CustomCodeOutputParameter("output1", "Description", DataType.date),
                                                                                            New JsonPathOutputParameter("test", "Description", "$.test", DataType.date)}, "Code")
                           ).
                WithAction("Action 5", HttpMethod.Get, "/action5",
                           description:="Action 5 description",
                           id:=5,
                           enabled:=True,
                           bodyContent:=New FileCollectionBodyContent("Test file collection")
                           ).
                WithAction("Action 6", HttpMethod.Get, "/action6",
                           description:="Action 6 description",
                           id:=6,
                           enabled:=True,
                           bodyContent:=New CustomCodeBodyContent("Content = ""something"";")
                           ).
                WithConfigurationSettings(New WebApiConfigurationSettings(1, 2))

        End Sub


    End Class



End Namespace

#End If
