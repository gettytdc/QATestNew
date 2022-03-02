Imports Newtonsoft.Json

Namespace WebApis.AccessTokens

    ''' <summary>
    ''' Class that represents an access token used to authenticate a Web API request
    ''' </summary>
    <JsonObject>
    Public Class AccessToken

        ''' <summary>
        ''' The access token value
        ''' </summary>
        ReadOnly Property AccessToken As String

        ''' <summary>
        ''' The date the access token expires in UTC
        ''' </summary>
        ReadOnly Property ExpiryDate As DateTime?

        ''' <summary>
        ''' Indicates whether the access token is valid
        ''' </summary>
        ReadOnly Property Valid As Boolean

        ''' <summary>
        ''' Create a new instance of an <see cref="AccessToken"/>
        ''' </summary>
        ''' <param name="accessToken">The access token value</param>
        ''' <param name="expiryDate">The date the access token expires in UTC</param>
        ''' <param name="valid">Indicates whether the access token is valid</param>
        Public Sub New(accessToken As String, expiryDate As DateTime?, valid As Boolean)
            Me.AccessToken = accessToken
            Me.ExpiryDate = expiryDate
            Me.Valid = valid
        End Sub

        ''' <summary>
        ''' Create a new instance of an <see cref="AccessToken"/> from the json response
        ''' received from an authorization server.
        ''' </summary>
        ''' <param name="access_token">The access token value</param>
        ''' <param name="expires_in">The number of seconds until the token expires</param>
        ''' <param name="token_type">The type of authorization the token is to be used for</param>
        <JsonConstructor>
        Public Sub New(access_token As String, expires_in As Integer?, token_type As String)

            Me.AccessToken = access_token
            Me.ExpiryDate = 
                If(expires_in IsNot Nothing, 
                   GetExpiryDate(expires_in, DateTime.UtcNow), 
                   Nothing)
            Me.Valid = True

        End Sub

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim token = TryCast(obj, AccessToken)
            If token Is Nothing Then Return False
            Return token.AccessToken = AccessToken
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return AccessToken.GetHashCode()
        End Function

        ''' <summary>
        ''' Calculates the expiry date of an access token (in UTC) from the passed 
        ''' expiresIn value provided by the authorization server.
        ''' </summary>
        ''' <param name="expiresIn">Number of seconds until the token expires</param>
        ''' <returns>The access token's expiry date in UTC.</returns>
        Public Shared Function GetExpiryDate(expiresIn As Integer?, relativeTo As DateTime) As DateTime?
            If expiresIn IsNot Nothing AndAlso expiresIn > 0 Then _
                Return relativeTo + TimeSpan.FromSeconds(expiresIn.Value)
            Return Nothing
        End Function
    End Class

End Namespace

