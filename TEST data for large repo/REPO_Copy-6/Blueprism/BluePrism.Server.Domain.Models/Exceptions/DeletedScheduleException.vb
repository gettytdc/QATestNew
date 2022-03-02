Imports System.Runtime.Serialization

<Serializable>
Public Class DeletedScheduleException : Inherits DeletedException

    ''' <summary>
    ''' Creates a new exception indicating that schedule has been deleted
    ''' </summary>
    ''' <param name="scheduleId">The id of the deleted schedule.
    ''' </param>
    Public Sub New(scheduleId As Integer)
        MyBase.New("The schedule with id '{0}' has been deleted", scheduleId)
    End Sub

    Protected Sub New(info As SerializationInfo, ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub
End Class

