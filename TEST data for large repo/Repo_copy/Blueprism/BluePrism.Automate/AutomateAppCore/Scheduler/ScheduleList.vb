Imports BluePrism.Scheduling
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization

''' <summary>
''' Class describing a list of schedule instances - either a report, ie. a list of
''' schedule instances which have occurred, or a timetable, ie. a list of schedule
''' instances which are due to occur.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp", IsReference:=True)>
Public Class ScheduleList
    Inherits DescribedNamedObject

#Region "Events"

    ''' <summary>
    ''' Class to handle the events for this object.
    ''' This is here so that we can make a schedule list serializable and ensure that
    ''' the events are not serialized. In .NET 2.0 it was impossible to mark an
    ''' event as not serializable in VB; this was the workaround we used to be able
    ''' to do that.
    ''' </summary>
    Public Class ScheduleListEventHandler

        ''' <summary>
        ''' Event fired immediately before the name of this list is changed.
        ''' </summary>
        ''' <param name="args">The event arguments detailing the event.</param>
        Public Event NameChanging(ByVal args As ListRenameEventArgs)

        ''' <summary>
        ''' Fires the name changing event with the given arguments.
        ''' </summary>
        ''' <param name="args">The arguments detailing the name changing event
        ''' which is to be fired.</param>
        Friend Sub FireNameChanging(ByVal args As ListRenameEventArgs)
            RaiseEvent NameChanging(args)
        End Sub

    End Class

    ''' <summary>
    ''' Handler for the events which occur within this list.
    ''' </summary>
    Public ReadOnly Property Events() As ScheduleListEventHandler
        Get
            If mEventHandler Is Nothing Then mEventHandler = New ScheduleListEventHandler()
            Return mEventHandler
        End Get
    End Property

    ''' <summary>
    ''' The handler of the name changing event for this object.
    ''' </summary>
    <NonSerialized()> _
    Private mEventHandler As ScheduleListEventHandler

#End Region

#Region "Constructors"

    ''' <summary>
    ''' Creates a new schedule list object backed by the given store.
    ''' </summary>
    ''' <param name="store"></param>
    Public Sub New(ByVal store As DatabaseBackedScheduleStore)
        Me.New(ScheduleListType.Timetable, store, 0)
    End Sub

    ''' <summary>
    ''' Creates a new schedule list object of the given type, backed by the given
    ''' store.
    ''' </summary>
    ''' <param name="type">The type of schedule list to create</param>
    ''' <param name="store">The store which backs this list.</param>
    Public Sub New(ByVal type As ScheduleListType, ByVal store As DatabaseBackedScheduleStore)
        Me.New(type, store, 0)
    End Sub

    ''' <summary>
    ''' Creates a new schedule list object with no store. Note that this list
    ''' is considered non-functional until a store is set, and this constructor
    ''' should only really be called by the database or store code itself.
    ''' </summary>
    Friend Sub New()
        Me.New(ScheduleListType.Timetable, Nothing, 0)
    End Sub

    ''' <summary>
    ''' Creates a new schedule list object backed by the given store.
    ''' </summary>
    ''' <param name="type">The type of list that this object represents.</param>
    ''' <param name="store">The store that this list is registered on</param>
    ''' <param name="id">The ID of this list, or zero if it has no ID.</param>
    Public Sub New( _
     ByVal type As ScheduleListType, ByVal store As DatabaseBackedScheduleStore, ByVal id As Integer)
        mStore = store
        mID = id
        mScheduleListType = type
        ' Some sensible defaults
        mDaysDifference = 1
        mRelativeDate = ScheduleRelativeDate.Today
        mAbsoluteDate = Nothing
        mAllSchedules = True
    End Sub

#End Region

#Region "Member variables"

    ' The ID of this schedule list, zero if not saved on the database.
    <DataMember>
    Private mID As Integer

    ' The type of list that this object represents
    <DataMember>
    Private mScheduleListType As ScheduleListType

    ' The days to view schedule activity within
    <DataMember>
    Private mDaysDifference As Integer

    ' The relative date to base the list from
    <DataMember>
    Private mRelativeDate As ScheduleRelativeDate

    ' The absolute date to base the list from 
    <DataMember>
    Private mAbsoluteDate As DateTime

    ' Whether to examine all schedules or not.
    <DataMember>
    Private mAllSchedules As Boolean

    ' The schedule IDs to look at in the list.
    <DataMember>
    Private mScheduleIds As IBPSet(Of Integer)

    ' The store from which schedules and list data can be retrieved
    <NonSerialized()> _
    Private mStore As DatabaseBackedScheduleStore

#End Region

#Region "Properties"

    ''' <summary>
    ''' Overrides the Name in DescribedNamedObject to enable the appropriate
    ''' event to be fired.
    ''' </summary>
    Public Overrides Property Name() As String
        Set(ByVal value As String)
            If Not Object.Equals(Me.Name, value) Then
                Events.FireNameChanging(New ListRenameEventArgs(Me, Me.Name, value))
            End If
            MyBase.Name = value
        End Set
        Get
            Return MyBase.Name
        End Get
    End Property

    ''' <summary>
    ''' The unique ID for this clsScheduleList.
    ''' </summary>
    Public Property ID() As Integer
        Get
            Return mID
        End Get
        Set(ByVal value As Integer)
            If value <> mID Then MarkDataChanged("ID", mID, value)
            mID = value
        End Set
    End Property

    ''' <summary>
    ''' The type of this schedule list.
    ''' </summary>
    Public Property ListType() As ScheduleListType
        Get
            Return mScheduleListType
        End Get
        Set(ByVal value As ScheduleListType)
            If value <> mScheduleListType Then _
             MarkDataChanged("ListType", mScheduleListType, value)
            mScheduleListType = value
        End Set
    End Property

    ''' <summary>
    ''' The span of days which, when applied to the root date, makes up the date
    ''' range in this list.
    ''' </summary>
    Public Property DaysDistance() As Integer
        Get
            Return mDaysDifference
        End Get
        Set(ByVal value As Integer)
            If value <> mDaysDifference Then _
             MarkDataChanged("DaysDifference", mDaysDifference, value)
            mDaysDifference = value
        End Set
    End Property

    ''' <summary>
    ''' The relative date to use in this list.
    ''' </summary>
    Public Property RelativeDate() As ScheduleRelativeDate
        Get
            Return mRelativeDate
        End Get
        Set(ByVal value As ScheduleRelativeDate)
            If value <> mRelativeDate Then _
             MarkDataChanged("RelativeDate", mRelativeDate, value)
            mRelativeDate = value
        End Set
    End Property

    ''' <summary>
    ''' The absolute date to use in this list. Setting this to any value other than
    ''' <see cref="DateTime.MinValue"/> will force the <see cref="RelativeDate"/>
    ''' value to <see cref="ScheduleRelativeDate.None"/>.
    ''' </summary>
    Public Property AbsoluteDate() As DateTime
        Get
            Return mAbsoluteDate
        End Get
        Set(ByVal value As DateTime)
            If value <> Date.MinValue Then RelativeDate = ScheduleRelativeDate.None
            If value.Date <> mAbsoluteDate Then _
             MarkDataChanged("AbsoluteDate", mAbsoluteDate, value)
            mAbsoluteDate = value.Date
        End Set
    End Property

    ''' <summary>
    ''' The schedules which are in use in this list.
    ''' If the list is for 'all schedules' this will return all active schedules.
    ''' Note that this collection is read-only, to modify schedules in this list, use
    ''' the AddSchedule, RemoveSchedule, ClearSchedules commands.
    ''' </summary>
    Public ReadOnly Property Schedules() As ICollection(Of SessionRunnerSchedule)
        Get
            If AllSchedules Then Return mStore.GetActiveSessionRunnerSchedules()

            Dim list As New List(Of SessionRunnerSchedule)
            For Each id As Integer In ScheduleIds
                Dim sched As SessionRunnerSchedule = mStore.GetSessionRunnerSchedule(id)
                If sched IsNot Nothing Then list.Add(sched)
            Next
            Return list.AsReadOnly()

        End Get
    End Property

    ''' <summary>
    ''' Boolean to indicate whether all schedules should be included in the 
    ''' schedulelist
    ''' </summary>
    Public Property AllSchedules() As Boolean
        Get
            Return mAllSchedules
        End Get
        Set(ByVal value As Boolean)
            mAllSchedules = value
        End Set
    End Property

    ''' <summary>
    ''' The schedule IDs which are specified in this list.
    ''' If the list is for 'all schedules' this list may be empty.
    ''' </summary>
    Friend ReadOnly Property ScheduleIds() As IBPSet(Of Integer)
        Get
            If mScheduleIds Is Nothing Then mScheduleIds = New clsSortedSet(Of Integer)
            Return mScheduleIds
        End Get
    End Property

    ''' <summary>
    ''' The schedule store which is used by this list to maintain its collection
    ''' of schedules. This should be set outside the store / database in the 
    ''' list's constructor and only set using this property by the store /
    ''' database code.
    ''' </summary>
    Public Property Store() As DatabaseBackedScheduleStore
        Get
            Return mStore
        End Get
        Friend Set(ByVal value As DatabaseBackedScheduleStore)
            mStore = value
        End Set
    End Property

#End Region

#Region "Public Methods"

    ''' <summary>
    ''' Adds a schedule to this schedule list.
    ''' </summary>
    ''' <param name="sched">The schedule being added.</param>
    Public Sub AddSchedule(ByVal sched As SessionRunnerSchedule)
        MarkDataChanged("ScheduleAdded", Nothing, sched)
        ScheduleIds.Add(sched.Id)
    End Sub

    ''' <summary>
    ''' Remove the given schedule from this schedule list
    ''' </summary>
    ''' <param name="sched">The schedule being removed.</param>
    Public Sub RemoveSchedule(ByVal sched As SessionRunnerSchedule)
        MarkDataChanged("ScheduleRemoved", sched, Nothing)
        ScheduleIds.Remove(sched.Id)
    End Sub

    ''' <summary>
    ''' Clears all schedules from the schedules collection. This has the effect of
    ''' making the 
    ''' </summary>
    Public Sub ClearSchedules()
        MarkDataChanged("SchedulesCleared", Nothing, Nothing)
        ScheduleIds.Clear()
    End Sub

    ''' <summary>
    ''' Gets the "root date" of this list, ie the date from which this list
    ''' extends. For a timetable, this is the date at which the schedules are
    ''' listed from; for a report, this is the date up to which schedules are
    ''' listed.
    ''' </summary>
    ''' <returns>The date from which this list is rooted.</returns>
    Friend ReadOnly Property RootDate() As DateTime
        Get
            Select Case RelativeDate

                Case ScheduleRelativeDate.None
                    Return AbsoluteDate

                Case ScheduleRelativeDate.Today
                    Return Today

                Case ScheduleRelativeDate.Tomorrow
                    Return Today.AddDays(1)

                Case ScheduleRelativeDate.Yesterday
                    Return Today.AddDays(-1)

            End Select

        End Get
    End Property

    ''' <summary>
    ''' Sets the absolute date range in this list to that given.
    ''' Note that the list operates at a day level so the times on the list may
    ''' not match up to those given.
    ''' </summary>
    ''' <param name="startDate">The start date of the range.</param>
    ''' <param name="endDate">The end date of the range.</param>
    Public Sub SetDateRange(ByVal startDate As Date, ByVal endDate As Date)
        mRelativeDate = ScheduleRelativeDate.None

        If mScheduleListType = ScheduleListType.Timetable Then
            mAbsoluteDate = startDate
            mDaysDifference = (endDate - startDate).Days

        Else
            mDaysDifference = (endDate - startDate).Days
            mAbsoluteDate = endDate.AddDays(-1)

        End If
    End Sub

    ''' <summary>
    ''' Gets the start date for the range covered by this list.
    ''' Note that the start date is inclusive, ie. occurrences which happen
    ''' at exactly the time specified by this start date should be included
    ''' when searching
    ''' </summary>
    ''' <returns>The date which acts as the inclusive start date for this list
    ''' </returns>
    ''' <remarks>Note that if the root date of this list is undefined, then the
    ''' start date is undefined and this will return Date.MinValue.</remarks>
    Public Function GetStartDate() As Date

        ' Date range for a report is :
        ' RootDate - {DaysDistance days} + {1 day} => RootDate + {1 day}

        ' Date range for a timetable is :
        ' RootDate => RootDate + {DaysDistance days}

        ' The extra day on the start date (for reports) is because the days back
        ' include the root date, so if RootDate is today and DaysDistance = 1,
        ' the report should be for "Today => Tomorrow"
        ' so... Today - 1 day + 1 day => Today + 1 day

        ' The extra day on the end date is to ensure that the search is inclusive,
        ' ie.that any data for any days specified is included. This isn't 
        ' necessary for a timetable since adding the days includes the extra
        ' day necessary to make it inclusive : eg, if RootDate is today and 
        ' DaysDistance = 1, the timetable should be : "Today => Tomorrow"
        ' so.... Today => Today + 1 day

        Dim dt As Date = RootDate
        If dt = Date.MinValue OrElse dt = Date.MaxValue Then Return Date.MinValue

        If mScheduleListType = ScheduleListType.Report Then
            Return dt.AddDays(1 - mDaysDifference)
        Else
            Return dt
        End If

    End Function

    ''' <summary>
    ''' Gets the end date for the range covered by this list.
    ''' Note that the end date is exclusive, ie. occurrences which happen
    ''' at exactly the time specified by this end date should not be included.
    ''' Since the schedule works at 1 second granularity, a single second
    ''' subtracted from this date will result in an inclusive end date.
    ''' </summary>
    ''' <returns>The exclusive end date/time for this list.</returns>
    ''' <remarks>Note that if the root date of this list is undefined, then the
    ''' end date is undefined and this will return Date.MaxValue.</remarks>
    Public Function GetEndDate() As Date

        ' See the comment in the body of GetStartDate() to see how this
        ' method was arrived at.

        Dim dt As Date = RootDate
        If dt = Date.MinValue OrElse dt = Date.MaxValue Then Return Date.MaxValue

        If mScheduleListType = ScheduleListType.Report Then
            Return dt.AddDays(1)
        Else
            Return dt.AddDays(mDaysDifference)
        End If

    End Function

    ''' <summary>
    ''' Creates a copy of this schedule list with no ID, but with the same values as
    ''' this list. It will refer to the same store object (by reference) as this list
    ''' </summary>
    ''' <returns>A copy of this schedule list.</returns>
    Public Overridable Overloads Function Copy() As ScheduleList

        Dim clone As ScheduleList = DirectCast(MyBase.Copy(), ScheduleList)

        ' The ID should be zero-ised to indicate it is a) not the same
        ' as this schedule list, and b) not yet on the database.
        clone.mID = 0

        ' Reduce the schedule IDs to nothing to force a recreation of the 
        ' collection by the 'ScheduleIds' property. Union that collection
        ' with the IDs specified in this list.
        clone.mScheduleIds = Nothing
        clone.ScheduleIds.Union(Me.ScheduleIds)

        ' Create a new EventManager which has the effect of disassociating the
        ' cloned list from any listeners to the original.
        clone.mEventHandler = New ScheduleListEventHandler()

        ' The store is also not deep-cloned, since we want to keep the
        ' same reference object in the cloned version of the list.

        Return clone

    End Function

    ''' <summary>
    ''' Gets the entries which are on this list from the held schedule store.
    ''' </summary>
    ''' <returns>The collection of schedule instance objects representing the
    ''' entries in this list.</returns>
    ''' <exception cref="InvalidOperationException">If this list has no schedule
    ''' store set within it.</exception>
    Public Function GetEntries() As ICollection(Of IScheduleInstance)
        Return GetEntries(GetStartDate(), GetEndDate())
    End Function

    ''' <summary>
    ''' Gets the entries which correspond to this list and which fall within the
    ''' overridden dates specified. Note that dates within this list are ignored for
    ''' this call and the provided dates will be used in their place.
    ''' </summary>
    ''' <param name="startDate">The start date for which the entries are required.</param>
    ''' <param name="endDate">The end date for which the entries are required.</param>
    ''' <returns>The collection of schedule instance objects which are entries which
    ''' match this type of list for all the schedules specified within this list, and
    ''' which occur within the specified overriding date range.
    ''' </returns>
    Public Function GetEntries(ByVal startDate As Date, ByVal endDate As Date) _
     As ICollection(Of IScheduleInstance)

        If mStore Is Nothing Then
            Throw New InvalidOperationException("No store available to load the list entries from")
        End If

        Return mStore.GetListEntries(Me, startDate, endDate)

    End Function

#End Region

End Class
