Namespace Auth

    ''' <summary>
    ''' Event handler delegate for handling <see cref="UserEventArgs"/> events.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The args detailing the event</param>
    Public Delegate Sub UserEventHandler(sender As Object, e As UserEventArgs)

    ''' <summary>
    ''' Event Args detailing a user event.
    ''' </summary>
    Public Class UserEventArgs : Inherits EventArgs

        ' The user that this event refers to
        Private mUser As User

        ''' <summary>
        ''' Creates a new event args object, referring to a user.
        ''' </summary>
        ''' <param name="u">The user this event refers to, or null if this event
        ''' refers to the lack of a user.</param>
        Public Sub New(u As User)
            mUser = u
        End Sub

        ''' <summary>
        ''' The user that this event relates to, or null if this user event refers to
        ''' the lack of a user (eg. a 'select user' event where no user has been
        ''' selected).
        ''' </summary>
        Public ReadOnly Property User As User
            Get
                Return mUser
            End Get
        End Property

        Public Property LogoutDenied As Boolean
        Public Property LogoutDeniedMessage As String

    End Class

End Namespace
