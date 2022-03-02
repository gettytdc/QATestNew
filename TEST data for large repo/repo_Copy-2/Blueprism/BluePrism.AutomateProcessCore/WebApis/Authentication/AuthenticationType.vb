Namespace WebApis.Authentication
    ''' <summary>
    ''' Types of authentication used within a Web API call
    ''' </summary>
    Public Enum AuthenticationType
        ''' <summary>
        ''' Uses no authentication
        ''' </summary>
        None = 0
        ''' <summary>
        ''' Uses Http Basic Authentication
        ''' </summary>
        Basic = 1
        ''' <summary>
        ''' Uses Bearer Token Authentication
        ''' </summary>
        BearerToken = 3
        ''' <summary>
        ''' Uses OAuth 2.0 Authentication, with a client credentials grant type
        ''' </summary>
        OAuth2ClientCredentials = 4
        ''' <summary>
        ''' Uses OAuth 2.0 Authentication, with a JWT bearer token grant type
        ''' </summary>
        OAuth2JwtBearerToken = 5

        ''' <summary>
        ''' Uses Custom Authentication
        ''' </summary>
        Custom = 6
    End Enum

    Public Class AuthenticationTypeExtensions
        Public Shared Function Getdescription(type As AuthenticationType) As String
            Return WebApiResources.ResourceManager.GetString($"AuthenticationType_{type.ToString()}")
        End Function
    End Class

End Namespace
