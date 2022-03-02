Imports System.IO

Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore
Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.My.Resources
Imports BluePrism.Server.Domain.Models

''' Project  : AutomateAppCore
''' Class    : clsWorkQueueItem
''' <summary>
''' Class defining an object which represents a work queue item.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class clsWorkQueueItem

#Region " Class-scope definitions "

    ''' <summary>
    ''' Maximum allowed lengths of the specified fields.
    ''' </summary>
    Public Class MaxLengths
        Public Const KeyValue As Integer = 255
        Public Const Status As Integer = 255
    End Class

    ''' <summary>
    ''' A dictionary of column names mapped to their types.
    ''' This is primarily for use converting the data to other formats
    ''' </summary>
    Private Shared ReadOnly ColumnDefs As IDictionary(Of String, Type)

    ''' <summary>
    ''' Class constructor. This builds the column definitions dictionary
    ''' </summary>
    Shared Sub New()
        ColumnDefs = New clsOrderedDictionary(Of String, Type)
        ColumnDefs.Add("Id", GetType(Guid))
        ColumnDefs.Add("KeyValue", GetType(String))
        ColumnDefs.Add("Priority", GetType(Integer))
        ColumnDefs.Add("Status", GetType(String))
        ColumnDefs.Add("Tags", GetType(String))
        ColumnDefs.Add("Resource", GetType(String))
        ColumnDefs.Add("Attempt", GetType(Integer))
        ColumnDefs.Add("Loaded", GetType(DateTime))
        ColumnDefs.Add("LastUpdated", GetType(DateTime))
        ColumnDefs.Add("Deferred", GetType(DateTime))
        ColumnDefs.Add("Locked", GetType(DateTime))
        ColumnDefs.Add("Completed", GetType(DateTime))
        ColumnDefs.Add("Worktime", GetType(Integer))
        ColumnDefs.Add("Exception", GetType(DateTime))
        ColumnDefs.Add("ExceptionReason", GetType(String))
    End Sub

    ''' <summary>
    ''' The states as held in the database for a given work queue item.
    ''' </summary>
    ''' <remarks>The column 'state' which these refer to was added in the
    ''' DB script : db_upgradeR113.sql </remarks>
    Public Enum State
        Exceptioned = 5
        Completed = 4
        Deferred = 3
        Locked = 2
        Pending = 1
        None = 0
    End Enum

    ''' <summary>
    ''' The characters which have meaning within a CSV file, and would thus cause a
    ''' field to need to be escaped.
    ''' </summary>
    Private Shared ReadOnly ReservedCsvChars As Char() = ("""," & vbCrLf).ToCharArray()

#End Region

#Region " Member Variables "

    ' The Item ID
    <DataMember>
    Private mID As Guid

    ' The priority of this item instance
    <DataMember>
    Private mPriority As Integer

    ' The ID of the item attempt
    <DataMember>
    Private mIdent As Long

    ' The state of the item
    <DataMember>
    Private mCurrentState As State

    ' The queue position of the item
    <DataMember>
    Private mPosition As Integer

    ' The key value string for the item
    <DataMember>
    Private mKey As String

    ' The user-set status of the item
    <DataMember>
    Private mStatus As String

    ' The tags applied to the item
    <DataMember>
    Private mTagMask As clsTagMask

    ' The last resource to have used the item
    <DataMember>
    Private mResource As String

    ' The attempt number represented by this object
    <DataMember>
    Private mAttempt As Integer

    ' The loaded date for the item
    <DataMember>
    Private mLoaded As DateTime

    ' The deferred date for the item
    <DataMember>
    Private mDeferred As DateTime

    ' The last updated date
    <DataMember>
    Private mLastUpdated As DateTime

    ' The locked date for the item
    <DataMember>
    Private mLocked As DateTime

    ' The completed date for the item
    <DataMember>
    Private mCompleted As DateTime

    ' The total work time (in seconds) of the item
    <DataMember>
    Private mWorktime As Integer

    ' The work item of this item attempt
    <DataMember>
    Private mAttemptWorkTime As Integer

    ' The date/time of the exception set in this item
    <DataMember>
    Private mException As DateTime

    ' The exception reason for the item
    <DataMember>
    Private mExceptionReason As String

    ' The data XML for the item, may be null meaning the data has not been populated,
    ' not necessarily that it doesn't exist
    <DataMember>
    Private mDataXml As String

    ' The data for the item in a collection row
    <DataMember>
    Private mRow As clsCollectionRow

    <DataMember>
    Private mSessionExceptionRetry As Boolean

#End Region

#Region " Properties "

    ''' <summary>
    ''' The GUID identifier for the item.
    ''' </summary>
    Public Property ID() As Guid
        Get
            Return mID
        End Get
        Set(ByVal value As Guid)
            mID = value
        End Set
    End Property

    ''' <summary>
    ''' The db-specific identity of the work queue item
    ''' </summary>
    Public Property Ident() As Long
        Get
            Return mIdent
        End Get
        Set(ByVal value As Long)
            mIdent = value
        End Set
    End Property

#Region "Item State"

    ''' <summary>
    ''' <para>The current state of the work queue item. This will usually attempt to 
    ''' derive the state of the item from the rest of the fields, though it can
    ''' be overridden by setting it via this property.</para>
    ''' 
    ''' <para>To restore default behaviour, just set the state to</para>
    ''' <see cref="State.None">State.Unknown</see>
    ''' </summary>
    Public Property CurrentState() As State
        Get
            ' Ideally, the state should always be intuited from the data
            If mCurrentState = State.None Then
                ' Try and intuit it from the data
                If mLocked <> Nothing Then
                    Return State.Locked
                ElseIf mException <> Nothing Then
                    Return State.Exceptioned
                ElseIf mCompleted <> Nothing Then
                    Return State.Completed
                ElseIf mDeferred <> Nothing Then
                    Return State.Deferred
                Else
                    Return State.Pending
                End If
            End If
            Return mCurrentState
        End Get
        Set(ByVal value As State)
            mCurrentState = value
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating if this item is currently locked
    ''' </summary>
    Public ReadOnly Property IsLocked() As Boolean
        Get
            Return CurrentState = State.Locked
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if this item is currently exceptioned
    ''' </summary>
    Public ReadOnly Property IsExceptioned() As Boolean
        Get
            Return CurrentState = State.Exceptioned
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if this item is currently completed
    ''' </summary>
    Public ReadOnly Property IsCompleted() As Boolean
        Get
            Return CurrentState = State.Completed
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if this item is currently pending
    ''' </summary>
    Public ReadOnly Property IsPending() As Boolean
        Get
            Return CurrentState = State.Pending
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if this item is currently deferred
    ''' </summary>
    Public ReadOnly Property isDeferred() As Boolean
        Get
            Return CurrentState = State.Deferred
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if this item is currently awaiting execution, ie.
    ''' if it is pending or deferred.
    ''' </summary>
    Public ReadOnly Property IsAwaitingExecution() As Boolean
        Get
            Select Case CurrentState
                Case State.Pending, State.Deferred, State.Locked
                    Return True
                Case Else
                    Return False
            End Select
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating whether this item has been worked, ie. if it has
    ''' previously exceptioned or completed.
    ''' Note this will return true if it has exceptioned and is currently 
    ''' awaiting a retry.
    ''' </summary>
    Public ReadOnly Property isWorked() As Boolean
        Get
            Select Case CurrentState
                Case State.Exceptioned, State.Completed
                    Return True
                Case Else
                    Return False
            End Select
        End Get
    End Property

    ''' <summary>
    ''' Gets the image key corresponding to the current state of this work queue
    ''' item. This will be one of:
    ''' <list>
    ''' <item>"Person" for an <see cref="State.Exceptioned">exceptioned</see> item</item>
    ''' <item>"Tick" for a <see cref="State.Completed">completed</see> item</item>
    ''' <item>"Padlock" for a <see cref="State.Locked">locked</see> item</item>
    ''' <item>"Ellipsis" for a <see cref="State.Pending">pending</see> or
    ''' <see cref="State.Deferred">Deferred</see> item.</item>
    ''' </list>
    ''' </summary>
    Public ReadOnly Property CurrentStateImageKey() As String
        Get
            Select Case CurrentState
                Case State.Exceptioned
                    Return "Person"
                Case State.Completed
                    Return "Tick"
                Case State.Locked
                    Return "Padlock"
                Case Else
                    Return "Ellipsis"
            End Select
        End Get
    End Property

#End Region

    ''' <summary>
    ''' Gets or sets the priority of the item instance
    ''' </summary>
    Public Property Priority() As Integer
        Get
            Return mPriority
        End Get
        Set(ByVal value As Integer)
            mPriority = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the position of this item in the queue - zero if it's not known,
    ''' or -1 if it is not pending in the queue
    ''' </summary>
    Public Property Position() As Integer
        Get
            Return mPosition
        End Get
        Set(ByVal value As Integer)
            mPosition = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the position of this item in the queue as a display string, returning
    ''' an empty string if it is not pending or its position is unknown
    ''' </summary>
    Public ReadOnly Property PositionDisplay() As String
        Get
            Return CStr(IIf(mPosition > 0, mPosition, ""))
        End Get
    End Property

    ''' <summary>
    ''' The key value for this item (if it has one)
    ''' </summary>
    Public Property KeyValue() As String
        Get
            Return Resolve(mKey)
        End Get
        Set(ByVal value As String)
            If value IsNot Nothing AndAlso value.Length > MaxLengths.KeyValue Then
                Throw New FieldLengthException(
                 My.Resources.clsWorkQueueItem_GivenKeyValueIsLongerThanThePermitted0Characters1,
                 MaxLengths.KeyValue, value)
            End If
            mKey = value
        End Set
    End Property

    ''' <summary>
    ''' The status of the item
    ''' </summary>
    Public Property Status() As String
        Get
            Return mStatus
        End Get
        Set(ByVal value As String)
            mStatus = value
        End Set
    End Property

    ''' <summary>
    ''' Collection of tags which is currently applied to this work queue item
    ''' </summary>
    Public ReadOnly Property Tags() As ICollection(Of String)
        Get
            ' Append the exception reason as a 'virtual tag' if one exists.
            If String.IsNullOrEmpty(ExceptionReason) Then Return mTagMask.OnTags
            Dim tagSet As New clsSet(Of String)(mTagMask.OnTags)
            tagSet.Add("Exception: " & ExceptionReason.Replace(";"c, ":"c))
            Return tagSet
        End Get
    End Property

    ''' <summary>
    ''' Gets the tags applied to this item as a semi-colon-separated string
    ''' </summary>
    Public ReadOnly Property TagString() As String
        Get
            Return clsTagMask.ToString(Tags, "")
        End Get
    End Property

    ''' <summary>
    ''' The resource which is working or has worked this queue item.
    ''' </summary>
    Public Property Resource() As String
        Get
            If mResource Is Nothing Then Return ""
            Return mResource
        End Get
        Set(ByVal value As String)
            mResource = value
        End Set
    End Property

    ''' <summary>
    ''' The attempt number of this work queue item.
    ''' </summary>
    Public Property Attempt() As Integer
        Get
            Return mAttempt
        End Get
        Set(ByVal value As Integer)
            mAttempt = value
        End Set
    End Property

    ''' <summary>
    ''' The number of attempts taken by this work item. This mirrors the old
    ''' 'attempts' value for a work queue item, as opposed to the 'attempt number'
    ''' which is used now.
    ''' </summary>
    Public ReadOnly Property AttemptsSoFar() As Integer
        Get
            ' If not worked or locked, we want to dec this by one to indicate that
            ' this attempt has not been initiated yet.
            If FinishedDate = Nothing AndAlso Locked = Nothing Then
                Return Me.Attempt - 1
            Else
                Return Me.Attempt
            End If
        End Get
    End Property

    ''' <summary>
    ''' The date/time at which point this item was loaded
    ''' </summary>
    Public Property Loaded() As DateTime
        Get
            Return mLoaded
        End Get
        Set(ByVal value As DateTime)
            mLoaded = value
        End Set
    End Property

    ''' <summary>
    ''' The date/time at which point this item was loaded, formatted into a string
    ''' </summary>
    Public ReadOnly Property LoadedDisplay() As String
        Get
            Return FormatDateTime(Loaded)
        End Get
    End Property

    ''' <summary>
    ''' The date/time that this item is deferred to
    ''' </summary>
    Public Property Deferred() As DateTime
        Get
            Return mDeferred
        End Get
        Set(ByVal value As DateTime)
            mDeferred = value
        End Set
    End Property

    ''' <summary>
    ''' The date/time that this item is deferred to, formatted into a string
    ''' </summary>
    Public ReadOnly Property DeferredDisplay() As String
        Get
            Return FormatDateTime(Deferred)
        End Get
    End Property

    ''' <summary>
    ''' The date/time at which point this item was locked (if it is locked)
    ''' </summary>
    Public Property Locked() As DateTime
        Get
            Return mLocked
        End Get
        Set(ByVal value As DateTime)
            mLocked = value
        End Set
    End Property

    ''' <summary>
    ''' The date/time at which point this item was locked (if it is locked), 
    ''' formatted into a string
    ''' </summary>
    Public ReadOnly Property LockedDisplay() As String
        Get
            Return FormatDateTime(Locked)
        End Get
    End Property

    ''' <summary>
    ''' The date/time at which this item was completed (if it has been completed)
    ''' </summary>
    Public Property CompletedDate() As DateTime
        Get
            Return mCompleted
        End Get
        Set(ByVal value As DateTime)
            mCompleted = value
        End Set
    End Property

    ''' <summary>
    ''' The date/time at which this item was completed (if it has been completed),
    ''' formatted into a string
    ''' </summary>
    Public ReadOnly Property CompletedDateDisplay() As String
        Get
            Return FormatDateTime(CompletedDate)
        End Get
    End Property


    ''' <summary>
    ''' The number of seconds that this item has been worked on for (including all
    ''' previous attempts)
    ''' </summary>
    Public Property Worktime() As Integer
        Get
            Return mWorktime
        End Get
        Set(ByVal value As Integer)
            mWorktime = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the amount of time this item has been worked on for as a timespan
    ''' </summary>
    Public ReadOnly Property WorkTimeSpan() As TimeSpan
        Get
            Return TimeSpan.FromSeconds(mWorktime)
        End Get
    End Property

    ''' <summary>
    ''' The number of seconds that this particular attempt of the queue item has
    ''' been worked on for.
    ''' </summary>
    Public Property AttemptWorkTime() As Integer
        Get
            Return mAttemptWorkTime
        End Get
        Set(ByVal value As Integer)
            mAttemptWorkTime = value
        End Set
    End Property

    ''' <summary>
    ''' The amount of time this particular attempt of the item has been worked on for
    ''' </summary>
    Public ReadOnly Property AttemptWorkTimeSpan() As TimeSpan
        Get
            Return TimeSpan.FromSeconds(mAttemptWorkTime)
        End Get
    End Property

    ''' <summary>
    ''' Gets the amount of time this item has been worked on for, formatted into a
    ''' string
    ''' </summary>
    Public ReadOnly Property WorkTimeDisplay() As String
        Get
            Return FormatTimespan(WorkTimeSpan)
        End Get
    End Property

    ''' <summary>
    ''' The date/time at which point this item was exceptioned (if it has been)
    ''' </summary>
    Public Property ExceptionDate() As DateTime
        Get
            Return mException
        End Get
        Set(ByVal value As DateTime)
            mException = value
        End Set
    End Property

    ''' <summary>
    ''' The date/time at which this item was exceptioned (if it has been), formatted
    ''' into a string
    ''' </summary>
    Public ReadOnly Property ExceptionDateDisplay() As String
        Get
            Return FormatDateTime(ExceptionDate)
        End Get
    End Property

    ''' <summary>
    ''' The reason this item has exceptioned or an empty string if it has no reason.
    ''' </summary>
    Public Property ExceptionReason() As String
        Get
            If mExceptionReason Is Nothing Then Return "" Else Return mExceptionReason
        End Get
        Set(ByVal value As String)
            mExceptionReason = value
        End Set
    End Property

    ''' <summary>
    ''' The date that this item was finished - either exceptioned or completed, or
    ''' Date.MinValue if this item hasn't actually finished.
    ''' </summary>
    Public ReadOnly Property FinishedDate() As Date
        Get
            If mException <> Nothing Then Return mException
            If mCompleted <> Nothing Then Return mCompleted
            Return Nothing
        End Get
    End Property

    Public Property LastUpdated() As DateTime
        Get
           Return mLastUpdated
        End Get
        Set
            mLastUpdated = value
        End Set
    End Property

    ''' <summary>
    ''' The date/time that this item was last updated, formatted into a string
    ''' This will be the later of ExceptionDate or CompletedDate, or it will be the
    ''' Loaded date if neither of those are set.
    ''' </summary>
    Public ReadOnly Property LastUpdatedDisplay() As String
        Get
            Return FormatDateTime(LastUpdated)
        End Get
    End Property

    ''' <summary>
    ''' The data stored alongside this item, or null if it has no data, or the
    ''' data has not been set in this work queue item.
    ''' </summary>
    ''' <remarks>
    ''' <strong>Note: </strong> The value returned from this property should be
    ''' considered a clone of the collection held in this item, ie. any changes
    ''' directly to the returned collection will not be reflected in this work queue
    ''' item. In order to set the collection data, it must be set back into this
    ''' item explicitly.
    ''' </remarks>
    Public Property Data() As clsCollection
        Get
            Return New clsCollection(DataXml)
        End Get
        Set(ByVal value As clsCollection)
            If value Is Nothing Then DataXml = Nothing Else DataXml = value.GenerateXML()
        End Set
    End Property

    Public Property SessionExceptionRetry() As Boolean
        Get
            Return mSessionExceptionRetry
        End Get
        Set(ByVal value As Boolean)
            mSessionExceptionRetry = value
        End Set
    End Property

    ''' <summary>
    ''' The data stored alongside this item as an XML-formatted string, or an empty
    ''' string if it has no data, or the data has not been set in this object.
    ''' </summary>
    Friend Property DataXml() As String
        Get
            ' If it's not yet populated...
            If mDataXml Is Nothing Then
                ' Check if we have a row set - if so, use that. Otherwise attempt
                ' to get the data from the database.
                If mRow IsNot Nothing Then
                    mDataXml = New clsCollection(mRow).GenerateXML()
                Else
                    mDataXml = gSv.WorkQueueItemGetDataXml(mIdent)
                End If
            End If
            Return mDataXml
        End Get
        Set(ByVal value As String)
            mDataXml = value
        End Set
    End Property

    ''' <summary>
    ''' The data row which contains the data for this
    ''' </summary>
    Friend Property DataRow() As clsCollectionRow
        Get
            Return mRow
        End Get
        Set(ByVal value As clsCollectionRow)
            mRow = value
        End Set
    End Property

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new WorkQueueItem with data from the given data provider, providing
    ''' a data converter to allow the data within the provider to be properly
    ''' converted (usually encrypted / decrypted)
    ''' </summary>
    ''' <param name="prov">The provider of the data for this queue item. This
    ''' constructor expects the following properties :-<list>
    ''' <item>id: Guid</item>
    ''' <item>ident: Long</item>
    ''' <item>keyvalue: String</item>
    ''' <item>status: String</item>
    ''' <item>attempt: Integer (The <see cref="Attempt"/> number - not the
    ''' <see cref="AttemptsSoFar"/> number)</item>
    ''' </list>
    ''' </param>
    ''' <remarks>Note: this constructor does not read the data xml from the given
    ''' provider. The data can be retrieved dynamically by the DataXml property, but
    ''' if this object resides on the server-side and gSv is not set, that will fail,
    ''' so the server must initialise the data manually if it expects to be using
    ''' that data itself - once the object passes to the client, the DataXml property
    ''' </remarks>
    Friend Sub New(ByVal prov As IDataProvider)
        Me.New( _
         prov.GetValue("id", Guid.Empty), prov.GetValue("ident", 0L), prov.GetString("keyvalue"))
        mStatus = prov.GetString("status")
        mAttempt = prov.GetValue("attempt", 1)
    End Sub

    ''' <summary>
    ''' Constructor to create a WorkQueueItem object with just the item ID.
    ''' </summary>
    ''' <param name="id">The ID of the work queue item to be represented by this
    ''' object.</param>
    Public Sub New(ByVal id As Guid)
        Me.New(id, 0, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new WorkQueueItem with the given identities.
    ''' </summary>
    ''' <param name="id">The ID for this work queue item. Note that this ID is common
    ''' to all instances of this work queue item (ie. all retries).</param>
    ''' <param name="ident">The unique identity for this work queue item instance -
    ''' this is unique amongst all instances of all work queue items.</param>
    Public Sub New(ByVal id As Guid, ByVal ident As Long)
        Me.New(id, ident, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new work queue item identified by the given values.
    ''' </summary>
    ''' <param name="id">The GUID for this item.</param>
    ''' <param name="key">The key value for this item.</param>
    Public Sub New(ByVal id As Guid, ByVal ident As Long, ByVal key As String)
        mID = id
        mIdent = ident
        mKey = key
        mCurrentState = State.None
        mTagMask = New clsTagMask()
        mPosition = -1 ' Set to 'Unknown'
    End Sub

#End Region

#Region " Conversion to other data representations "

    ''' <summary>
    ''' Resolve the given field into a CSV field - ie. if it contains commas
    ''' or quotes, ensure that the field is wrapped into quotes, and any 
    ''' quotes within are escaped (by doubling).
    ''' Also, if the value is null, ensure that an empty string is returned
    ''' </summary>
    ''' <param name="field">The field to resolve</param>
    ''' <returns>The field in such a format that it will not cause any
    ''' problems within a CSV file.</returns>
    Private Shared Function ResolveForCsv(ByVal field As String) As String

        If field Is Nothing Then Return ""
        If field.IndexOfAny(ReservedCsvChars) >= 0 Then
            Return """" & field.Replace("""", """""") & """" ' so elegant...
        End If
        Return field

    End Function

    ''' <summary>
    ''' Resolves the given date time into a csv string - this checks if the
    ''' time is currently 'nothing' (ie. 01/01/0001 00:00:00) and returns an
    ''' empty string if that is the case, otherwise returning the default
    ''' formatting representation of the given date/time.
    ''' </summary>
    ''' <param name="field">The field to resolve</param>
    ''' <returns>The field as a representation usable in a CSV file</returns>
    Private Shared Function ResolveForCsv(ByVal field As DateTime) As String
        If field = Nothing Then Return "" Else Return field.ToString()
    End Function

    ''' <summary>
    ''' Writes a CSV representation of this work queue item to the given text
    ''' writer. 
    ''' </summary>
    ''' <param name="out">The writer to write to.</param>
    ''' <exception cref="IOException">Thrown if an I/O error occurs.</exception>
    ''' <exception cref="ObjectDisposedException">Thrown if the given TextWriter is
    ''' closed.</exception>
    ''' <exception cref="NullReferenceException">Thrown if the given TestWriter
    ''' reference was <c>Nothing</c>.</exception>
    ''' <remarks>
    ''' <strong>Note: </strong> This does not currently include the 'queue item data'
    ''' ie. the arbitrary data which is associated with this work queue item.
    ''' </remarks>
    Public Sub ToCsvWriter(ByVal out As TextWriter)

        ' Note that the order of the fields here should match the order in the
        ' ColumnDefs dictionary.
        out.Write(ID.ToString())
        out.Write(","c)
        out.Write(ResolveForCsv(KeyValue))
        out.Write(","c)
        out.Write(Priority)
        out.Write(","c)
        out.Write(ResolveForCsv(Status))
        out.Write(","c)
        out.Write(ResolveForCsv(AppendTags(New StringBuilder()).ToString()))
        out.Write(","c)
        out.Write(ResolveForCsv(Resource))
        out.Write(","c)
        out.Write(Attempt)
        out.Write(","c)
        out.Write(ResolveForCsv(Loaded))
        out.Write(","c)
        out.Write(ResolveForCsv(LastUpdated))
        out.Write(","c)
        out.Write(ResolveForCsv(Deferred))
        out.Write(","c)
        out.Write(ResolveForCsv(Locked))
        out.Write(","c)
        out.Write(ResolveForCsv(CompletedDate))
        out.Write(","c)
        out.Write(Worktime)
        out.Write(","c)
        out.Write(ResolveForCsv(ExceptionDate))
        out.Write(","c)
        out.Write(ResolveForCsv(ExceptionReason))
        out.WriteLine()

    End Sub

    ''' <summary>
    ''' Writes the given collection of work queue items to the given text writer
    ''' in CSV format.
    ''' </summary>
    ''' <param name="items">The items whose data should be written to the
    ''' given writer.</param>
    ''' <param name="out">The text writer to which the items should be written
    ''' </param>
    ''' <exception cref="IOException">Thrown if an I/O error occurs.</exception>
    ''' <exception cref="ObjectDisposedException">Thrown if the given TextWriter is
    ''' closed.</exception>
    ''' <exception cref="NullReferenceException">Thrown if the given TestWriter
    ''' reference was <c>Nothing</c>.</exception>
    Public Shared Sub ToCsv( _
     ByVal items As ICollection(Of clsWorkQueueItem), _
     ByVal out As TextWriter)

        Dim sep As String = ""
        For Each name As String In ColumnDefs.Keys
            out.Write(sep)
            Dim localizedName = IboResources.ResourceManager.GetString($"clsWorkQueuesActions_Params_{name}")
            out.Write(ResolveForCsv(CStr(IIf(String.IsNullOrWhiteSpace(localizedName), name, localizedName))))
            sep = ","
        Next
        out.WriteLine()

        For Each item As clsWorkQueueItem In items
            item.ToCsvWriter(out)
        Next

        out.Flush()

    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Appends the tags currently set in this work queue item to the given buffer
    ''' </summary>
    ''' <param name="sb">The string builder to which the tags should be appended
    ''' </param>
    ''' <returns>The given string builder with the tags appended</returns>
    Private Function AppendTags(ByVal sb As StringBuilder) As StringBuilder
        Return clsTagMask.ToBuffer(sb, mTagMask.OnTags, "")
    End Function

    ''' <summary>
    ''' Adds the given tag to this work queue item, if it is not already applied to
    ''' it. Note that work queue item tags are case insensitive.
    ''' </summary>
    ''' <param name="tag">The tag which should be added - tags are case-insensitive
    ''' and any outlying whitespace is trimmed before applying it to an item</param>
    Public Sub AddTag(ByVal tag As String)
        If tag = "" Then Return ' Ignore obvious empty tags
        Try
            mTagMask.SetTagOn(tag)

        Catch ae As ArgumentNullException
            ' Ignore null errors for now - this method is currently only called
            ' from the database code anyway

        End Try

    End Sub

    ''' <summary>
    ''' Removes the given tag from this work queue item, if it is currently applied.
    ''' </summary>
    ''' <param name="tag">The tag to remove. Note that tags are case-insensitive and
    ''' any outlying whitespace is trimmed before the tag is removed.</param>
    Public Sub RemoveTag(ByVal tag As String)
        If tag = "" Then Return ' Ignore obvious empty tags
        Try
            mTagMask.SetTagOff(tag)
        Catch ae As ArgumentNullException
            ' Ignore null errors for consistency - AddTag() does so.
        End Try
    End Sub

    ''' <summary>
    ''' Converts a database date/time value to a local one.
    ''' </summary>
    ''' <param name="dbDateTime">The datetime as it appears in the database.</param>
    ''' <returns>Returns a local datetime, equivalent to the supplied one.</returns>
    Private Function FormatDateTime(ByVal dbDateTime As Object) As String
        Dim dt As DateTime = CType(dbDateTime, DateTime)
        If dt = Nothing Then Return ""
        Return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime().ToString()
    End Function

    ''' <summary>
    ''' Formats a timespan for display in the UI.
    ''' </summary>
    ''' <param name="value">The timespan to be displayed.</param>
    ''' <returns>Returns a string representing the supplied timespan, suitable for
    ''' display in the UI.</returns>
    Private Function FormatTimespan(ByVal value As TimeSpan) As String
        Dim sb As New StringBuilder()
        If value.Days > 0 Then sb.AppendFormat("{0}.", value.Days)
        If value.Hours > 0 OrElse value.Days > 0 Then _
             sb.AppendFormat("{0:00}:", value.Hours)
        sb.AppendFormat("{0:00}:{1:00}", value.Minutes, value.Seconds)
        Return sb.ToString()
    End Function


    ''' <summary>
    ''' Utility method to resolve a string to an empty string if null, or
    ''' the original string otherwise
    ''' </summary>
    ''' <param name="str">The string to resolve</param>
    ''' <returns>The resolved string</returns>
    Private Shared Function Resolve(ByVal str As String) As String
        If String.IsNullOrEmpty(str) Then
            Return ""
        Else
            Return str
        End If
    End Function
#End Region

End Class
