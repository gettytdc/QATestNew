''' <summary>
''' Stub class to represent an empty log context where a context is required, but no
''' specific information is required.
''' </summary>
Public Class EmptyLogContext : Implements ILogContext

    ''' <summary>
    ''' The single empty log context implementation
    ''' </summary>
    Public Shared ReadOnly Property Empty As ILogContext = New EmptyLogContext()

    ''' <summary>
    ''' Creates an empty log context
    ''' </summary>
    Private Sub New()
        ' The only thing we want a non-zeroed return value for is Attempts
        Attempts = 1
    End Sub

#Region " Empty Implementation "

    ''' <summary>
    ''' Gets or sets the predefined number of attempts which should be made to write
    ''' a log entry before either failing or continuing, as decided by the
    ''' <see cref="ILogContext.FailOnError"/> property value.
    ''' </summary>
    Public ReadOnly Property Attempts As Integer Implements ILogContext.Attempts

    ''' <summary>
    ''' Indicates whether the context is a debugging context
    ''' </summary>
    Public ReadOnly Property Debugging As Boolean Implements ILogContext.Debugging

    ''' <summary>
    ''' Gets a flag indicating that the session should fail if any error
    ''' occurs while logging, after a defined number of attempts.
    ''' </summary>
    Public ReadOnly Property FailOnError As Boolean Implements ILogContext.FailOnError

    ''' <summary>
    ''' The master process for the session that the log that this context represents
    ''' was created for
    ''' </summary>
    Public ReadOnly Property Process As clsProcess Implements ILogContext.Process

    ''' <summary>
    ''' The ID of the master process which was started in this session
    ''' </summary>
    Public ReadOnly Property ProcessId As Guid Implements ILogContext.ProcessId

    ''' <summary>
    ''' The name of the master process which was started in this session
    ''' </summary>
    Public ReadOnly Property ProcessName As String Implements ILogContext.ProcessName

    ''' <summary>
    ''' The version of the master process which started in this session
    ''' </summary>
    Public ReadOnly Property ProcessVersion As String _
     Implements ILogContext.ProcessVersion

    ''' <summary>
    ''' The machine on which the session is running
    ''' </summary>
    Public ReadOnly Property ResourceId As Guid Implements ILogContext.ResourceId

    ''' <summary>
    ''' Gets the period between attempts to write a log entry.
    ''' </summary>
    Public ReadOnly Property RetrySeconds As Integer _
     Implements ILogContext.RetrySeconds

    ''' <summary>
    ''' Indicates whether screenshots may be logged
    ''' </summary>
    Public ReadOnly Property ScreenshotAllowed As Boolean _
     Implements ILogContext.ScreenshotAllowed

    ''' <summary>
    ''' The ID of the session log that this object provides context for
    ''' </summary>
    Public ReadOnly Property SessionId As Guid Implements ILogContext.SessionId

    ''' <summary>
    ''' The integer session number of the session log that this object provides
    ''' context for
    ''' </summary>
    Public ReadOnly Property SessionNo As Integer Implements ILogContext.SessionNo

    ''' <summary>
    ''' The start username of the session log that this object contextualises, may
    ''' not correlate with a Blue Prism username (ie. it's a free text field).
    ''' </summary>
    Public ReadOnly Property UserName As String Implements ILogContext.UserName

    ''' <summary>
    ''' The name of the resource determined from its resourceId
    ''' </summary>
    Public ReadOnly Property ResourceName As String _
     Implements ILogContext.ResourceName

#End Region

End Class
