Namespace Auth
    Public Class AuthenticationServerLoginResult
        Private Sub New(result As LoginResult, issuer As String, authenticationTime As DateTimeOffset?)
            Me.Result = result
            Me.Issuer = issuer
            Me.AuthenticationTime = authenticationTime
        End Sub

        Public ReadOnly Property Result As LoginResult
        Public ReadOnly Property Issuer As String
        Public ReadOnly Property AuthenticationTime As DateTimeOffset?

        Public Shared Function Success(user As IUser, issuer As String, authenticationTime As DateTimeOffset?) As AuthenticationServerLoginResult
            Dim result = New LoginResult(LoginResultCode.Success, user)
            Return New AuthenticationServerLoginResult(result, issuer, authenticationTime)
        End Function

        Public Shared Function Failed(code As LoginResultCode) As AuthenticationServerLoginResult
            If code = LoginResultCode.Success Then Throw New ArgumentException($"Login result code cannot be {LoginResultCode.Success}")
            Dim result = New LoginResult(code)
            Return New AuthenticationServerLoginResult(result, String.Empty, Nothing)
        End Function
    End Class
End Namespace
