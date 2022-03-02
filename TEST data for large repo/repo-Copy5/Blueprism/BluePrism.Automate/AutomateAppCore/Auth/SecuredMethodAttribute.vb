Namespace Auth
    ''' <summary>
    ''' Decorates server methods that require a logged in user and optionally 
    ''' specific permissions
    ''' </summary>
    Public Class SecuredMethodAttribute
        Inherits Attribute

        ''' <summary>
        ''' The list of permissions required to access the server method. If any
        ''' permissions are specified, the current user must have one of the 
        ''' permissions to access the method. If empty, then the user must be 
        ''' logged in but no specific permissions are required.
        ''' </summary>
        Public Property Permissions() As String()

        ''' <summary>
        ''' Indicates whether the server method can be called directly. This is
        ''' used to protect methods that require an authenticated user when 
        ''' accessed via server connection but can execute without an 
        ''' authenticated user when called locally.
        ''' </summary>
        Public Property AllowLocalUnsecuredCalls As Boolean

        ''' <summary>
        ''' Creates a new SecuredMethod attribute
        ''' </summary>
        ''' <param name="permissions">The list of permissions required to access the 
        ''' server method. If any permissions are specified, the current user 
        ''' must have one of the permissions. If no permissions are specified, 
        ''' then the user must be logged in but no specific permissions are 
        ''' required.</param>
        Sub New(ParamArray permissions() As String)
            Me.Permissions = permissions
        End Sub

        ''' <summary>
        ''' Creates a new SecuredMethod attribute 
        ''' </summary>
        ''' <param name="forReview">Indicates whether review is needed 
        ''' (used during development) </param>
        Sub New(forReview As Boolean)
        End Sub

    End Class
End NameSpace