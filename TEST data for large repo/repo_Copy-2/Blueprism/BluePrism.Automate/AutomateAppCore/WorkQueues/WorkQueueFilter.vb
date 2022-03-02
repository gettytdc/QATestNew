Imports System.Data.SqlClient
Imports System.Runtime.Serialization

Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.clsServer.Constants


''' <summary>
''' Class encapsulating a set of filters on the querying of work queue contents.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class WorkQueueFilter

#Region " Member Variables "

    ' The specific item to filter on - typically used to get the attempts of a
    ' work item
    <DataMember>
    Private mItemId As Guid = Nothing

    ' The start date to filter the loaded date on
    <DataMember>
    Private mLoadedStartDate As DateTime = MinSQLDate

    ' The end date to filter the loaded date on
    <DataMember>
    Private mLoadedEndDate As DateTime = MaxSQLDate

    ' The start date to filter the completed date on
    <DataMember>
    Private mCompletedStartDate As DateTime = MinSQLDate

    ' The end date to filter the completed date on
    <DataMember>
    Private mCompletedEndDate As DateTime = MaxSQLDate

    ' The start date to filter the lastupdated date on
    <DataMember>
    Private mLastUpdatedStartDate As DateTime = MinSQLDate

    ' The end date to filter the lastupdated date on
    <DataMember>
    Private mLastUpdatedEndDate As DateTime = MaxSQLDate

    ' The start date to filter the exception date on
    <DataMember>
    Private mExceptionStartDate As DateTime = MinSQLDate

    ' The end date to filter the exception date on
    <DataMember>
    Private mExceptionEndDate As DateTime = MaxSQLDate

    ' The start date to filter the deferred date on
    <DataMember>
    Private mNextReviewStartDate As DateTime = MinSQLDate

    ' The end date to filter the deferred date on
    <DataMember>
    Private mNextReviewEndDate As DateTime = MaxSQLDate

    ' The largest acceptable number of attempts against an item
    <DataMember>
    Private mMaxAttempt As Integer = Integer.MaxValue

    ' The smallest acceptable number of attempts against an item
    <DataMember>
    Private mMinAttempt As Integer = Integer.MinValue

    ' The largest acceptable priority
    <DataMember>
    Private mMaxPriority As Integer = Integer.MaxValue

    ' The smallest acceptable priority
    <DataMember>
    Private mMinPriority As Integer = Integer.MinValue

    ' The search string for a status value of an item
    <DataMember>
    Private mStatus As String = ""

    ' The search string for a resource working an item.
    <DataMember>
    Private mResource As String = ""

    ' The tags to apply / mask in the search
    <DataMember>
    Private mTagger As New clsTagMask()

    ' The smallest acceptable worktime on a case, in seconds.
    <DataMember>
    Public MinWorkTime As Integer = Integer.MinValue

    ' The largest acceptable worktime on a case, in seconds.
    <DataMember>
    Public MaxWorkTime As Integer = Integer.MaxValue

    ' The search string for an item's key
    <DataMember>
    Public ItemKey As String = ""

    ' The maximum number of items to be returned by the query.
    <DataMember>
    Public MaxRows As Integer = Integer.MaxValue

    ' The starting zero-based index of the first row to return. Used for paging.
    <DataMember>
    Public StartIndex As Integer

    ' The column to sort any data on
    <DataMember>
    Private mSortCol As QueueSortColumn

    ' The order in which the data should be sorted
    <DataMember>
    Private mSortOrd As QueueSortOrder

    ' The acceptable states of the items to be returned.
    <DataMember>
    Private mStateFilter As clsSet(Of QueueItemState)

#End Region

#Region " Properties "

    ''' <summary>
    ''' The Item ID for the item - this is not used from outside clsServer,
    ''' and is used to retrieve the work queue item retry instances
    ''' for a particular item.
    ''' </summary>
    Friend Property ItemId() As Guid
        Get
            Return mItemId
        End Get
        Set(ByVal value As Guid)
            mItemId = value
        End Set
    End Property

    ''' <summary>
    ''' The earliest acceptable date on which an item was loaded into the queue,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property LoadedStartDate() As DateTime
        Get
            Return mLoadedStartDate
        End Get
        Set(ByVal value As DateTime)
            mLoadedStartDate = value
        End Set
    End Property

    ''' <summary>
    ''' The latest acceptable date on which an item was loaded into the queue,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property LoadedEndDate() As DateTime
        Get
            Return mLoadedEndDate
        End Get
        Set(ByVal value As DateTime)
            mLoadedEndDate = value
        End Set
    End Property

    ''' <summary>
    ''' The earliest acceptable date on which an item was marked as completed,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property CompletedStartDate() As DateTime
        Get
            Return mCompletedStartDate
        End Get
        Set(ByVal value As DateTime)
            mCompletedStartDate = value
        End Set
    End Property

    ''' <summary>
    ''' The latest acceptable date on which an item was marked as completed,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property CompletedEndDate() As DateTime
        Get
            Return mCompletedEndDate
        End Get
        Set(ByVal value As DateTime)
            mCompletedEndDate = value
        End Set
    End Property

    ''' <summary>
    ''' The earliest acceptable date on which an item was last updated,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property LastUpdatedStartDate() As DateTime
        Get
            Return mLastUpdatedStartDate
        End Get
        Set(ByVal value As DateTime)
            mLastUpdatedStartDate = value
        End Set
    End Property

    ''' <summary>
    ''' The latest acceptable date on which an item was last updated,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property LastUpdatedEndDate() As DateTime
        Get
            Return mLastUpdatedEndDate
        End Get
        Set(ByVal value As DateTime)
            mLastUpdatedEndDate = value
        End Set
    End Property

    ''' <summary>
    ''' The earliest acceptable date on which an item was marked as an exception,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property ExceptionStartDate() As DateTime
        Get
            Return mExceptionStartDate
        End Get
        Set(ByVal value As DateTime)
            mExceptionStartDate = value
        End Set
    End Property

    ''' <summary>
    ''' The latest acceptable date on which an item was marked as an exception,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property ExceptionEndDate() As DateTime
        Get
            Return mExceptionEndDate
        End Get
        Set(ByVal value As DateTime)
            mExceptionEndDate = value
        End Set
    End Property

    ''' <summary>
    ''' The earliest acceptable date on which an item is deferred,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property NextReviewStartDate() As DateTime
        Get
            Return mNextReviewStartDate
        End Get
        Set(ByVal value As DateTime)
            mNextReviewStartDate = value
        End Set
    End Property

    ''' <summary>
    ''' The latest acceptable date on which an item is deferred, if any,
    ''' as it appears in the database (NB in particular UTC)
    ''' </summary>
    Public Property NextReviewEndDate() As DateTime
        Get
            Return mNextReviewEndDate
        End Get
        Set(ByVal value As DateTime)
            mNextReviewEndDate = value
        End Set
    End Property

    ''' <summary>
    ''' The search string for a resource working an item.
    ''' </summary>
    Public Property Resource() As String
        Get
            Return mResource
        End Get
        Set(ByVal value As String)
            mResource = value
        End Set
    End Property

    ''' <summary>
    ''' The search string for a status value of an item
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
    ''' The smallest acceptable number of attempts against an item
    ''' </summary>
    Public Property MinAttempt() As Integer
        Get
            Return mMinAttempt
        End Get
        Set(ByVal value As Integer)
            mMinAttempt = value
        End Set
    End Property

    ''' <summary>
    ''' The largest acceptable number of attempts against an item
    ''' </summary>
    Public Property MaxAttempt() As Integer
        Get
            Return mMaxAttempt
        End Get
        Set(ByVal value As Integer)
            mMaxAttempt = value
        End Set
    End Property

    ''' <summary>
    ''' The smallest acceptable priority
    ''' </summary>
    Public Property MinPriority() As Integer
        Get
            Return mMinPriority
        End Get
        Set(ByVal value As Integer)
            mMinPriority = value
        End Set
    End Property

    ''' <summary>
    ''' The largest acceptable priority
    ''' </summary>
    Public Property MaxPriority() As Integer
        Get
            Return mMaxPriority
        End Get
        Set(ByVal value As Integer)
            mMaxPriority = value
        End Set
    End Property

    ''' <summary>
    ''' The tags which are currently being filtered on as a string in the format
    ''' "+wanted tag;+other wanted tag;-unwanted tag".
    ''' When setting, if any tags don't match the required (extremely liberal)
    ''' format, they are skipped.
    ''' <seealso cref="clsTagMask">TagMask has a fuller explanation for applying
    ''' and masking tags.</seealso>
    ''' </summary>
    Public Property Tags() As String
        Get
            Return mTagger.ToString()
        End Get
        Set(ByVal value As String)
            mTagger.Clear()
            mTagger.ApplyTags(value)
        End Set
    End Property

    ''' <summary>
    ''' The order in which the items should be sorted
    ''' </summary>
    Public Property SortColumn() As QueueSortColumn
        Get
            Return mSortCol
        End Get
        Set(ByVal value As QueueSortColumn)
            mSortCol = value
        End Set
    End Property

    ''' <summary>
    ''' The order in which items should be returned.
    ''' </summary>
    Public Property SortOrder() As QueueSortOrder
        Get
            Return mSortOrd
        End Get
        Set(ByVal value As QueueSortOrder)
            mSortOrd = value
        End Set
    End Property

    ''' <summary>
    ''' The set of states currently set in this filter
    ''' </summary>
    Public ReadOnly Property StateFilter() As IBPSet(Of QueueItemState)
        Get
            If mStateFilter Is Nothing Then mStateFilter = New clsSet(Of QueueItemState)
            Return mStateFilter
        End Get
    End Property

    ''' <summary>
    ''' Checks if this filter has any states set in it.
    ''' </summary>
    Public ReadOnly Property HasAnyStates() As Boolean
        Get
            Return mStateFilter IsNot Nothing AndAlso mStateFilter.Count > 0
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Adds the given states to this filter.
    ''' </summary>
    ''' <param name="states">The states to add to the current item states set in
    ''' this filter.</param>
    Public Sub AddStates(ByVal ParamArray states() As QueueItemState)
        StateFilter.Union(states)
    End Sub

    ''' <summary>
    ''' Sets the given states into this filter
    ''' </summary>
    ''' <param name="states">The states to set in this filter, overwriting anything
    ''' that is currently set herein.</param>
    Public Sub SetStates(ByVal ParamArray states() As QueueItemState)
        StateFilter.Clear()
        AddStates(states)
    End Sub

    ''' <summary>
    ''' Clears any states which were previously set in this filter.
    ''' </summary>
    Public Sub ClearStates()
        mStateFilter = Nothing
    End Sub

    ''' <summary>
    ''' Adds the where clause to the given stringbuilder which can filter on
    ''' the given tags, either only showing items with the tags, or only
    ''' showing items without them.
    ''' </summary>
    ''' <param name="sb">The StringBuilder to which the where clause should
    ''' be appended</param>
    ''' <param name="tagCollection">The collection of tags which should be
    ''' filtered on.</param>
    ''' <param name="showThoseWithTags">True to indicate that only items
    ''' with the given tags should be shown; False to indicate that only
    ''' items <em>without</em> the given tags should be shown.</param>
    ''' <param name="prefix">The optional prefix for the BPAWorkQueueItem
    ''' table which will be prepended to any columns referenced in this
    ''' where clause.</param>
    Private Sub AddTagsWhereClause( _
     ByVal sb As StringBuilder, _
     ByVal tagCollection As ICollection(Of String), _
     ByVal showThoseWithTags As Boolean, _
     ByVal prefix As String)

        Dim modifier As String
        Dim paramName As String
        If showThoseWithTags Then
            modifier = ""
            paramName = "showtag"
        Else
            modifier = "not"
            paramName = "hidetag"
        End If

        Dim i As Integer = 0
        For Each tag As String In tagCollection
            Dim wildcarded As String = clsServer.ApplyWildcard(tag)

            ' Use the new view in order to catch virtual tags...
            sb.AppendFormat( _
             " and {0} exists (" & _
             "    select 1 " & _
             "    from BPViewWorkQueueItemTag it2 " & _
             "    where it2.queueitemident = {1}ident" & _
             "      and it2.tag {2} @{3}{4}" & _
             " ) ", _
             modifier, prefix, IIf(wildcarded Is tag, "=", "like"), paramName, i _
            )
            i += 1
        Next

    End Sub

    ''' <summary>
    ''' Appends the filters constraints to the given string builder
    ''' </summary>
    ''' <param name="sb">The string builder to which the constraints
    ''' in this filter should be appended</param>
    ''' <param name="prefix">The optional prefix for the BPAWorkQueueItem table.
    ''' This will be prepended to any BPAWorkQueueItem column referenced in the
    ''' where clause appended by this method.</param>
    Public Sub AppendWhereClause( _
     ByVal sb As StringBuilder, _
     Optional ByVal prefix As String = Nothing)

        If prefix IsNot Nothing Then prefix = prefix & "."

        If mLoadedStartDate <> MinSQLDate Then _
         sb.Append(" AND ").Append(prefix).Append("loaded >= @loadedstartdate")

        If mLoadedEndDate <> MaxSQLDate Then _
         sb.Append(" AND ").Append(prefix).Append("loaded <= @loadedenddate")

        If mExceptionStartDate <> MinSQLDate Then _
         sb.Append(" AND ").Append(prefix).Append("exception >= @exceptionstartdate")

        If mExceptionEndDate <> MaxSQLDate Then _
         sb.Append(" AND ").Append(prefix).Append("exception <= @exceptionenddate")

        If mNextReviewStartDate <> MinSQLDate Then _
         sb.Append(" AND ").Append(prefix).Append("deferred >= @nextreviewstartdate")

        If mNextReviewEndDate <> MaxSQLDate Then _
         sb.Append(" AND (").Append(prefix).Append("deferred is null OR ") _
           .Append(prefix).Append("deferred <= @nextreviewenddate)")

        ' Note that lastupdated doesn't use a prefix since it's a derived column
        If mLastUpdatedStartDate <> MinSQLDate Then _
         sb.Append(" AND ").Append(prefix).Append("lastupdated >= @lastupdatedstartdate")

        If mLastUpdatedEndDate <> MaxSQLDate Then _
         sb.Append(" AND ").Append(prefix).Append("lastupdated <= @lastupdatedenddate")

        If mCompletedStartDate <> MinSQLDate Then _
         sb.Append(" AND ").Append(prefix).Append("completed >= @completedstartdate")

        If mCompletedEndDate <> MaxSQLDate Then _
         sb.Append(" AND ").Append(prefix).Append("completed <= @completedenddate")

        If mMinAttempt > Integer.MinValue Then _
         sb.Append(" AND ").Append(prefix).Append("attempt >= @minattempt")

        If mMaxAttempt < Integer.MaxValue Then _
         sb.Append(" AND ").Append(prefix).Append("attempt <= @maxattempt")

        If mMinPriority > Integer.MinValue Then _
         sb.Append(" AND ").Append(prefix).Append("priority >= @minpriority")

        If mMaxPriority < Integer.MaxValue Then _
         sb.Append(" AND ").Append(prefix).Append("priority <= @maxpriority")

        If mStatus <> "" Then _
         sb.Append(" AND (").Append(prefix).Append("status is null OR ") _
           .Append(prefix).Append("status like @status)")

        If mTagger.OnTags.Count > 0 Then _
         AddTagsWhereClause(sb, mTagger.OnTags, True, prefix)

        If mTagger.OffTags.Count > 0 Then _
         AddTagsWhereClause(sb, mTagger.OffTags, False, prefix)

        If MinWorkTime > Integer.MinValue Then _
         sb.Append(" AND (").Append(prefix).Append("worktime >= @minworktime)")

        If MaxWorkTime < Integer.MaxValue Then _
         sb.Append(" AND (").Append(prefix).Append("worktime <= @maxworktime)")

        If ItemKey <> "" Then _
         sb.Append(" AND (").Append(prefix).Append("keyvalue is null OR ") _
           .Append(prefix).Append("keyvalue like @itemkey)")

        If Me.HasAnyStates Then
            sb.Append(" AND ").Append(prefix).Append("state in (")
            For Each state As QueueItemState In StateFilter
                sb.Append(CInt(state)).Append(","c)
            Next
            sb.Length -= 1
            sb.Append(")"c)
        End If
        If mResource <> "" Then _
         sb.Append(" AND runningresourcename like @resource")

        If mItemId <> Nothing Then _
         sb.Append(" AND ").Append(prefix).Append("id = @itemid")


    End Sub

    ''' <summary>
    ''' Gets the DB Name of the sort column which is set in this filter
    ''' </summary>
    ''' <returns>The name of the database column within the BPWorkQueueItem
    ''' table which is represented by the sort column setting in this
    ''' object</returns>
    Public Function GetSortColumnDBName() As String
        Select Case mSortCol
            Case QueueSortColumn.ByPosition
                Return "i.queuepositiondate"
            Case QueueSortColumn.ByAttempt
                Return "i.attempt"
            Case QueueSortColumn.ByResource
                Return "s.runningresourcename"
            Case QueueSortColumn.ByItemKey
                Return "i.keyvalue"
            Case QueueSortColumn.ByPriority
                Return "i.priority"
            Case QueueSortColumn.ByCompleted
                Return "i.completed"
            Case QueueSortColumn.ByExceptionDate
                Return "i.exception"
            Case QueueSortColumn.ByExceptionReason
                Return "i.exceptionreasonvarchar"
            Case QueueSortColumn.ByLastUpdatedDate
                Return "i.lastupdated"
            Case QueueSortColumn.ByLoadedDate
                Return "i.loaded"
            Case QueueSortColumn.ByNextReviewDate
                Return "i.deferred"
            Case QueueSortColumn.ByState
                Return "i.state"
            Case QueueSortColumn.ByStatus
                Return "i.status"
            Case QueueSortColumn.ByTags
                Return "i.tag"
            Case QueueSortColumn.ByWorkTime
                Return "i.worktime"
            Case Else 'including default
                Return "i.lastupdated"
        End Select
    End Function

    Private Function GetLocalDate(dateToConvert As DateTime, dateToCompare As Date) As DateTime

        If dateToConvert = dateToCompare Then
            Return dateToCompare
        Else
            Return DateTime.SpecifyKind(dateToConvert, DateTimeKind.Local).ToUniversalTime()
        End If

    End Function

    ''' <summary>
    ''' Adds the parameters set in this object into the given SQL command,
    ''' using the labels defined in the AppendWhereClause method
    ''' </summary>
    ''' <param name="cmd">The SQL command in which the parameters which
    ''' are held within this filter should be added.</param>
    Public Sub AddParams(ByVal cmd As SqlCommand)
        Dim params As SqlParameterCollection = cmd.Parameters

        params.AddWithValue("@loadedstartdate", GetLocalDate(mLoadedStartDate, MinSQLDate))
        params.AddWithValue("@loadedenddate", GetLocalDate(mLoadedEndDate, MaxSQLDate))
        params.AddWithValue("@exceptionstartdate", GetLocalDate(mExceptionStartDate, MinSQLDate))
        params.AddWithValue("@exceptionenddate", GetLocalDate(mExceptionEndDate, MaxSQLDate))
        params.AddWithValue("@nextreviewstartdate", GetLocalDate(mNextReviewStartDate, MinSQLDate))
        params.AddWithValue("@nextreviewenddate", GetLocalDate(mNextReviewEndDate, MaxSQLDate))
        params.AddWithValue("@lastupdatedstartdate", GetLocalDate(mLastUpdatedStartDate, MinSQLDate))
        params.AddWithValue("@lastupdatedenddate", GetLocalDate(mLastUpdatedEndDate, MaxSQLDate))
        params.AddWithValue("@completedstartdate", GetLocalDate(mCompletedStartDate, MinSQLDate))
        params.AddWithValue("@completedenddate", GetLocalDate(mCompletedEndDate, MaxSQLDate))
        params.AddWithValue("@status", "%" & mStatus & "%")
        params.AddWithValue("@minattempt", mMinAttempt)
        params.AddWithValue("@maxattempt", mMaxAttempt)
        params.AddWithValue("@minpriority", mMinPriority)
        params.AddWithValue("@maxpriority", mMaxPriority)
        params.AddWithValue("@minworktime", MinWorkTime)
        params.AddWithValue("@maxworktime", MaxWorkTime)
        params.AddWithValue("@itemkey", "%" & ItemKey & "%")
        params.AddWithValue("@resource", "%" & mResource & "%")
        params.AddWithValue("@itemid", mItemId)
        If mTagger.OnTags.Count > 0 Then
            Dim i As Integer = 0
            For Each tag As String In mTagger.OnTags
                params.AddWithValue("@showtag" & i, clsServer.ApplyWildcard(tag))
                i += 1
            Next
        End If
        If mTagger.OffTags.Count > 0 Then
            Dim i As Integer = 0
            For Each tag As String In mTagger.OffTags
                params.AddWithValue("@hidetag" & i, clsServer.ApplyWildcard(tag))
                i += 1
            Next
        End If

    End Sub

#End Region

End Class
