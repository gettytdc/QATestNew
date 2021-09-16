Namespace Auth

    ''' <summary>
    ''' Event args detailing an event which occurred with respect to a role.
    ''' </summary>
    Public Class RoleEventArgs : Inherits EventArgs

        ' The role affected by the event
        Private mRole As Role

        ''' <summary>
        ''' Creates a new event args regarding a role
        ''' </summary>
        ''' <param name="role">The role involved in the event</param>
        Public Sub New(ByVal role As Role)
            If role Is Nothing Then Throw New ArgumentNullException(NameOf(role))
            mRole = role
        End Sub

        ''' <summary>
        ''' The role involved in the event
        ''' </summary>
        Public ReadOnly Property Role() As Role
            Get
                Return mRole
            End Get
        End Property

    End Class

End Namespace
