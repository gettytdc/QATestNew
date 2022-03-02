Imports BluePrism.Scheduling

''' <summary>
''' Exception thrown when a session could not be created.
''' </summary>
<Serializable>
Public Class ScheduledSessionCreationFailedException
    Inherits ScheduleException

    ' Diag data for the exception
    Private mData As Object

    ' The session that failed to be created.
    Private mSession As ScheduledSession

    ''' <summary>
    ''' Diagnostic data to aid in explaining the exception
    ''' </summary>
    Public ReadOnly Property DiagData() As Object
        Get
            Return mData
        End Get
    End Property

    ''' <summary>
    ''' The session object which has failed to be created.
    ''' Note that the error message and diagnostic data may not remain in the
    ''' session by catchers of this exception - it is safe to retrieve the 
    ''' error message using <see cref="Exception.Message"/> and the diag data
    ''' using <see cref="DiagData"/>.
    ''' </summary>
    Public ReadOnly Property Session() As ScheduledSession
        Get
            Return mSession
        End Get
    End Property

    ''' <summary>
    ''' Creates a new exception indicating that the given session could not be
    ''' created. This assumes that the sessions 'ErrorMessage' property has been
    ''' set and the message can be used as the message for this exception.
    ''' </summary>
    ''' <param name="sess">The session which has failed with its ErrorMessage
    ''' property giving a detailed error message.</param>
    ''' <exception cref="NullReferenceException">If the given session was null.
    ''' </exception>
    Public Sub New(ByVal sess As ScheduledSession)
        MyBase.New(sess.ErrorMessage)
        mData = sess.Data
        mSession = sess
    End Sub

End Class
