Public Interface ILogContext
    Inherits IProcessLogContext

    ''' <summary>
    ''' The start username of the session log that this object contextualises, may
    ''' not correlate with a Blue Prism username (ie. it's a free text field).
    ''' </summary>
    ReadOnly Property UserName As String

    ''' <summary>
    ''' The machine on which the session is running
    ''' </summary>
    ReadOnly Property ResourceId As Guid

    ''' <summary>
    ''' Indicates whether screenshots may be logged
    ''' </summary>
    ReadOnly Property ScreenshotAllowed As Boolean

    ''' <summary>
    ''' Indicates whether the context is a debugging context
    ''' </summary>
    ReadOnly Property Debugging As Boolean

    ''' <summary>
    ''' The name of the resource determined from its resourceId
    ''' </summary>
    ReadOnly Property ResourceName As String

    ''' <summary>
    ''' The ID of the master process which was started in this session
    ''' </summary>
    ReadOnly Property ProcessId As Guid

    ''' <summary>
    ''' The name of the master process which was started in this session
    ''' </summary>
    ReadOnly Property ProcessName As String

    ''' <summary>
    ''' The version of the master process which started in this session
    ''' </summary>
    ReadOnly Property ProcessVersion As String

    ''' <summary>
    ''' The ID of the session log that this object provides context for
    ''' </summary>
    ReadOnly Property SessionId As Guid

    ''' <summary>
    ''' The integer session number of the session log that this object provides
    ''' context for
    ''' </summary>
    ReadOnly Property SessionNo As Integer

    ''' <summary>
    ''' Gets a flag indicating that the session should fail if any error
    ''' occurs while logging, after a defined number of attempts.
    ''' </summary>
    ''' <remarks>This defaults to True in a standard log context instance</remarks>
    ReadOnly Property FailOnError As Boolean

    ''' <summary>
    ''' Gets the predefined number of attempts which should be made to write
    ''' a log entry before either failing or continuing, as decided by the
    ''' <see cref="ILogContext.FailOnError"/> property value.
    ''' </summary>
    ReadOnly Property Attempts As Integer

    ''' <summary>
    ''' Gets the period between attempts to write a log entry.
    ''' </summary>
    ReadOnly Property RetrySeconds As Integer

End Interface