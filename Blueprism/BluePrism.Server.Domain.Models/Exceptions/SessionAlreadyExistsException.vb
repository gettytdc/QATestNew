
Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a session log cannot be restored because a session log
''' with the same ID already exists on the database - implying that it has been
''' restored once already, or never deleted from the database.
''' </summary>
<Serializable()> _
Public Class SessionAlreadyExistsException : Inherits AlreadyExistsException

    ' The session ID of the pre-existent session.
    Private mSessionId As Guid

    ''' <summary>
    ''' Creates a new exception with the default message, indicating that the session
    ''' log with the given ID is already on the database.
    ''' </summary>
    Public Sub New(ByVal sessId As Guid)
        MyBase.New(My.Resources.SessionAlreadyExistsException_RestoringASessionLogFailedBecauseASessionWithThatIDAlreadyExists)
        mSessionId = sessId
    End Sub

    ''' <summary>
    ''' Deserializes an exception indicating that a session that is being restored
    ''' already exists on the database.
    ''' </summary>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
        mSessionId = DirectCast(info.GetValue("sessionId", GetType(Guid)), Guid)
    End Sub

    ''' <summary>
    ''' The session number of the session log which could not be restored
    ''' </summary>
    Public ReadOnly Property SessionId() As Guid
        Get
            Return mSessionId
        End Get
    End Property

    ''' <summary>
    ''' Gets the object data for this exception in order to enable serialization.
    ''' </summary>
    ''' <param name="info">The serialization info for this serialization process.
    ''' </param>
    ''' <param name="context">The streaming context. Nope. No idea. Complete blank.
    ''' </param>
    Public Overrides Sub GetObjectData( _
                                       ByVal info As SerializationInfo, ByVal context As StreamingContext)

        If info Is Nothing Then Throw New ArgumentNullException(NameOf(info))
        info.AddValue("sessionId", mSessionId)
        MyBase.GetObjectData(info, context)

    End Sub

End Class
