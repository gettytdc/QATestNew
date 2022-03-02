Imports IdentityModel.Client

Namespace clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer
    Public Class TokenResponseWrapper
        Implements ITokenResponseWrapper

        Public ReadOnly Property AccessToken As String Implements ITokenResponseWrapper.AccessToken
        Public ReadOnly Property ExpiresIn As Integer Implements ITokenResponseWrapper.ExpiresIn
        Public ReadOnly Property Exception As Exception Implements ITokenResponseWrapper.Exception
        Public ReadOnly Property IsError As Boolean Implements ITokenResponseWrapper.IsError
        Public ReadOnly Property [Error] As String Implements ITokenResponseWrapper.[Error]

        Public Sub New(response As TokenResponse)
            AccessToken = response.AccessToken
            ExpiresIn = response.ExpiresIn
            Exception = response.Exception
            IsError = response.IsError
            [Error] = response.Error
        End Sub
    End Class
End Namespace
