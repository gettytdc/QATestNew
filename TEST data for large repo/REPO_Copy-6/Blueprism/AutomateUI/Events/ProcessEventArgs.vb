Imports BluePrism.AutomateProcessCore

''' <summary>
''' Delegate which describes an event handler for process events
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event</param>
Public Delegate Sub ProcessEventHandler(sender As Object, e As ProcessEventArgs)

''' <summary>
''' Creates a new event args object wrapping a process
''' </summary>
Public Class ProcessEventArgs : Inherits EventArgs

    ' The process represented in this event
    Private mProcess As clsProcess

    ''' <summary>
    ''' Creates a new event
    ''' </summary>
    ''' <param name="proc">The process that this event relates to</param>
    Public Sub New(proc As clsProcess)
        If proc Is Nothing Then Throw New ArgumentNullException(NameOf(proc))
        mProcess = proc
    End Sub

    ''' <summary>
    ''' The process that this event args object relates to
    ''' </summary>
    Public ReadOnly Property Process As clsProcess
        Get
            Return mProcess
        End Get
    End Property

End Class
