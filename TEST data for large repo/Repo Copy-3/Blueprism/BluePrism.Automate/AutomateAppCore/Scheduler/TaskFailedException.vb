Imports BluePrism.Scheduling

''' <summary>
''' Exception thrown when a task has failed.
''' </summary>
<Serializable>
Public Class TaskFailedException
    Inherits ScheduleException

    ''' <summary>
    ''' Creates a new exception indicating that the given task has failed,
    ''' with the given detail message and the specified cause.
    ''' </summary>
    ''' <param name="task">The task which has failed.</param>
    ''' <param name="message">The user-presentable reason why the task
    ''' failed.</param>
    ''' <param name="cause">The cause of the exception.</param>
    Public Sub New(ByVal task As ScheduledTask, ByVal message As String, ByVal cause As Exception)
        MyBase.New(message, cause)
    End Sub

    ''' <summary>
    ''' Creates a new exception indicating that the given task has failed,
    ''' with the given detail message.
    ''' </summary>
    ''' <param name="task">The task which has failed.</param>
    ''' <param name="message">The user-presentable reason why the task
    ''' failed.</param>
    Public Sub New(ByVal task As ScheduledTask, ByVal message As String)
        Me.New(task, message, Nothing)
    End Sub


End Class
