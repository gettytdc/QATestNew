Imports BluePrism.AutomateAppCore.Auth

''' Project : AutomateAppCore
''' <summary>
''' Class to capture data for an audit event occurring within the schedule handling.
''' </summary>
<Serializable()> _
Public Class ScheduleAuditEvent

#Region "Member variables"

    ' The user ID
    Private mUser As IUser

    ' The code indicating the type of audit event
    Private mCode As ScheduleEventCode

    ' The ID of the schedule
    Private mSchedId As Integer

    ' The ID of the affected task or zero if no task is affected
    Private mTaskId As Integer

    ' The ID of the affected session or zero if no session is affected
    Private mSession As ScheduledSession

    ' The comment
    Private mComment As String

#End Region

#Region "Constructors"

#Region "Currently Logged In User"

    ''' <summary>
    ''' Creates a new audit event which only refers to a schedule, using the
    ''' currently logged in user.
    ''' </summary>
    ''' <param name="code">The code indicating the type of audit event to create
    ''' </param>
    ''' <param name="schedId">The ID of the schedule affected.</param>
    Public Sub New(ByVal code As ScheduleEventCode, ByVal schedId As Integer)
        Me.New(code, User.Current, schedId)
    End Sub

    ''' <summary>
    ''' Creates a new audit event which refers only to a schedule and a task and has
    ''' no session or comment component, using the currently logged in user.
    ''' </summary>
    ''' <param name="code">The code indicating the type of audit event to create
    ''' </param>
    ''' <param name="schedId">The ID of the schedule affected.</param>
    ''' <param name="taskId">The ID of the affected task</param>
    Public Sub New(ByVal code As ScheduleEventCode, _
     ByVal schedId As Integer, ByVal taskId As Integer)
        Me.New(code, User.Current, schedId, taskId)
    End Sub

    ''' <summary>
    ''' Creates a new audit event with the specified parameters, setting the user as
    ''' the currently logged in user.
    ''' </summary>
    ''' <param name="code">The audit code</param>
    ''' <param name="schedId">The schedule ID</param>
    ''' <param name="taskId">The task ID, or zero</param>
    ''' <param name="session">The session or Nothing</param>
    ''' <param name="comment">The comment</param>
    Public Sub New(ByVal code As ScheduleEventCode, ByVal schedId As Integer, _
     ByVal taskId As Integer, ByVal session As ScheduledSession, ByVal comment As String)

        Me.New(code, User.Current, schedId, taskId, session, comment)

    End Sub

#End Region

#Region "Specified User"

    ''' <summary>
    ''' Creates a new audit event which only refers to a schedule.
    ''' </summary>
    ''' <param name="code">The code indicating the type of audit event to create
    ''' </param>
    ''' <param name="schedId">The ID of the schedule affected.</param>
    Public Sub New(ByVal code As ScheduleEventCode, _
     ByVal user As IUser, ByVal schedId As Integer)
        Me.New(code, user, schedId, 0, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new audit event which refers only to a schedule and a task and has
    ''' no session or comment component
    ''' </summary>
    ''' <param name="code">The code indicating the type of audit event to create
    ''' </param>
    ''' <param name="schedId">The ID of the schedule affected.</param>
    ''' <param name="taskId">The ID of the affected task</param>
    Public Sub New(ByVal code As ScheduleEventCode, _
     ByVal user As IUser, ByVal schedId As Integer, ByVal taskId As Integer)
        Me.New(code, user, schedId, taskId, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new audit event with the specified parameters.
    ''' </summary>
    ''' <param name="code">The audit code</param>
    ''' <param name="schedId">The schedule ID</param>
    ''' <param name="taskId">The task ID, or zero</param>
    ''' <param name="session">The session or Nothing</param>
    ''' <param name="comment">The comment</param>
    Public Sub New(ByVal code As ScheduleEventCode, ByVal user As IUser, _
     ByVal schedId As Integer, ByVal taskId As Integer, ByVal session As ScheduledSession, _
     ByVal comment As String)

        mCode = code
        mSchedId = schedId
        mTaskId = taskId
        mSession = session
        mComment = comment
        mUser = user

    End Sub

#End Region

#End Region

#Region "Properties"

    ''' <summary>
    ''' The ID of the user who initiated this audit record.
    ''' </summary>
    Public ReadOnly Property UserId() As Guid
        Get
            Return mUser.Id
        End Get
    End Property

    ''' <summary>
    ''' The event code of this audit record.
    ''' </summary>
    Public ReadOnly Property Code() As ScheduleEventCode
        Get
            Return mCode
        End Get
    End Property

    ''' <summary>
    ''' The ID of the schedule which this record refers to
    ''' </summary>
    Public ReadOnly Property ScheduleId() As Integer
        Get
            Return mSchedId
        End Get
    End Property

    ''' <summary>
    ''' The ID of the task which this record refers to.
    ''' This is the only property of an audit event which is mutable - the ID
    ''' can change when a task is added to the database.
    ''' </summary>
    Public Property TaskId() As Integer
        Get
            Return mTaskId
        End Get
        Set(ByVal value As Integer)
            mTaskId = value
        End Set
    End Property

    ''' <summary>
    ''' The ID of the scheduled session which this record refers to
    ''' </summary>
    Public ReadOnly Property ScheduledSession() As ScheduledSession
        Get
            Return mSession
        End Get
    End Property

    ''' <summary>
    ''' The ID of the resource to which this record refers, or Guid.Empty if it
    ''' does not refer to a session.
    ''' </summary>
    Public ReadOnly Property ResourceId() As Guid
        Get
            If mSession Is Nothing Then Return Guid.Empty Else Return mSession.ResourceId
        End Get
    End Property

    ''' <summary>
    ''' The ID of the process to which this record refers, or Guid.Empty if it
    ''' does not refer to a session.
    ''' </summary>
    Public ReadOnly Property ProcessId() As Guid
        Get
            If mSession Is Nothing Then Return Guid.Empty Else Return mSession.ProcessId
        End Get
    End Property

    ''' <summary>
    ''' The comment within this audit record.
    ''' </summary>
    Public ReadOnly Property Comment() As String
        Get
            Return mComment
        End Get
    End Property

#End Region

End Class
