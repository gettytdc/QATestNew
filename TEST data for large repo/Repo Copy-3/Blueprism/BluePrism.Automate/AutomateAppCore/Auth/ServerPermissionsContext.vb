
Namespace Auth

    ''' <summary>
    ''' Context properties for checking server permissions
    ''' </summary>
    Public Class ServerPermissionsContext

        ''' <summary>Should all local requests be allowed, regardless of user and permissions </summary>
        Public Property AllowAnyLocalCalls As Boolean

        ''' <summary>If the request is local</summary>
        Public Property IsLocal As Boolean

        ''' <summary>The user making the request</summary>
        Public Property User As IUser

        ''' <summary>The required permissions for this request</summary>
        Public Property Permissions As String()

    End Class

End Namespace
