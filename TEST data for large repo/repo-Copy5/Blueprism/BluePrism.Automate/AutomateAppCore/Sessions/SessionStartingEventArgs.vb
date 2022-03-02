Namespace Sessions

    Public Class SessionStartingEventArgs
        Inherits EventArgs

        Public ReadOnly Property Sessions As IEnumerable(Of clsProcessSession)

        Public Sub New(sessions As IEnumerable(Of clsProcessSession))
            Me.Sessions = sessions
        End Sub
    End Class

End Namespace
