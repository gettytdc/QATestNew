Imports System.Runtime.Serialization

''' <summary>
''' Contains meta-information about the data stored within a credential, including the 
''' properties used and the titles displayed to the user. Built-in CredentialTypes 
''' exist for various known usages of credentials data.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class CredentialType 
    
    ''' <summary>
    ''' The general credential type of any authentication.
    ''' </summary>
    ''' <returns>General credential type</returns>
    Public Shared Property General As New CredentialType("General", True, True)

    ''' <summary>
    ''' The credential type for basic authentication.
    ''' </summary>
    ''' <returns>Basic authentication credential type</returns>
    Public Shared Property BasicAuthentication As New CredentialType("BasicAuthentication", True, True)

    ''' <summary>
    ''' The credential type for OAuth 2.0 (Client Credentials) authentication.
    ''' </summary>
    ''' <returns>OAuth 2.0 (Client Credentials) authentication credential type</returns>
    Public Shared Property OAuth2ClientCredentials As New CredentialType("OAuth2ClientCredentials", True, True)

    ''' <summary>
    ''' The credential type for OAuth 2.0 (JWT Bearer Token) authentication.
    ''' </summary>
    ''' <returns>OAuth 2.0 (JWT Bearer Token) authentication credential type</returns>
    Public Shared Property OAuth2JwtBearerToken As New CredentialType("OAuth2JwtBearerToken", True, True)

    ''' <summary>
    ''' The credential type for Bearer Token authentication.
    ''' </summary>
    ''' <returns>Bearer token credential type</returns>

    Public Shared Property BearerToken As New CredentialType("BearerToken", False, True)

    ''' <summary>
    ''' The credential type for use in a Data Gateway configuration.
    ''' </summary>
    ''' <returns></returns>
    Public Shared Property DataGatewayCredentials As New CredentialType("DataGatewayCredentials", True, False)

    ''' <summary>
    ''' Gets a list of all the credential types.
    ''' </summary>
    ''' <returns>A collection of credential types.</returns>
    Public Shared Function GetAll() As IEnumerable(Of CredentialType)
        Return {General, BasicAuthentication, OAuth2ClientCredentials, OAuth2JwtBearerToken, BearerToken, DataGatewayCredentials}
    End Function

    ''' <summary>
    ''' Gets the credential with the specified name. 
    ''' </summary>
    ''' <param name="name">The name of the credential type expected</param>
    ''' <returns>The corresponding credential type.</returns>
    ''' <exception cref="ArgumentException">Error thrown if the name is not recognised</exception>
    Public Shared Function GetByName(name As String) As CredentialType
        Dim match = GetAll().FirstOrDefault(Function(t) t.Name = name)

        If match Is Nothing Then _
            Throw New ArgumentException(String.Format(My.Resources.CredentialType_CredentialType0NotRecognised, name), NameOf(name))

        Return match
    End Function

    Private Sub New(credentialName As String, usernameVisible As Boolean, accessRightsVisible As Boolean)
        Name = credentialName
        IsUsernameVisible = usernameVisible
        IsAccessRightsVisible = accessRightsVisible
    End Sub

    ''' <summary>
    ''' The name of the credential type.
    ''' </summary>
    Public ReadOnly Property Name As String

    ''' <summary>
    ''' Should the username be visible when editing a credential of this type.
    ''' </summary>
    Public ReadOnly Property IsUsernameVisible As Boolean

    ''' <summary>
    ''' Is the access rights tab visible when editing a credential of this type.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsAccessRightsVisible As Boolean


    ''' <summary>
    ''' Gets the user-facing title for this CredentialType for the current culture
    ''' </summary>
    Public ReadOnly Property LocalisedTitle() as String
        Get
            Return CredentialsResources.ResourceManager.
                GetString($"CredentialTypes_{Name}_Title")
        End Get
    End Property

    ''' <summary>
    ''' Gets the user-facing title for the username property for the current culture
    ''' </summary>
    Public ReadOnly Property LocalisedUsernamePropertyTitle() as String
        Get
            Return CredentialsResources.ResourceManager.
                GetString($"CredentialTypes_{Name}_UsernamePropertyTitle")
        End Get
    End Property

    ''' <summary>
    ''' Gets the user-facing title for the password property for the current culture
    ''' </summary>
    Public ReadOnly Property LocalisedPasswordPropertyTitle() As String
        Get
            Return CredentialsResources.ResourceManager.
                GetString($"CredentialTypes_{Name}_PasswordPropertyTitle")
        End Get
    End Property

    ''' <summary>
    ''' Gets the user-facing description of the credential type for the current culture
    ''' </summary>
    Public ReadOnly Property LocalisedDescription() As String
        Get
            Return CredentialsResources.ResourceManager.
                GetString($"CredentialTypes_{Name}_Description")
        End Get
    End Property

    ''' <summary>
    ''' Override of ToString()
    ''' </summary>
    ''' <returns>The name of the credential type</returns>
    Public Overrides Function ToString() As String
        Return Name
    End Function
    
End Class
