''' <summary>
''' Delegate for handling active queue session events.
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event, providing the queue and session
''' that the event refers to</param>
Public Delegate Sub ActiveQueueSessionEventHandler(
    sender As Object, e As ActiveQueueSessionEventArgs)

''' <summary>
''' Event args detailing an active queue session event
''' </summary>
Public Class ActiveQueueSessionEventArgs : Inherits ActiveQueueEventArgs

    ' The ID of the session regarding which this event was invoked
    Private mSessionId As Guid

    ' The name of the resource that the session is/was on
    Private mResourceName As String

    ''' <summary>
    ''' Creates a new active queue session event args.
    ''' </summary>
    ''' <param name="ident">The identity of the queue for which the session exists.
    ''' </param>
    ''' <param name="sessId">The ID of the session that this event is on behalf of.
    ''' </param>
    Public Sub New(ident As Integer, sessId As Guid)
        Me.New(ident, sessId, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new active queue session event args.
    ''' </summary>
    ''' <param name="ident">The identity of the queue for which the session exists.
    ''' </param>
    ''' <param name="sessId">The ID of the session that this event is on behalf of.
    ''' </param>
    Public Sub New(ident As Integer, sessId As Guid, resName As String)
        MyBase.New(ident)
        mSessionId = sessId
        mResourceName = resName
    End Sub

    ''' <summary>
    ''' The ID of the session to which this event refers
    ''' </summary>
    Public ReadOnly Property SessionId As Guid
        Get
            Return mSessionId
        End Get
    End Property

    ''' <summary>
    ''' The name of the resource that the session in this args object relates to, or
    ''' an empty string if no resource name is set in these args.
    ''' </summary>
    Public ReadOnly Property ResourceName As String
        Get
            Return If(mResourceName, "")
        End Get
    End Property

End Class
