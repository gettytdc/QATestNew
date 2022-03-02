Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization

''' Project  : AutomateAppCore
''' Class    : clsScheduleLogEntry
''' <summary>
''' Class to represent a single log entry in a schedule log.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp", Name:="sle")>
Public Class ScheduleLogEntry

#Region "Member variables"

    ' The date/time of this entry
    <DataMember(Name:="et")>
    Private mEntryTime As DateTime

    ' The type of entry
    <DataMember(Name:="ey")>
    Private mEntryType As ScheduleLogEventType

    ' The task ID - zero if not applicable
    <DataMember(Name:="ti", EmitDefaultValue:=False)>
    Private mTaskId As Integer

    ' The session log number - zero if not applicable
    <DataMember(Name:="sl", EmitDefaultValue:=False)>
    Private mSessionLogNo As Integer

    ' The reason for the termination stored in this entry
    <DataMember(Name:="tr", EmitDefaultValue:=False)>
    Private mTerminationReason As String

    ' The stack trace of the exception which caused this termination entry
    <NonSerialized>
    Private mStackTrace As String

#End Region

#Region "Constructors"

#Region "Live logs - ie. no date/time given on creation"

    ''' <summary>
    ''' Creates a new log entry <em>with no timestamp</em> which consists
    ''' of an entry type only, ie. no association with either a task or
    ''' a session.
    ''' </summary>
    ''' <param name="entryType">The type of log entry.</param>
    Public Sub New(ByVal entryType As ScheduleLogEventType)
        Me.New(entryType, Nothing, 0, 0, Nothing)
    End Sub


    ''' <summary>
    ''' Creates a new log entry <em>with no timestamp</em> which consists
    ''' of an entry type, task ID and session log number only, ie. no
    ''' termination information.
    ''' </summary>
    ''' <param name="entryType">The type of log entry</param>
    ''' <param name="taskid">The ID of the task associated with this
    ''' log entry, zero if none is associated with this entry.</param>
    ''' <param name="sessionLogNo">The session number of the session log
    ''' associated with this entry, zero if none is appropriate.</param>
    Public Sub New( _
     ByVal entryType As ScheduleLogEventType, _
     ByVal taskId As Integer, _
     ByVal sessionLogNo As Integer)
        Me.New(entryType, Nothing, taskId, sessionLogNo, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new log entry <em>with no timestamp</em> which consists
    ''' of an entry type and termination information - ie. this is not
    ''' associated with any task or session log.
    ''' </summary>
    ''' <param name="entryType">The type of log entry</param>
    ''' <param name="terminationReason">The reason for the termination
    ''' that this entry represents, null otherwise.</param>
    Public Sub New(
     ByVal entryType As ScheduleLogEventType,
     ByVal terminationReason As String)
        Me.New(entryType, Nothing, 0, 0, terminationReason)
    End Sub

    ''' <summary>
    ''' Creates a new log entry <em>with no timestamp</em> which consists
    ''' of an entry type, task ID and session log number along with the
    ''' specified termination information.
    ''' </summary>
    ''' <param name="entryType">The type of log entry</param>
    ''' <param name="taskid">The ID of the task associated with this
    ''' log entry, zero if none is associated with this entry.</param>
    ''' <param name="sessionLogNo">The session number of the session log
    ''' associated with this entry, zero if none is appropriate.</param>
    ''' <param name="terminationReason">The reason for the termination
    ''' that this entry represents, null otherwise.</param>
    Public Sub New(
     ByVal entryType As ScheduleLogEventType,
     ByVal taskId As Integer,
     ByVal sessionLogNo As Integer,
     ByVal terminationReason As String)
        Me.New(entryType, Nothing, taskId, sessionLogNo, terminationReason)
    End Sub

#End Region

#Region "Historical logs - ie. entry date/time given"

    ''' <summary>
    ''' Creates a new log entry from the given data provider.
    ''' </summary>
    ''' <param name="provider">The provider of the data. This is expected
    ''' to have the following columns and types :-
    ''' <list>
    ''' <item>entrytype : ScheduleLogEventType (int); </item>
    ''' <item>entrytime : DateTime; </item>
    ''' <item>taskid : Integer; </item>
    ''' <item>logsessionnumber : Integer; </item>
    ''' <item>terminationreason : String; </item>
    ''' <item>stacktrace : String</item>
    ''' </list>
    ''' </param>
    Friend Sub New(ByVal provider As IDataProvider)
        Me.New(
         provider.GetValue(Of ScheduleLogEventType)("entrytype", Nothing),
         provider.GetValue("entrytime", DateTime.MaxValue),
         provider.GetValue("taskid", 0),
         provider.GetValue("logsessionnumber", 0),
         provider.GetValue(Of String)("terminationreason", Nothing))
    End Sub

#End Region

#Region "Common constructor"

    ''' <summary>
    ''' Common constructor - ultimately called by all other constructors.
    ''' Creates a new log entry for the given type and time, which references
    ''' the given task ID and session log and provides the specified 
    ''' termination information.
    ''' </summary>
    ''' <param name="entryType">The type of log event</param>
    ''' <param name="entryTime">The time of the log event</param>
    ''' <param name="taskId">The ID of the task to which this event refers,
    ''' zero if it is not associated with a task</param>
    ''' <param name="sessionLogNo">The sessionnumber of the session log to
    ''' which this event refers, zero if it does not refer to a session log
    ''' </param>
    ''' <param name="terminationReason">The user-presentable reason for the
    ''' termination which this log entry represents, null if this log entry
    ''' does not represent a termination.</param>
    Private Sub New(entryType As ScheduleLogEventType, entryTime As Date, taskId As Integer, sessionLogNo As Integer, terminationReason As String)
        mEntryType = entryType
        mEntryTime = Date.SpecifyKind(entryTime, DateTimeKind.Utc)
        mTaskId = taskId
        mSessionLogNo = sessionLogNo
        mTerminationReason = terminationReason
    End Sub

#End Region

#End Region

#Region "Properties"

    ''' <summary>
    ''' The time of this log entry
    ''' </summary>
    Public Property EntryTime() As DateTime
        Get
            Return mEntryTime
        End Get
        Friend Set(ByVal value As DateTime)
            mEntryTime = value
        End Set
    End Property

    ''' <summary>
    ''' The type of entry this object represents.
    ''' </summary>
    Public ReadOnly Property EntryType() As ScheduleLogEventType
        Get
            Return mEntryType
        End Get
    End Property

    ''' <summary>
    ''' The ID of the task represented by this log entry.
    ''' </summary>
    Public ReadOnly Property TaskId() As Integer
        Get
            Return mTaskId
        End Get
    End Property

    ''' <summary>
    ''' The session log number linked to by this entry.
    ''' </summary>
    Public ReadOnly Property SessionLogNo() As Integer
        Get
            Return mSessionLogNo
        End Get
    End Property

    ''' <summary>
    ''' The user-presentable termination reason that this entry 
    ''' represents, or an empty string if no termination reason
    ''' is recorded in this entry.
    ''' </summary>
    Public ReadOnly Property TerminationReason() As String
        Get
            Return CStr(IIf(mTerminationReason Is Nothing, "", mTerminationReason))
        End Get
    End Property


    ''' <summary>
    ''' The stack trace detailing the exception which caused the
    ''' termination that this entry represents. An empty string
    ''' if no stack trace is recorded in this entry.
    ''' </summary>
    Public ReadOnly Property StackTrace() As String
        Get
            Return CStr(IIf(mStackTrace Is Nothing, "", mStackTrace))
        End Get
    End Property

#End Region

#Region "Functions"

    ''' <summary>
    ''' Checks if this entry has a task associated with it.
    ''' </summary>
    ''' <returns>True if this entry references a task, false otherwise.
    ''' </returns>
    Public Function HasTask() As Boolean
        Return mTaskId > 0
    End Function

    ''' <summary>
    ''' Checks if this entry has a session log associated with it.
    ''' </summary>
    ''' <returns>True if this entry references a session log, false
    ''' otherwise.</returns>
    Public Function HasSession() As Boolean
        Return mSessionLogNo > 0
    End Function

    ''' <summary>
    ''' Checks if this entry has a termination reason recorded on it.
    ''' </summary>
    ''' <returns>True if this entry has a termination reason, False
    ''' otherwise.</returns>
    Public Function HasTerminationReason() As Boolean
        Return Not String.IsNullOrEmpty(mTerminationReason)
    End Function

    ''' <summary>
    ''' Checks if this entry has a stack trace recorded on it.
    ''' </summary>
    ''' <returns>True if this entry has a stack trace on it;
    ''' False otherwise.</returns>
    Public Function HasStackTrace() As Boolean
        Return Not String.IsNullOrEmpty(mStackTrace)
    End Function

#End Region

#Region "Object overrides"

    ''' <summary>
    ''' Returns a string representation of this log entry.
    ''' </summary>
    ''' <returns>This log entry as a string</returns>
    Public Overrides Function ToString() As String
        Return String.Format("[{0}] : {1} - <{2}:{3}>", _
         IIf(mEntryTime = Nothing, "", mEntryTime.ToString("yyyy-MM-dd HH:mm:ss")), _
         mEntryType, mTaskId, mSessionLogNo)
    End Function

    ''' <summary>
    ''' Checks if this object is equal to the given object.
    ''' </summary>
    ''' <param name="obj">The object to check against this log entry.</param>
    ''' <returns>True if the given object is a clsScheduleLogEntry object with the
    ''' same data as this object.</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Return Equals(TryCast(obj, ScheduleLogEntry))
    End Function

    ''' <summary>
    ''' Checks if this object is equal to the given log entry.
    ''' </summary>
    ''' <param name="entry">The entry to check against this log entry.</param>
    ''' <returns>True if the given object is non null and contains the same data as
    ''' this object.</returns>
    Public Overloads Function Equals(ByVal entry As ScheduleLogEntry) As Boolean
        If entry IsNot Nothing Then
            Return Me.mEntryTime = entry.mEntryTime AndAlso _
             Me.mEntryType = entry.mEntryType AndAlso _
             Me.mTaskId = entry.mTaskId AndAlso _
             Me.mSessionLogNo = entry.mSessionLogNo AndAlso _
             Object.Equals(Me.mStackTrace, entry.mStackTrace) AndAlso _
             Object.Equals(Me.mTerminationReason, entry.mTerminationReason)
        End If
        Return False
    End Function

    ''' <summary>
    ''' Gets a hash value for this log entry.
    ''' </summary>
    ''' <returns>An integer containing a hash value for this object.</returns>
    Public Overrides Function GetHashCode() As Integer

        Dim stackTraceHashCode As Integer = 0
        If mStackTrace IsNot Nothing Then stackTraceHashCode = mStackTrace.GetHashCode()

        Dim terminationReasonHashCode As Integer = 0
        If mTerminationReason IsNot Nothing Then _
         terminationReasonHashCode = mTerminationReason.GetHashCode()

        Return mEntryTime.GetHashCode() Xor _
         mEntryType.GetHashCode() Xor _
         mTaskId.GetHashCode() Xor _
         mSessionLogNo.GetHashCode() Xor _
         stackTraceHashCode Xor _
         terminationReasonHashCode

    End Function

#End Region

End Class
