Imports BluePrism.AutomateAppCore

''' <summary>
''' Delegate which describes an event handler for Queue events
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event</param>
Public Delegate Sub QueueEventHandler(sender As Object, e As QueueEventArgs)

''' <summary>
''' Creates a new event args object wrapping a Queue
''' </summary>
Public Class QueueEventArgs : Inherits EventArgs

    ' The Queue represented in this event
    Private mQueue As clsWorkQueue

    ''' <summary>
    ''' Creates a new event
    ''' </summary>
    ''' <param name="wq">The Queue that this event relates to</param>
    Public Sub New(wq As clsWorkQueue)
        mQueue = wq
    End Sub

    ''' <summary>
    ''' The Queue that this event args object relates to, or null if it relates to
    ''' 'no queue' (used, for example, if a queue selection has changed to nothing
    ''' being selected).
    ''' </summary>
    Public ReadOnly Property Queue As clsWorkQueue
        Get
            Return mQueue
        End Get
    End Property

    ''' <summary>
    ''' Gets the ID of the wrapped queue, or <see cref="Guid.Empty"/> if this event
    ''' args object holds no queue.
    ''' </summary>
    Public ReadOnly Property QueueId As Guid
        Get
            Return If(mQueue Is Nothing, Guid.Empty, mQueue.Id)
        End Get
    End Property

    ''' <summary>
    ''' Gets the identity of the wrapped queue, or 0 if this event args object holds
    ''' no queue.
    ''' </summary>
    Public ReadOnly Property QueueIdent As Integer
        Get
            Return If(mQueue Is Nothing, 0, mQueue.Ident)
        End Get
    End Property

End Class
