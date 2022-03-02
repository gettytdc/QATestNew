Imports BluePrism.AutomateProcessCore

''' <summary>
''' Class to encapsulate the context of an instance of the logging engine
''' </summary>
<Serializable()> _
Public Class LogContext
    Implements ILogContext

#Region " Constants "

    ''' <summary>
    ''' The default period between retries if a logging operation fails
    ''' </summary>
    Public Const DefaultRetrySeconds As Integer = 5

    ''' <summary>
    ''' The default number of attempts to log an entry
    ''' </summary>
    Public Const DefaultAttempts As Integer = 5

#End Region

#Region " Member Variables "

    ' The master process of the session
    Private mProcess As clsProcess

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new log context with the given attributes
    ''' </summary>
    ''' <param name="username">The username (OS or BluePrism or a combination) of the
    ''' starting user</param>
    ''' <param name="resourceId">The machine on which the session is running</param>
    ''' <param name="proc">The process to be logged</param>
    ''' <param name="debugging">Whether the process is being debugged</param>
    ''' <param name="sessionId">The ID of the session</param>
    ''' <param name="sessionNo">The integer number of the session</param>
    Public Sub New(username As String, resourceId As Guid, proc As clsProcess,
                   debugging As Boolean, sessionId As Guid, sessionNo As Integer)
        Me.UserName = username
        Me.ResourceId = resourceId
        Me.ResourceName = gSv.GetResourceName(resourceId)
        Me.ScreenshotAllowed = gSv.GetAllowResourceScreenshot()
        Me.Debugging = debugging
        mProcess = proc
        Me.SessionId = sessionId
        Me.SessionNo = sessionNo
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The start username of the session log that this object contextualises, may
    ''' not correlate with a Blue Prism username (ie. it's a free text field).
    ''' </summary>
    Public ReadOnly Property UserName() As String Implements ILogContext.UserName

    ''' <summary>
    ''' The machine on which the session is running
    ''' </summary>
    Public ReadOnly Property ResourceId() As Guid Implements ILogContext.ResourceId

    ''' <summary>
    ''' Indicates whether screenshots may be logged
    ''' </summary>
    Public ReadOnly Property ScreenshotAllowed As Boolean _
     Implements ILogContext.ScreenshotAllowed

    ''' <summary>
    ''' Indicates whether the context is a debugging context
    ''' </summary>
    Public ReadOnly Property Debugging As Boolean Implements ILogContext.Debugging

    ''' <summary>
    ''' The name of the resource determined from its resourceId
    ''' </summary>
    Public ReadOnly Property ResourceName As String _
     Implements ILogContext.ResourceName

    ''' <summary>
    ''' The master process for the session that the log that this context represents
    ''' was create for
    ''' </summary>
    Public ReadOnly Property Process() As clsProcess Implements ILogContext.Process
        Get
            Return mProcess
        End Get
    End Property

    ''' <summary>
    ''' The ID of the master process which was started in this session
    ''' </summary>
    Public ReadOnly Property ProcessId As Guid Implements ILogContext.ProcessId
        Get
            Return If(mProcess Is Nothing, Guid.Empty, mProcess.Id)
        End Get
    End Property

    ''' <summary>
    ''' The name of the master process which was started in this session
    ''' </summary>
    Public ReadOnly Property ProcessName() As String _
     Implements ILogContext.ProcessName
        Get
            If mProcess Is Nothing Then Return ""
            Return mProcess.Name
        End Get
    End Property

    ''' <summary>
    ''' The version of the master process which started in this session
    ''' </summary>
    Public ReadOnly Property ProcessVersion() As String _
     Implements ILogContext.ProcessVersion
        Get
            If mProcess Is Nothing Then Return ""
            Return mProcess.Version
        End Get
    End Property

    ''' <summary>
    ''' The ID of the session log that this object provides context for
    ''' </summary>
    Public ReadOnly Property SessionId() As Guid Implements ILogContext.SessionId

    ''' <summary>
    ''' The integer session number of the session log that this object provides
    ''' context for
    ''' </summary>
    Public ReadOnly Property SessionNo() As Integer Implements ILogContext.SessionNo

    ''' <summary>
    ''' Gets a flag indicating that the session should fail if any error
    ''' occurs while logging, after a defined number of attempts.
    ''' </summary>
    ''' <remarks>This defaults to True in a standard log context instance</remarks>
    Public ReadOnly Property FailOnError() As Boolean Implements ILogContext.FailOnError
        Get
            If mProcess Is Nothing Then Return False
            Return mProcess.AbortOnLogError
        End Get
    End Property

    ''' <summary>
    ''' Gets the predefined number of attempts which should be made to write
    ''' a log entry before either failing or continuing, as decided by the
    ''' <see cref="FailOnError"/> property value.
    ''' </summary>
    ''' <remarks>This defaults to <see cref="DefaultAttempts"/> in a standard log
    ''' context instance</remarks>
    Public ReadOnly Property Attempts() As Integer Implements ILogContext.Attempts
        Get
            If mProcess Is Nothing Then Return DefaultAttempts
            Return mProcess.LoggingAttempts
        End Get
    End Property

    ''' <summary>
    ''' Gets the period between attempts to write a log entry.
    ''' </summary>
    ''' <remarks>This defaults to <see cref="DefaultRetrySeconds"/> in a standard log
    ''' context instance</remarks>
    Public ReadOnly Property RetrySeconds() As Integer Implements ILogContext.RetrySeconds
        Get
            If mProcess Is Nothing Then Return DefaultRetrySeconds
            Return mProcess.LoggingRetryPeriod
        End Get
    End Property

#End Region

End Class
