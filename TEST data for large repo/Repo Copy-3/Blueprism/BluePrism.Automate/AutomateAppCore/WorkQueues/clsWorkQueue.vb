Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.Groups
Imports System.Runtime.Serialization
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models


''' Project: AutomateAppCore
''' Class:   clsWorkQueue
''' <summary>
''' Class to represent a work queue with some basic summarised data regarding its
''' contents
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class clsWorkQueue

#Region " Class scope definitions "

    ''' <summary>
    ''' The maximum lengths allowed for properties of a work queue.
    ''' </summary>
    Public Class MaxLengths
        Public Const Name As Integer = 255
        Public Const KeyField As Integer = 255
    End Class




#End Region

#Region " Private members "

    ' The name of this queue
    <DataMember(Name:="n", EmitDefaultValue:=False)>
    Private mName As String

    ' The field within the queue data to use as the key for a case in this queue
    <DataMember(Name:="k", EmitDefaultValue:=False)>
    Private mKeyField As String

    ' The original ID of the encrypter used in this queue (ie. at last load time)
    <DataMember(Name:="o", EmitDefaultValue:=False)>
    Private mOrigEncryptID As Integer

    ' The active sessions operating on behalf of this queue
    <DataMember(Name:="s", EmitDefaultValue:=False)>
    Private mSessions As ICollection(Of clsProcessSession)

    ' The group store to use to load the resource group data from
    <NonSerialized>
    Private mGroupStore As IGroupStore

    <NonSerialized>
    Private mClock As ISystemClock = Nothing

#End Region

#Region " Auto-properties "

    ''' <summary>
    ''' The snapshot configuration ID of this queue.
    ''' </summary>
    <DataMember(Name:="sc", EmitDefaultValue:=False)>
    Public Property SnapshotConfigurationId As Integer

    ''' <summary>
    ''' The ID of this work queue.
    ''' </summary>
    <DataMember(Name:="i", EmitDefaultValue:=False)>
    Public Property Id As Guid

    ''' <summary>
    ''' The integer identity of this queue.
    ''' </summary>
    <DataMember(Name:="id", EmitDefaultValue:=False)>
    Public Property Ident As Integer

    ''' <summary>
    ''' The ID of the encryption key associated with this queue.
    ''' </summary>
    <DataMember(Name:="e", EmitDefaultValue:=False)>
    Public Property EncryptionKeyID As Integer

    ''' <summary>
    ''' True to indicate that this work queue is currently running; False to
    ''' indicate that it has been suspended.
    ''' </summary>
    <DataMember(Name:="ir", EmitDefaultValue:=False)>
    Public Property IsRunning As Boolean

    ''' <summary>
    ''' The ID of the process assigned to this queue, or <see cref="Guid.Empty"/> if
    ''' this queue has no assigned process (and is thus not an active queue).
    ''' </summary>
    <DataMember(Name:="pi", EmitDefaultValue:=False)>
    Public Property ProcessId As Guid

    ''' <summary>
    ''' Nam of the process assigned to this work queue.
    ''' </summary>
    ''' <returns></returns>
    <DataMember(Name:="pn", EmitDefaultValue:=False)>
    Public Property ProcessName As String


    ''' <summary>
    ''' The ID of the group assigned to this queue or <see cref="Guid.Empty"/> if
    ''' this queue has no assigned resource group.
    ''' </summary>
    <DataMember(Name:="r", EmitDefaultValue:=False)>
    Public Property ResourceGroupId As Guid

    ''' <summary>
    ''' Name of the resource group assigned to this queue.
    ''' </summary>
    ''' <returns></returns>
    <DataMember(Name:="rg", EmitDefaultValue:=False)>
    Public Property ResourceGroupName As String

    ''' <summary>
    ''' True if the process assigned to this queue is hidden from this user
    ''' </summary>
    ''' <returns></returns>
    <DataMember(Name:="ia", EmitDefaultValue:=False)>
    Public Property IsAssignedProcessHidden As Boolean

    ''' <summary>
    ''' True if the resource group assigned to this queue is hidden from this user
    ''' </summary>
    ''' <returns></returns>
    <DataMember(Name:="ig", EmitDefaultValue:=False)>
    Public Property IsResourceGroupHidden As Boolean


    ''' <summary>
    ''' The maximum number of retries of an item permitted by this queue.
    ''' </summary>
    <DataMember(Name:="m", EmitDefaultValue:=False)>
    Public Property MaxAttempts As Integer

    ''' <summary>
    ''' The total number of items (including each retry separately) in this queue.
    ''' </summary>
    <DataMember(Name:="t", EmitDefaultValue:=False)>
    Public Property TotalAttempts As Integer

    ''' <summary>
    ''' The total number of successfully completed items in this queue.
    ''' </summary>
    <DataMember(Name:="c", EmitDefaultValue:=False)>
    Public Property Completed As Integer

    ''' <summary>
    ''' The total number of pending items in this queue.
    ''' </summary>
    <DataMember(Name:="p", EmitDefaultValue:=False)>
    Public Property Pending As Integer

    ''' <summary>
    ''' The number of deferred (in the future) items in this queue
    ''' </summary>
    <DataMember(Name:="d", EmitDefaultValue:=False)>
    Public Property Deferred As Integer

    ''' <summary>
    ''' The total number of exceptioned items in this queue.
    ''' </summary>
    <DataMember(Name:="ex", EmitDefaultValue:=False)>
    Public Property Exceptioned As Integer

    ''' <summary>
    ''' The total amount of worktime for all items within this queue.
    ''' Note that this figure includes items which have been partially worked
    ''' and then deferred.
    ''' </summary>
    <DataMember(Name:="tw", EmitDefaultValue:=False)>
    Public Property TotalWorkTime As TimeSpan

    ''' <summary>
    ''' Average <em>worked</em> time for this queue.
    ''' Note that this <em>only</em> calculates the average work time based on the
    ''' work time for all finished items, not including partially worked attempts.
    ''' </summary>
    <DataMember(Name:="aw", EmitDefaultValue:=False)>
    Public Property AverageWorkedTime As TimeSpan

    ''' <summary>
    ''' The number of sessions to be targeted in this active queue
    ''' </summary>
    <DataMember(Name:="ts", EmitDefaultValue:=False)>
    Public Property TargetSessionCount As Integer

    ''' <summary>
    ''' The count of locked items in this active queue.
    ''' </summary>
    <DataMember(Name:="l", EmitDefaultValue:=False)>
    Public Property Locked As Integer

    <DataMember(Name:="se", EmitDefaultValue:=False)>
    Public Property SessionExceptionRetry As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new blank work queue.
    ''' </summary>
    Public Sub New()
        Me.New(Guid.Empty, "", "", 3, True, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new work queue using the data found in the given provider
    ''' </summary>
    ''' <param name="prov">The provider from which the data should be drawn.
    ''' This expects the following columns :-
    ''' <list>
    ''' <item>id : Guid : The ID of the queue</item>
    ''' <item>ident: Integer: The identity of the queue (ie its primary key)</item>
    ''' <item>name : String : The name of the queue</item>
    ''' <item>keyfield : String : The name of the field within the data to use
    ''' as the key for this queue.</item>
    ''' <item>processid : Guid : The ID of the process which works this (active)
    ''' queue</item>
    ''' <item>resourcegroupid : Guid : The ID of the resource group which is used to
    ''' schedule processes to work this (active) queue</item>
    ''' <item>running : Boolean : Whether the queue is currently running or not</item>
    ''' <item>maxattempts : Integer : The maximum number of retries for items in
    ''' this queue.</item>
    ''' <item>total : Integer : The total number of items in the queue</item>
    ''' <item>completed : Integer : The number of completed items in the queue</item>
    ''' <item>pending : Integer : The number of pending items in the queue</item>
    ''' <item>exceptioned : Integer : The number of exceptioned items in the queue
    ''' </item>
    ''' <item>totalworktime : Integer : The total number of seconds for items in
    ''' the queue - note that this will contain deferred items as well as finished ones
    ''' </item>
    ''' <item>averageworkedtime : Double : The average number of seconds for all
    ''' <em>worked</em> items in this queue - ie. all those which have finished,
    ''' succesfully or otherwise.</item>
    ''' </list>
    ''' </param>
    Public Sub New(ByVal prov As IDataProvider)
        Me.New(
         prov.GetGuid("id"),
         prov.GetValue("name", ""),
         prov.GetValue("keyfield", ""),
         prov.GetValue("maxattempts", 3),
         prov.GetValue("running", True),
         prov.GetValue("encryptid", 0)
                 )

        ' Of course, a data provider can hold much more than that...
        Ident = prov.GetValue("ident", 0)
        ProcessId = prov.GetGuid("processid")
        ProcessName = prov.GetString("processname")
        ResourceGroupId = prov.GetGuid("resourcegroupid")
        ResourceGroupName = prov.GetString("resourcegroupname")
        SnapshotConfigurationId = prov.GetInt("snapshotconfigurationid", -1)
        SessionExceptionRetry = prov.GetBool("sessionexceptionretry")
        SetStatistics(prov)
    End Sub

    ''' <summary>
    ''' Common constructor. Creates a new work queue with the given properties.
    ''' </summary>
    ''' <param name="id">The ID with which this queue is identified.</param>
    ''' <param name="name">The name of this queue.</param>
    ''' <param name="keyField">The key field from which each queue item's key is
    ''' retrieved from the item's data collection.</param>
    ''' <param name="maxAttempts">The maximum number of attempts that an item is
    ''' automatically retried before finally failing.</param>
    ''' <param name="running">Whether the queue is running or suspended.</param>
    Public Sub New(
                   id As Guid,
                   name As String,
                   keyField As String,
                   maxAttempts As Integer,
                   running As Boolean,
                   encryptID As Integer)
        Me.Id = id
        Me.Name = name
        Me.KeyField = keyField
        Me.MaxAttempts = maxAttempts
        Me.IsRunning = running
        mOrigEncryptID = encryptID
        Me.EncryptionKeyID = encryptID
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The clock used to calculate queue times (exposed for unit testing)
    ''' </summary>
    Public Property Clock As ISystemClock
        Get
            mClock = If(mClock, New SystemClockWrapper())
            Return mClock
        End Get
        Set
            mClock = Value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the store to use to load the resource group for this work queue
    ''' </summary>
    Public Property GroupStore As IGroupStore
        Get
            If mGroupStore Is Nothing Then Return NullGroupStore.Instance
            Return mGroupStore
        End Get
        Set(value As IGroupStore)
            mGroupStore = value
        End Set
    End Property

    ''' <summary>
    ''' The name of this work queue.
    ''' </summary>
    Public Property Name() As String
        Get
            Return If(mName, "")
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    ''' <summary>
    ''' Checks if this queue is snapshot configured.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsSnapshotConfigured() As Boolean
        Get
            Return SnapshotConfigurationId <> -1
        End Get
    End Property

    ''' <summary>
    ''' Checks if this queue object represents an 'active queue' or not. An active
    ''' queue has an <see cref="ProcessId">assigned process</see> and an
    ''' <see cref="ResourceGroupId">assigned resource group</see>, and it is capable
    ''' of running the process on the resources in the group.
    ''' </summary>
    Public ReadOnly Property IsActive As Boolean
        Get
            Return (ProcessId <> Guid.Empty AndAlso
                    ResourceGroupId <> Guid.Empty)
        End Get
    End Property

    ''' <summary>
    ''' The field within the data collection used as the key for this work queue
    ''' </summary>
    Public Property KeyField() As String
        Get
            Return If(mKeyField, "")
        End Get
        Set(ByVal value As String)
            mKeyField = value
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating if this queue as marked as being encrypted. True indicates
    ''' that the queue has an encryption key set in it; False means that it has not.
    ''' </summary>
    Public ReadOnly Property IsEncrypted() As Boolean
        Get
            Return (EncryptionKeyID > 0)
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if the encryption key has changed since the last time it was
    ''' set from the database / created.
    ''' </summary>
    Public ReadOnly Property EncryptionKeyChanged() As Boolean
        Get
            Return EncryptionKeyID <> mOrigEncryptID
        End Get
    End Property

    ''' <summary>
    ''' The label indicating the running status of this queue.
    ''' This will be either 'Active' or 'Paused' depending on the
    ''' <see cref="IsRunning"/> property.
    ''' </summary>
    Public ReadOnly Property RunningLabel() As String
        Get
            Return If(IsRunning, My.Resources.clsWorkQueue_RunningLabel_Running, My.Resources.clsWorkQueue_RunningLabel_Paused)
        End Get
    End Property

    ''' <summary>
    ''' The amount of time remaining, based on the number of pending cases and the
    ''' average work time for a case.
    ''' </summary>
    Public ReadOnly Property TimeRemaining As TimeSpan
        Get
            Return New TimeSpan(CLng(Pending - Deferred) * AverageWorkedTime.Ticks)
        End Get
    End Property

    ''' <summary>
    ''' Gets the <see cref="TimeRemaining"/> in display format.
    ''' </summary>
    Public ReadOnly Property TimeRemainingDisplay As String
        Get
            Return GetDisplayString(TimeRemaining)
        End Get
    End Property

    ''' <summary>
    ''' The time remaining for all currently assigned sessions to work the remaining
    ''' pending cases
    ''' </summary>
    Public ReadOnly Property ElapsedTimeRemaining As TimeSpan
        Get
            Dim remaining = TimeRemaining
            ' If there's no time remaining, then there's no time remaining
            If remaining = TimeSpan.Zero Then Return remaining

            ' If there is stuff to do and no assigned sessions, there is an
            ' infinite amount of time remaining - best we can do is MaxValue
            Dim sessionCount As Integer = RunningSessions.Count
            If sessionCount = 0 Then Return TimeSpan.MaxValue

            ' Otherwise, just get the mean value
            Return New TimeSpan(remaining.Ticks \ CLng(sessionCount))
        End Get
    End Property

    ''' <summary>
    ''' Gets the <see cref="ElapsedTimeRemaining"/> in display format.
    ''' </summary>
    Public ReadOnly Property ElapsedTimeRemainingDisplay As String
        Get
            Return GetDisplayString(ElapsedTimeRemaining)
        End Get
    End Property

    ''' <summary>
    ''' The projected end time (in local time) for this queue, taking into account
    ''' the current time and the <see cref="ElapsedTimeRemaining"/>. If there is
    ''' infinite time remaining, this will return <see cref="DateTime.MaxValue"/> -
    ''' the constant, ie. of the 'kind': <see cref="DateTimeKind.Unspecified"/>.
    ''' </summary>
    Public ReadOnly Property EndTime As Date
        Get
            Dim remaining = ElapsedTimeRemaining
            ' If we have no time remaining, no point in providing an end time
            If remaining = TimeSpan.Zero Then Return Date.MinValue
            ' If we have an infinite amount of time remaining, our end time is
            ' effectively infinity - MaxValue, again, is as close as we can get
            If remaining = TimeSpan.MaxValue Then Return Date.MaxValue

            Return Clock.Now.LocalDateTime + remaining
        End Get
    End Property

    ''' <summary>
    ''' The projected end time (in UTC) for this queue, taking into account
    ''' the current time and the <see cref="ElapsedTimeRemaining"/>. If there is
    ''' infinite time remaining, this will return <see cref="DateTime.MaxValue"/> -
    ''' the constant, ie. of the 'kind': <see cref="DateTimeKind.Unspecified"/>.
    ''' </summary>
    Public ReadOnly Property EndTimeUTC As Date
        Get
            Dim remaining = ElapsedTimeRemaining
            ' If we have an infinite amount of time remaining, our end time is
            ' effectively infinity - MaxValue, again, is as close as we can get.
            If remaining = TimeSpan.MaxValue Then Return Date.MaxValue

            Return Clock.UtcNow.UtcDateTime + remaining
        End Get
    End Property

    ''' <summary>
    ''' Gets a readonly collection containing the objects representing sessions which
    ''' were created on behalf of this active queue.
    ''' </summary>
    Public Property Sessions As ICollection(Of clsProcessSession)
        Set(value As ICollection(Of clsProcessSession))
            If value Is Nothing _
             Then mSessions = Nothing _
             Else mSessions = GetReadOnly.ICollection(value)
        End Set
        Private Get
            Return If(mSessions, GetEmpty.ICollection(Of clsProcessSession))
        End Get
    End Property

    ''' <summary>
    ''' Gets the sessions which are running on behalf of this active queue.
    ''' </summary>
    Public ReadOnly Property RunningSessions As ICollection(Of clsProcessSession)
        Get
            Return CollectionUtil.Filter(Sessions, Function(s) s.IsRunning)
        End Get
    End Property

    ''' <summary>
    ''' Gets the sessions which are running and in which a stop has not been
    ''' requested, ie. those which are currently set to continue to completion.
    ''' </summary>
    Public ReadOnly Property ContinuingSessions As ICollection(Of clsProcessSession)
        Get
            Return CollectionUtil.Filter(Sessions,
             Function(s) s.IsRunning AndAlso Not s.IsStopRequested)
        End Get
    End Property

    ''' <summary>
    ''' Gets the 'active' (ie. pending or running) sessions which were created on
    ''' behalf of this active queue.
    ''' </summary>
    Public ReadOnly Property ActiveSessions As ICollection(Of clsProcessSession)
        Get
            Return CollectionUtil.Filter(Sessions, Function(s) s.IsActive)
        End Get
    End Property

    ''' <summary>
    ''' Gets the 'Stopping' sessions which were created on behalf of this active
    ''' queue.
    ''' </summary>
    Public ReadOnly Property StoppingSessions As ICollection(Of clsProcessSession)
        Get
            Return CollectionUtil.Filter(Sessions, Function(s) s.IsStopRequested)
        End Get
    End Property

    ''' <summary>
    ''' Gets the IDs of the <see cref="ActiveSessions"/> which were created on behalf
    ''' of this active queue.
    ''' </summary>
    Public ReadOnly Property ActiveSessionIds As ICollection(Of Guid)
        Get
            Return CollectionUtil.Convert(ActiveSessions, Function(s) s.SessionID)
        End Get
    End Property

    ''' <summary>
    ''' The group tree that owns the resource group used to represent this queue.
    ''' </summary>
    Private ReadOnly Property GroupTree As IGroupTree
        Get
            Return GroupStore.GetTree(
                GroupTreeType.Resources,
                ResourceGroupMember.Active,
                Nothing,
                False,
                False,
                False)
        End Get
    End Property

    ''' <summary>
    ''' Gets the resource group which owns the resources which will be running this
    ''' queue's active sessions or null if either there is no resource group, or the
    ''' resource group doesn't exist in the tree or this work queue has no gruop
    ''' store to retrieve its data from.
    ''' </summary>
    Public ReadOnly Property ResourceGroup As IGroup
        Get
            Return GroupTree.Root.FindSubGroup(ResourceGroupId)
        End Get
    End Property

    ''' <summary>
    ''' Gets the names of the resources in the assigned resource group which had a
    ''' status of <see cref="ResourceStatus.Idle"/> when the group data was
    ''' retrieved.
    ''' </summary>
    Public ReadOnly Property IdleResourceNames As ICollection(Of String)
        Get
            Dim gp As IGroup = ResourceGroup
            If gp Is Nothing Then Return GetEmpty.ICollection(Of String)()
            Return New HashSet(Of String)(gp.ReturnResourceName(ResourceStatus.Idle))
        End Get
    End Property

    ''' <summary>
    ''' Gets a count of the 'idle' resources in the assigned resource group,
    ''' ie. the resources which had a status of <see cref="ResourceStatus.Idle"/>
    ''' when the group data was retrieved.
    ''' </summary>
    Public ReadOnly Property IdleResourceCount As Integer
        Get
            Return IdleResourceNames.Count
        End Get
    End Property

    ''' <summary>
    ''' Gets the count of 'reserved' resources - ie. those which are either already
    ''' running sessions (including those on which a stop has been requested) or
    ''' which are going to be running sessions in order to reach a target.
    ''' </summary>
    Public ReadOnly Property ReservedResourceCount As Integer
        Get
            Return Math.Max(RunningSessions.Count, TargetSessionCount)
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Checks if this queue is the owner of the active session identified by the
    ''' given ID.
    ''' </summary>
    ''' <param name="sessId">The ID of the session to check for</param>
    ''' <returns>True if this queue's sessions contains one with the given ID;
    ''' false otherwise.</returns>
    Public Function IsActiveSessionOwner(sessId As Guid) As Boolean
        Return Sessions.Any(Function(s) s.SessionID = sessId)
    End Function

    Public Sub UpdateActiveData()
        Dim q = gSv.UpdateActiveQueueData(Me)
        UpdateActiveData(q)
    End Sub

    ''' <summary>
    ''' Updates the active data held in this queue, ie. the  <see cref="Sessions"/>
    ''' and the <see cref="TargetSessionCount"/>.
    ''' </summary>
    Public Sub UpdateActiveData(q As clsWorkQueue)
        Sessions = q.Sessions
        TargetSessionCount = q.TargetSessionCount
        ResetTargetSessionCount()
    End Sub

    ''' <summary>
    ''' Updates the statistics held in this queue. This is the properties: <list>
    ''' <item><see cref="TotalAttempts"/></item>
    ''' <item><see cref="Completed"/></item>
    ''' <item><see cref="Pending"/></item>
    ''' <item><see cref="Deferred"/></item>
    ''' <item><see cref="Exceptioned"/></item>
    ''' <item><see cref="TotalWorkTime"/></item>
    ''' <item><see cref="AverageWorkedTime"/></item>
    ''' <item><see cref="Locked"/></item>
    ''' </list>
    ''' </summary>
    Public Sub UpdateStats(q As clsWorkQueue)
        TotalAttempts = q.TotalAttempts
        Completed = q.Completed
        Pending = q.Pending
        Deferred = q.Deferred
        Exceptioned = q.Exceptioned
        TotalWorkTime = q.TotalWorkTime
        AverageWorkedTime = q.AverageWorkedTime
        Locked = q.Locked
    End Sub

    ''' <summary>
    ''' Gets the display string for a timespan.
    ''' </summary>
    ''' <param name="ts">The timespan for which a display string is required.</param>
    ''' <returns>An empty string if the timespan represents a boundary time span;
    ''' otherwise the string-formatted timespan, ignoring the milliseconds present
    ''' in the span.</returns>
    Private Function GetDisplayString(ts As TimeSpan) As String
        If ts = TimeSpan.Zero OrElse ts = TimeSpan.MaxValue Then Return ""
        ' Remove the millis from the span
        ts = New TimeSpan(ts.Ticks - (ts.Ticks Mod TimeSpan.TicksPerSecond))
        ' And return the string equivalent
        Return ts.ToString()
    End Function

    ''' <summary>
    ''' Resets the target session count to match the number of active sessions for
    ''' this queue - ie. this checks all currently created or running sessions which
    ''' do not have a stop request set on them and sets the target session count to
    ''' the number found.
    ''' </summary>
    ''' <returns>The new target session count after resetting</returns>
    Friend Function ResetTargetSessionCount() As Integer
        TargetSessionCount =
            Sessions.AsEnumerable().Count(Function(s) s.IsActiveNotStopping)
        Return TargetSessionCount
    End Function

    ''' <summary>
    ''' Validates the name of this queue, ensuring that it is neither empty, nor
    ''' too long for the database.
    ''' </summary>
    Public Sub ValidateName()
        If String.IsNullOrEmpty(Name) Then Throw New ArgumentNullException(
         "Name", My.Resources.clsWorkQueue_YouMustProvideAQueueName)

        If Name.Length > MaxLengths.Name Then Throw New BluePrismException(
         My.Resources.clsWorkQueue_QueueNameMustBeNoMoreThan0Characters, MaxLengths.Name)
    End Sub

    ''' <summary>
    ''' Checks if this queue has the same configuration details as the given work
    ''' queue - this specfically ignores the ID of the queue and checks the name,
    ''' key field and max attempts
    ''' </summary>
    ''' <param name="wq">The queue to check if the details match this queue.</param>
    ''' <returns>True if the given queue is not null and has the same configuration
    ''' details as this queue.</returns>
    Public Function HasSameDetails(ByVal wq As clsWorkQueue) As Boolean
        Return (wq IsNot Nothing AndAlso
         Name = wq.Name AndAlso
         KeyField = wq.KeyField AndAlso
         MaxAttempts = wq.MaxAttempts
        )
    End Function

    ''' <summary>
    ''' "Commits" the encryption key name - setting it as the original encryption key
    ''' name for the purpose of detecting when it has changed.
    ''' This should be done every time the key is updated from the database.
    ''' </summary>
    ''' <remarks>This should only really be called explicitly by the database method
    ''' responsible for retrieving / updating work queue data, as it is the only part
    ''' of the code which knows what state the database is in.</remarks>
    Friend Sub CommitEncryptionKey()
        mOrigEncryptID = EncryptionKeyID
    End Sub

    ''' <summary>
    ''' "Resets" the encryption key name - ie. sets it to the original value from the
    ''' last time it was retrieved / updated on the database.
    ''' </summary>
    Public Sub ResetEncryptionKey()
        EncryptionKeyID = mOrigEncryptID
    End Sub

    ''' <summary>
    ''' Gets a string representation of this work queue.
    ''' </summary>
    ''' <returns>The name of this work queue.</returns>
    Public Overrides Function ToString() As String
        Return Me.Name
    End Function

    ''' <summary>
    ''' Checks if this work queue is equal to the given object.
    ''' An object is considered equal to a work queue if it is a non-null
    ''' instance of clsWorkQueue with the same ID and name as this one.
    ''' </summary>
    ''' <param name="obj">The object to check for equality against.</param>
    ''' <returns>True if the given object is a non-null work queue object
    ''' with the same ID and name as this one.</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim q As clsWorkQueue = TryCast(obj, clsWorkQueue)
        If q Is Nothing Then Return False
        Return Me.Id = q.Id AndAlso Me.Name = q.Name
    End Function

    ''' <summary>
    ''' Gets an integer hash for this work queue.
    ''' </summary>
    ''' <returns>An integer hash for this work queue - a function of ident and Name.
    ''' </returns>
    Public Overrides Function GetHashCode() As Integer
        Return Ident.GetHashCode() Xor Name.GetHashCode()
    End Function

    ''' <summary>
    ''' Sets the statistics for this queue from the given data provider.
    ''' </summary>
    ''' <param name="prov">The provider from which the data should be drawn.
    ''' This expects the following columns :-
    ''' <list>
    ''' <item>total : Integer : The total number of items in the queue</item>
    ''' <item>completed : Integer : The number of completed items in the queue</item>
    ''' <item>pending : Integer : The number of pending items in the queue</item>
    ''' <item>exceptioned : Integer : The number of exceptioned items in the queue</item>
    ''' <item>totalworktime : Integer : The total number of seconds for items in
    ''' the queue - note that this will contain deferred items as well as finished ones
    ''' </item>
    ''' <item>averageworkedtime : Double : The average number of seconds for all
    ''' <em>worked</em> items in this queue - ie. all those which have finished,
    ''' succesfully or otherwise.</item>
    ''' </list>
    ''' </param>
    Friend Sub SetStatistics(prov As IDataProvider)
        TotalAttempts = prov.GetValue("total", 0)

        ' Little shortcut here - there's no point in filling in the rest of the
        ' stats when the 'total number of items' is zero - they will all be
        ' zero by definition (or the data is corrupt - one of the two)
        If TotalAttempts = 0 Then Return

        Completed = prov.GetValue("completed", 0)
        Pending = prov.GetValue("pending", 0)
        Deferred = prov.GetValue("deferred", 0)
        Exceptioned = prov.GetValue("exceptioned", 0)
        TotalWorkTime = TimeSpan.FromSeconds(prov.GetValue("totalworktime", 0L))
        AverageWorkedTime =
         TimeSpan.FromSeconds(prov.GetValue("averageworkedtime", 0.0))
        Locked = prov.GetValue("locked", 0)
    End Sub

#End Region

End Class
