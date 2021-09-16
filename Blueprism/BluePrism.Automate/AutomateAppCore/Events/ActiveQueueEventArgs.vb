''' <summary>
''' Delegate for handling active queue events.
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event, providing the queue that the event
''' refers to</param>
Public Delegate Sub ActiveQueueEventHandler(
    sender As Object, e As ActiveQueueEventArgs)

''' <summary>
''' Event args detailing an active queue event
''' </summary>
Public Class ActiveQueueEventArgs : Inherits EventArgs

    ' The identity of the queue on which the active session occurred
    Private mQueueIdent As Integer

    ''' <summary>
    ''' Creates a new active queue event args.
    ''' </summary>
    ''' <param name="ident">The identity of the queue for which the session exists.
    ''' </param>
    Public Sub New(ident As Integer)
        mQueueIdent = ident
    End Sub

    ''' <summary>
    ''' The identity of the active queue for which the session was created.
    ''' </summary>
    Public ReadOnly Property QueueIdent As Integer
        Get
            Return mQueueIdent
        End Get
    End Property

End Class
