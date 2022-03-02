Namespace clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer
    Public Interface ITokenResponseWrapper
        ReadOnly Property AccessToken As String
        ReadOnly Property ExpiresIn As Integer
        ReadOnly Property Exception As Exception
        ReadOnly Property IsError As Boolean
        ReadOnly Property [Error] As String
    End Interface
End Namespace
