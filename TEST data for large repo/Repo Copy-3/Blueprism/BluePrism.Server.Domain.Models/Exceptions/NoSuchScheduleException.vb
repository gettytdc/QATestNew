Imports System.Runtime.Serialization

<Serializable> _
Public Class NoSuchScheduleException : Inherits NoSuchElementException

    ''' <summary>
    ''' Creates a new exception indicating that no schedule with the specified id
    ''' was found
    ''' </summary>
    ''' <param name="scheduleId">The id of the schedule which was not found.
    ''' </param>
    Public Sub New(scheduleId As Integer)
        MyBase.New("The schedule with id '{0}' was not found", scheduleId)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
