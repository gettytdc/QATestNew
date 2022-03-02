Imports BluePrism.ClientServerResources.Core.Data
Imports BluePrism.ClientServerResources.Core.Interfaces
Imports NLog

Public Class TokenValidator
    Implements ITokenValidator

    Private Log As Logger = LogManager.GetCurrentClassLogger()
    ''' <summary>
    ''' Basic token auth function.  This can be expanded to check permission etc of the user in future.
    ''' </summary>
    ''' <param name="token"></param>
    ''' <returns></returns>
    Public Function Validate(token As String) As TokenValidationInfo Implements ITokenValidator.Validate
        Log.Debug($"Validating user token {token}")
        If String.IsNullOrEmpty(token) Then
            Throw New ArgumentException($"'{NameOf(token)}' cannot be null or empty", NameOf(token))
        End If
        Try
            Dim authToken = New clsAuthToken(token)
            Dim reason As String = Nothing
            Dim user = authToken.Validate(reason)

            If user Is Nothing Then
                Log.Debug($"Unknown or invalid user token {token}, failure reason: {reason}")
                Return TokenValidationInfo.FailureTokenInfo(reason)
            End If
            Return TokenValidationInfo.SuccessTokenInfo(user.Name, user.Id)
        Catch ex As Exception
            Log.Debug(ex, $"Exception validating user token {token}")
            Return TokenValidationInfo.FailureTokenInfo(ex.Message)
        End Try
    End Function
End Class
