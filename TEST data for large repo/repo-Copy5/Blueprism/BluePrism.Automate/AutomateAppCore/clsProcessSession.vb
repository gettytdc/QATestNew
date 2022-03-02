
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models
Imports BluePrism.AutomateAppCore.Resources

''' Project  : Automate
''' Class    : clsProcessSession
'''
''' <summary>
''' A class for holding information about sessions.
''' </summary>
<Serializable, DataContract([Namespace]:="bp", Name:="ps"), KnownType(GetType(MemberPermissions))>
Public Class clsProcessSession : Implements IComparable, ISession

#Region " Constants, Enums, Inner Classes "

    ''' <summary>
    ''' Connection states which indicate that a resource is still connected to.
    ''' </summary>
    Private Shared ReadOnly ConnectedStates As ICollection(Of ResourceConnectionState) =
        {ResourceConnectionState.Connected, ResourceConnectionState.InUse, ResourceConnectionState.Server}

    ''' <summary>
    ''' The default XML to use for the arguments for this process.
    ''' This will probably never come up, because the database always holds an args
    ''' value even if the session had no args defined for it (an XML representation
    ''' of an empty argument list).
    ''' </summary>
    Private Const DefaultArgsXml As String = "<inputs />"

#End Region

#Region " Member Variables "

    ' The ID of the session
    <DataMember(Name:="sid", EmitDefaultValue:=False)>
    Private mSessionID As Guid

    ' The number of the session
    <DataMember(Name:="sn")>
    Private mSessionNum As Integer

    ' The current status of the session
    <DataMember(Name:="st")>
    Private mStatus As SessionStatus

    ' The session start time
    <DataMember(Name:="ss")>
    Private mSessionStart As DateTimeOffset

    ' The session end time
    <DataMember(Name:="se")>
    Private mSessionEnd As DateTimeOffset

    ' The arguments passed into the session on start
    <DataMember(Name:="a")>
    Private mArguments As clsArgumentList

    ' The ID of the process
    <DataMember(Name:="pid", EmitDefaultValue:=False)>
    Private mProcessID As Guid

    ' The name of the process
    <DataMember(Name:="pn")>
    Private mProcessName As String

    ' The cached process which this session ran / will run
    <NonSerialized>
    Private mProcess As clsProcess

    ' The user who started (created?) the session
    <DataMember(Name:="un")>
    Private mUserName As String

    ' The resource on which the session has run / is due to run
    <DataMember(Name:="rn")>
    Private mResourceName As String

    ' The ident of the active queue for which this session was created
    <DataMember(Name:="qid")>
    Private mQueueId As Integer

    ''' <summary>
    ''' The date that the last stage started
    ''' </summary>
    <DataMember(Name:="lu")>
    Public Property LastUpdated As DateTimeOffset

    ''' <summary>
    ''' The name of the last stage that ran
    ''' </summary>
    <DataMember(Name:="ls")>
    Public Property LastStage As String

    ''' <summary>
    ''' The permissions on this process for the current user
    ''' </summary>
    <DataMember(Name:="pp")>
    Public Property ProcessPermissions As IMemberPermissions

    ''' <summary>
    ''' The permissions on this resource for the current user
    ''' </summary>
    <DataMember(Name:="rp")>
    Public Property ResourcePermissions As IMemberPermissions

    ''' <summary>
    ''' The session termination reason of the terminated session
    ''' </summary>
    <DataMember(Name:="str")>
    Public Property SessionTerminationReason As SessionTerminationReason

    ''' <summary>
    ''' The exception type of the terminated session
    ''' </summary>
    <DataMember(Name:="et")>
    Public Property ExceptionType As String

    ''' <summary>
    ''' The exception message of the terminated session
    ''' </summary>
    <DataMember(Name:="em")>
    Public Property ExceptionMessage As String

#End Region

#Region " Auto-Properties "

    ''' <summary>
    ''' This holds the ID of the pc on which the process is to run on.
    ''' </summary>
    <DataMember(Name:="rid", EmitDefaultValue:=False)>
    Public Property ResourceID() As Guid

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty session object
    ''' </summary>
    Public Sub New()
        Me.New(NullDataProvider.Instance)
    End Sub

    ''' <summary>
    ''' Creates a new session object, drawing its value from the given data provider
    ''' </summary>
    ''' <param name="prov">The data provider containing the data to initialise this
    ''' session object from.</param>
    ''' <remarks>
    ''' The provider is expected to contain the following data, convertible to the
    ''' specified types:
    ''' <list>
    ''' <item>sessionnumber: Integer</item>
    ''' <item>sessionid: Guid</item>
    ''' <item>processid: Guid</item>
    ''' <item>statusid: <see cref="SessionStatus"/></item>
    ''' <item>processname: String</item>
    ''' <item>starterusername: String</item>
    ''' <item>runningresourcename: String</item>
    ''' <item>runningresourceid: Guid</item>
    ''' <item>startdatetime: DateTime</item>
    ''' <item>enddatetime: DateTime</item>
    ''' <item>startparamsxml: String</item>
    ''' <item>queueid: Integer</item>
    ''' <item>laststage: String</item>
    ''' <item>lastupdated: DateTime</item>
    ''' <item>warningthreshold: Integer</item>
    ''' </list>
    ''' </remarks>
    Public Sub New(prov As IDataProvider)
        SessionNum = prov.GetValue("sessionnumber", 0)
        SessionID = prov.GetGuid("sessionid")
        ProcessID = prov.GetGuid("processid")
        Status = prov.GetValue("statusid", Server.Domain.Models.SessionStatus.All)
        ProcessName = prov.GetString("processname")
        UserName = prov.GetString("starterusername")
        ResourceName = prov.GetString("runningresourcename")
        ResourceID = prov.GetGuid("runningresourceid")
        SessionStart = BPUtil.ConvertDateTimeOffset(prov, "startdatetime", "starttimezoneoffset")
        SessionEnd = BPUtil.ConvertDateTimeOffset(prov, "enddatetime", "endtimezoneoffset")
        ArgumentsXml = prov.GetString("startparamsxml")
        mQueueId = prov.GetValue("queueid", 0)
        LastStage = prov.GetValue("laststage", String.Empty)
        LastUpdated = BPUtil.ConvertDateTimeOffset(prov, "lastupdated", "lastupdatedtimezoneoffset")
        SessionTerminationReason = prov.GetValue("terminationreason", SessionTerminationReason.None)
        ExceptionType = prov.GetString("exceptiontype")
        ExceptionMessage = prov.GetString("exceptionmessage")
        If CheckStalled(prov.GetInt("warningThreshold")) Then Status = Server.Domain.Models.SessionStatus.Stalled
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' This holds the unique identifier for the session
    ''' </summary>
    Public Property SessionID() As Guid
        Get
            Return mSessionID
        End Get
        Set(ByVal value As Guid)
            mSessionID = value
        End Set
    End Property

    ''' <summary>
    ''' This holds the unique session number for the session
    ''' </summary>
    Public Property SessionNum() As Integer
        Get
            Return mSessionNum
        End Get
        Set(ByVal value As Integer)
            mSessionNum = value
        End Set
    End Property

    ''' <summary>
    ''' The start datetime/offset of this session. <see cref="DateTime.MinValue"/> if this
    ''' session has not yet been run.
    ''' </summary>
    Public Property SessionStart() As DateTimeOffset
        Get
            Return mSessionStart
        End Get
        Set(ByVal value As DateTimeOffset)
            mSessionStart = value
        End Set
    End Property

    ''' <summary>
    ''' The end datetime/offset of this session. <see cref="DateTime.MaxValue"/> if this
    ''' session has not finished.
    ''' </summary>
    Public Property SessionEnd() As DateTimeOffset
        Get
            ' Normalise the date such that if it's empty, it returns MaxValue - ie.
            ' the latest date ever... 'no end date' effectively.
            If mSessionEnd = DateTimeOffset.MinValue Then Return DateTimeOffset.MaxValue
            Return mSessionEnd
        End Get
        Set(ByVal value As DateTimeOffset)
            mSessionEnd = value
        End Set
    End Property

    ''' <summary>
    ''' The start date/time of this session converted to local time and without
    ''' the offset specifier, or an empty string if this session has not yet
    ''' started.
    ''' </summary>
    Public ReadOnly Property SessionStartText() As String
        Get
            If mSessionStart = DateTimeOffset.MinValue Then Return String.Empty
            Return mSessionStart.ToLocalTime.DateTime.ToString()
        End Get
    End Property

    ''' <summary>
    ''' The end date/time of this session converted to local time and without the
    ''' offset specifier, or an empty string if this session has not yet finished.
    ''' </summary>
    Public ReadOnly Property SessionEndText() As String
        Get
            If mSessionEnd = DateTimeOffset.MaxValue OrElse
                mSessionEnd = DateTimeOffset.MinValue Then Return String.Empty
            Return mSessionEnd.ToLocalTime.DateTime.ToString()
        End Get
    End Property

    ''' <summary>
    ''' The status of the process and can be any of the values in the session status
    ''' enumeration. Combinations of two statuses are not valid.
    ''' </summary>
    Public Property Status As Server.Domain.Models.SessionStatus
        Get
            Return mStatus
        End Get
        Set(value As Server.Domain.Models.SessionStatus)
            mStatus = value
        End Set
    End Property


    ''' <summary>
    ''' The status of this session in text form
    ''' </summary>
    Public ReadOnly Property StatusText() As String
        Get
            Return If(BPUtil.GetAttributeValue(Of DescriptionAttribute)(Status)?.Description, Status.ToString())
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the session represented by this object is 'active' or not.
    ''' A session is considered active if it is pending or running - ie. if its
    ''' status is one of:- <list>
    ''' <item><see cref="SessionStatus.Pending"/> or </item>
    ''' <item><see cref="SessionStatus.Running"/> or </item>
    ''' <item><see cref="SessionStatus.StopRequested"/></item>
    ''' </list>
    ''' </summary>
    Public ReadOnly Property IsActive As Boolean
        Get
            Return (IsRunning OrElse mStatus = SessionStatus.Pending)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the session represented by this object is active and not due to
    ''' stop when a stop request is acted upon. Basically, checks if its status is
    ''' one of:- <list>
    ''' <item><see cref="SessionStatus.Pending"/> or </item>
    ''' <item><see cref="SessionStatus.Running"/></item>
    ''' </list>
    ''' </summary>
    Public ReadOnly Property IsActiveNotStopping As Boolean
        Get
            Return (IsActive AndAlso Not IsStopRequested)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the session represented by this object is running or not. A
    ''' session is considered running when its status is one of: <list>
    ''' <item><see cref="SessionStatus.Running"/> or</item>
    ''' <item><see cref="SessionStatus.StopRequested"/></item>
    ''' </list>
    ''' Note that <see cref="SessionStatus.Debugging"/> is not counted as a 'running'
    ''' session as far as this property is concerned.
    ''' </summary>
    Public ReadOnly Property IsRunning As Boolean
        Get
            Return (
                mStatus = SessionStatus.Running OrElse
                mStatus = SessionStatus.StopRequested
            )
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the session represented by this object is running or being
    ''' debugged in Studio.
    ''' </summary>
    Public ReadOnly Property IsRunningOrDebugging As Boolean
        Get
            Return (IsRunning OrElse mStatus = SessionStatus.Debugging)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the session represented by this object has a stop requested
    ''' status or not. A session is considered to have a stop request when its status
    ''' is <see cref="SessionStatus.StopRequested"/>
    ''' </summary>
    Public ReadOnly Property IsStopRequested As Boolean
        Get
            Return (mStatus = SessionStatus.StopRequested)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the session represented by this object is finished or not. A
    ''' session is considered finished when its status is one of: <list>
    ''' <item><see cref="SessionStatus.Completed"/> or </item>
    ''' <item><see cref="SessionStatus.Failed"/> or </item>
    ''' <item><see cref="SessionStatus.Archived"/> or </item>
    ''' <item><see cref="SessionStatus.Stopped"/></item>
    ''' </list>
    ''' </summary>
    Public ReadOnly Property IsFinished As Boolean
        Get
            Return (
                mStatus = SessionStatus.Completed OrElse
                mStatus = SessionStatus.Failed OrElse
                mStatus = SessionStatus.Archived OrElse
                mStatus = SessionStatus.Stopped
            )
        End Get
    End Property

    ''' <summary>
    ''' This holds a the arguments which are passed to the process when this session
    ''' is run.
    ''' </summary>
    ''' <remarks>
    ''' This will never be null, although the argument list may contain no arguments.
    ''' <seealso cref="HasArguments"/>
    ''' </remarks>
    Public Property Arguments() As clsArgumentList Implements ISession.Arguments
        Get
            If mArguments Is Nothing Then mArguments = New clsArgumentList()
            Return mArguments
        End Get
        Set(ByVal value As clsArgumentList)
            mArguments = value
        End Set
    End Property

    ''' <summary>
    ''' The XML definining the arguments passed to the process when the session
    ''' is/was run.
    ''' </summary>
    Public Property ArgumentsXml() As String
        Get
            If Not HasArguments Then Return DefaultArgsXml
            Return mArguments.ArgumentsToXML(False)
        End Get
        Set(ByVal value As String)
            If value = "" Then
                mArguments = Nothing
            Else
                mArguments = clsArgumentList.XMLToArguments(value, False)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Checks if this session has any arguments defined on it.
    ''' True if this session has an argument list with arguments defined on it;
    ''' false otherwise.
    ''' </summary>
    Public ReadOnly Property HasArguments() As Boolean
        Get
            Return (mArguments IsNot Nothing AndAlso mArguments.Count > 0)
        End Get
    End Property

    ''' <summary>
    ''' Gets the identity of the active queue on whose behalf this session was
    ''' created.
    ''' </summary>
    Public ReadOnly Property QueueId As Integer
        Get
            Return mQueueId
        End Get
    End Property

    ''' <summary>
    ''' This holds the unique identifier for the process.
    ''' </summary>
    Public Property ProcessID() As Guid Implements ISession.ProcessID
        Get
            Return mProcessID
        End Get
        Set(ByVal value As Guid)
            mProcessID = value
        End Set
    End Property

    ''' <summary>
    ''' The name of the process which was / is to be run in this session.
    ''' </summary>
    Public Property ProcessName() As String
        Get
            Return mProcessName
        End Get
        Set(ByVal value As String)
            mProcessName = value
        End Set
    End Property

    ''' <summary>
    ''' This holds an object representation of the process
    ''' </summary>
    Public Property Process() As clsProcess
        Get
            Return mProcess
        End Get
        Set(ByVal value As clsProcess)
            mProcess = value
        End Set
    End Property

    ''' <summary>
    '''  The name of the user who initiated / created this session
    ''' </summary>
    Public Property UserName() As String
        Get
            Return mUserName
        End Get
        Set(ByVal value As String)
            mUserName = value
        End Set
    End Property


    ''' <summary>
    ''' This holds the name of the pc on which the process is to run on.
    ''' </summary>
    Public Property ResourceName() As String Implements ISession.ResourceName
        Get
            Return mResourceName
        End Get
        Set(ByVal value As String)
            mResourceName = value
        End Set
    End Property

    ''' <summary>
    ''' Checks to see if the process has stalled i.e
    ''' it hasn't been updated in 1 minute.
    ''' <param name="warningThreshold">The number of seconds since the last stage
    ''' change at which we should mark the session as a warning</param>
    ''' </summary>
    Private Function CheckStalled(warningThreshold As Integer) As Boolean
        'Do the check in universal time just for clarity.
        Dim updated = LastUpdated.ToUniversalTime

        ' Check it's valid to apply this warning.
        If updated = DateTimeOffset.MinValue OrElse warningThreshold = 0 _
            OrElse Not IsRunning Then Return False
        Return updated.AddSeconds(warningThreshold) < DateTimeOffset.UtcNow
    End Function

    ''' <summary>
    ''' The last updated date as formatted for the user interface.
    ''' </summary>
    Public ReadOnly Property LastUpdatedText As String
        Get
            If IsFinished Then Return String.Empty
            Dim updated = LastUpdated
            Return If(updated = DateTimeOffset.MinValue, String.Empty, updated.LocalDateTime.ToString())
        End Get
    End Property

    ''' <summary>
    ''' The last stage as formatted for the user interface.
    ''' </summary>
    Public ReadOnly Property LastStageText As String
        Get
            If IsFinished Then Return String.Empty
            Return LastStage
        End Get
    End Property

#End Region

#Region " ProcessSession-specific methods "

    ''' <summary>
    ''' Gets updated status text, checking, if the session is running, to see if
    ''' the resource for this session is still connected to.
    ''' </summary>
    ''' <param name="mgr">The resource connection manager to use to talk to the
    ''' resource. If null, this will just use the current status text without
    ''' attempting to contact the resource.</param>
    ''' <returns>The status text, ensuring that the resource is reachable if the
    ''' session is running.</returns>
    Public Function GetUpdatedStatusText(ByVal mgr As IResourceConnectionManager) As String

        'Bug 3423 - when a resource pc cannot be contacted, don't continue
        'to show as running, but give some 'not sure' indication instead
        If Not IsRunning OrElse mgr Is Nothing Then Return StatusText

        Dim rm As IResourceMachine = mgr.GetResource(ResourceName)
        ' If null, either we don't know about this Resource at all, which should
        ' be impossible, else it's a pool member.
        If rm Is Nothing AndAlso mgr.GetControllingResource(ResourceName) Is Nothing Then
            Return My.Resources.clsProcessSession_Unknown

        ElseIf rm IsNot Nothing AndAlso Not ConnectedStates.Contains(rm.ConnectionState) Then
            ' If we know about it, but are not connected to it, well,
            ' that's a "not sure" indication if ever I did see one..
            Return My.Resources.clsProcessSession_NotResponding

        End If

        Return StatusText

    End Function

    ''' <summary>
    ''' Get the ID of the Resource we should talk to about this session - e.g. if we
    ''' want to start or stop processes on it. Normally this is the Resource that's
    ''' running it, but if that Resource is a member of a Pool we need to talk to the
    ''' controller instead, but we return the ID of the Pool itself.
    ''' </summary>
    ''' <param name="resid">On successful return, contains the Resource ID.</param>
    ''' <param name="sErr">On failure, contains an error description.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function GetTargetResourceID(ByRef resid As Guid, ByRef sErr As String) As Boolean

        resid = gSv.GetResourceId(ResourceName)
        'If this resource is in a pool, we have to talk to the controller...
        Dim poolid As Guid, controllerid As Guid

        Try
            gSv.GetResourcePoolInfo(resid, poolid, controllerid)
        Catch ex As Exception
            Return False
        End Try

        If Not poolid.Equals(Guid.Empty) Then
            If controllerid.Equals(Guid.Empty) Then
                sErr = My.Resources.clsProcessSession_TheControllerOfThatResourcePoolIsCurrentlyOffline
                Return False
            End If
            resid = poolid
        End If
        Return True

    End Function

    ''' <summary>
    ''' The Parameters within the process for which arguments need to be set.
    ''' </summary>
    Public Function GetStartParameters() As List(Of clsProcessParameter) Implements ISession.GetStartParameters
        Dim startStageID As Guid = Process.GetStartStage()
        If startStageID.Equals(Guid.Empty) Then Return Nothing

        Dim startStage As clsProcessStage = Process.GetStage(startStageID)
        If startStage Is Nothing Then Return Nothing

        Return startStage.GetParameters()
    End Function

#End Region

#Region " Compare / Clone / Equals overrides and implementations "

    ''' <summary>
    ''' This implementation of the CompareTo function allows two sessions to be
    ''' compared for sorting purposes. The sessions are sorted by starttime, and then
    ''' endtime.
    ''' </summary>
    ''' <param name="obj">The clsProcessSession to compare this one to</param>
    ''' <returns>1, 0, or -1 if this session is 'greater than', 'equal to' or 'less
    ''' than' the given session, according to starttime, then endtime, then ID.
    ''' </returns>
    Public Function CompareTo(ByVal obj As Object) As Integer _
     Implements IComparable.CompareTo

        If Not TypeOf obj Is clsProcessSession Then Throw New ArgumentException(
         "Object is not a ProcessSession")

        Dim sess As clsProcessSession = CType(obj, clsProcessSession)
        Dim i As Integer = SessionStart.CompareTo(sess.SessionStart)

        If i <> 0 Then Return i
        i = SessionEnd.CompareTo(sess.SessionEnd)

        If i <> 0 Then Return i
        Return SessionID.CompareTo(sess.SessionID)

    End Function

    ''' <summary>
    ''' Creates a (mostly) deep clone of the session. Note that any process
    ''' cached in this session is not cloned.
    ''' </summary>
    ''' <returns>A clone of the session</returns>
    Private Function CloneObject() As Object Implements ICloneable.Clone
        Return Clone()
    End Function

    ''' <summary>
    ''' Creates a (mostly) deep clone of the session. Note that any process
    ''' cached in this session is not cloned.
    ''' </summary>
    ''' <returns>A clone of the session</returns>
    Public Function Clone() As clsProcessSession
        Dim sess As clsProcessSession = TryCast(MemberwiseClone(), clsProcessSession)
        If mArguments IsNot Nothing Then sess.mArguments = mArguments.Clone()
        Return sess
    End Function

    ''' <summary>
    ''' Checks if the given ISession is equal to this session
    ''' </summary>
    ''' <param name="other">The ISession to check to see if it is equal to
    ''' this session or not.</param>
    ''' <returns>True if the given ISession is a clsProcessSession with the
    ''' same data as this session; False otherwise</returns>
    Public Overloads Function Equals(other As ISession) As Boolean _
     Implements IEquatable(Of ISession).Equals
        Return Equals(TryCast(other, clsProcessSession))
    End Function

    ''' <summary>
    ''' Checks if the given object is equal to this session.
    ''' </summary>
    ''' <param name="o">The object to check to see if it is equal to
    ''' this session or not.</param>
    ''' <returns>True if the given object is a clsProcessSession with the
    ''' same data as this session; False otherwise</returns>
    Public Overrides Function Equals(o As Object) As Boolean
        Return Equals(TryCast(o, clsProcessSession))
    End Function


    ''' <summary>
    ''' Checks if the given session is equal to this session.
    ''' </summary>
    ''' <param name="sess">The session to check to see if it is equal to
    ''' this session or not.</param>
    ''' <returns>True if the given clsProcessSession is not nothing and has the
    ''' same data as this session; False otherwise</returns>
    Public Overloads Function Equals(sess As clsProcessSession) As Boolean
        Return (sess IsNot Nothing _
         AndAlso sess.SessionID = Me.SessionID _
         AndAlso sess.SessionStart = Me.SessionStart _
         AndAlso sess.SessionEnd = Me.SessionEnd _
         AndAlso sess.Status = Me.Status _
         AndAlso sess.ProcessName = Me.ProcessName _
         AndAlso sess.ResourceName = Me.ResourceName _
         AndAlso sess.LastStage = Me.LastStage _
         AndAlso sess.LastUpdated = Me.LastUpdated)
    End Function

#End Region

End Class
