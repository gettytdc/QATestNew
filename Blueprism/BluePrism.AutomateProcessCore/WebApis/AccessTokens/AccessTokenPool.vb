Imports System.Collections.Concurrent
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.Core.Utility

Namespace WebApis.AccessTokens

    ''' <summary>
    ''' A pool that stores access tokens that can be used for authenticating Web API 
    ''' requests. Access tokens are uniquely identified by the Web API they will be 
    ''' authenticating requests to and the credential that identifies who is making 
    ''' the request.
    ''' </summary>
    Public Class AccessTokenPool
        Implements IAccessTokenPool

        Private mPool As New ConcurrentDictionary(Of AccessTokenPoolKey, AccessToken)
        Private ReadOnly mSystemClock As ISystemClock

        Sub New(clock As ISystemClock)
            mSystemClock = clock
        End Sub

        ''' <inheritdoc/>
        Public Function GetAccessToken(context As ActionContext, credential As ICredential, requester As IAccessTokenRequester) _
            As AccessToken Implements IAccessTokenPool.GetAccessToken

            Dim token As AccessToken = Nothing
            Dim key As New AccessTokenPoolKey(context.WebApiId, credential.Name)

            If mPool.TryGetValue(key, token) Then
                If CanAccessTokenBeUsed(token) Then
                    Return token
                Else
                    Dim newToken = requester.RequestAccessToken(context, credential)
                    mPool.TryUpdate(key, newToken, token)
                    Return GetAccessToken(context, credential, requester)
                End If
            Else
                Dim newToken = requester.RequestAccessToken(context, credential)
                mPool.TryAdd(key, newToken)
                Return GetAccessToken(context, credential, requester)
            End If

        End Function

        ''' <inheritdoc/>
        Public Sub InvalidateToken(webApiId As Guid, credentialName As String, accessToken As AccessToken) _
            Implements IAccessTokenPool.InvalidateToken
            Dim key = New AccessTokenPoolKey(webApiId, credentialName)
            Dim invalidAccessToken =
                New AccessToken(accessToken.AccessToken, accessToken.ExpiryDate, False)
            mPool.TryUpdate(key, invalidAccessToken, accessToken)
        End Sub

        ''' <summary>
        ''' Checks if the access token can be used to authenticate a Web API
        ''' request.
        ''' </summary>
        ''' <param name="token">The access token to check</param>
        ''' <returns>Returns True, if the access token is both valid and isn't due to 
        ''' expire in the next 30 seconds.</returns>
        Public Function CanAccessTokenBeUsed(token As AccessToken) As Boolean _
            Implements IAccessTokenPool.CanAccessTokenBeUsed
            Return token.Valid AndAlso If(token.ExpiryDate.HasValue,
                (token.ExpiryDate.Value - mSystemClock.UtcNow.UtcDateTime).TotalSeconds > 30, True)
        End Function

    End Class

End Namespace
