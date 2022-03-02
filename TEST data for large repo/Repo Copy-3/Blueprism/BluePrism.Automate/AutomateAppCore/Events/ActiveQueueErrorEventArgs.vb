

''' <summary>
''' Delegate used to handle active queue error events.
''' </summary>
''' <param name="sender">The source of the event, typically the active queue manager.
''' </param>
''' <param name="e">The args detailing the event</param>
Public Delegate Sub ActiveQueueErrorEventHandler(
    sender As Object, e As ActiveQueueErrorEventArgs)

''' <summary>
''' Event Args detailing the event fired when an error occurs in an active queue
''' </summary>
Public Class ActiveQueueErrorEventArgs : Inherits ActiveQueueEventArgs

    ' The exception which occurred in the active queue
    Private mException As Exception

    ''' <summary>
    ''' Creates a new active queue error event args object
    ''' </summary>
    ''' <param name="queueIdent">The identity of the queue with which the error
    ''' occurred.</param>
    ''' <param name="ex">The exception object detailing the error</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="ex"/> is null
    ''' </exception>
    Public Sub New(queueIdent As Integer, ex As Exception)
        MyBase.New(queueIdent)
        If ex Is Nothing Then Throw New ArgumentNullException(NameOf(ex))
        mException = ex
    End Sub

    ''' <summary>
    ''' The exception which occurred within an active queue.
    ''' </summary>
    Public ReadOnly Property Exception As Exception
        Get
            Return mException
        End Get
    End Property

End Class
