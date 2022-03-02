Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization

''' <summary>
''' Filter for session logs.
''' Note that the 'process type' isn't retrieved from the dictionary given in the
''' constructor or a call to <see cref="clsSessionLogFilter.SetFrom"/>; Neither is it
''' cleared after a call to <see cref="clsSessionLogFilter.Clear"/>
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class clsSessionLogFilter

#Region " Class scope declarations "

    ''' <summary>
    ''' The names of filters implemented in this class
    ''' </summary>
    Public Class FilterNames
        Public Shared SessionNo As String = "Session No"
        Public Shared StartDate As String = "Start"
        Public Shared EndDate As String = "End"
        Public Shared Process As String = "Process"
        Public Shared Status As String = "Status"
        Public Shared SourceLocn As String = "Started From"
        Public Shared TargetLocn As String = "Run On"
        Public Shared WindowsUser As String = "Windows User"
    End Class

#End Region

#Region " Member Vars "

    ' The session log number desired
    <DataMember>
    Private mSessNo As Integer

    ' The type of process to filter on
    <DataMember>
    Private mProcType As DiagramType

    ' The range within which the start date should fall
    <DataMember>
    Private mStartDateRange As clsDateRange

    ' The range within which the end date should fall
    <DataMember>
    Private mEndDateRange As clsDateRange

    ' The name of the process
    <DataMember>
    Private mProcessName As String

    ' The status string to search for
    <DataMember>
    Private mStatus As String

    ' The name of the resource that the session was started from
    <DataMember>
    Private mSourceResourceName As String

    ' The name of the resource that the session was run on
    <DataMember>
    Private mTargetResourceName As String

    ' The username of the windows user which ran the session
    <DataMember>
    Private mWindowsUser As String

    <DataMember>
    Private mRowsPerPage As Integer

    <DataMember>
    Private mCurrentPage As Integer

    <DataMember>
    Private mSortColumn As String

    <DataMember>
    Private mSortDirection As ListSortDirection


#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new, empty session log filter.
    ''' </summary>
    Public Sub New()
        Me.New(DiagramType.Unset, GetEmpty.IDictionary(Of String, Object))
    End Sub

    ''' <summary>
    ''' Creates a new session log filter of the given type.
    ''' </summary>
    ''' <param name="tp">The type of process to filter the logs on</param>
    Public Sub New(ByVal tp As DiagramType)
        Me.New(tp, GetEmpty.IDictionary(Of String, Object))
    End Sub

    ''' <summary>
    ''' Creates a new session log filter of the given type with the specified filter
    ''' values. The keys in the dictionary should correspond to the values in the
    ''' <see cref="FilterNames"/> class.
    ''' </summary>
    ''' <param name="tp">The type of process to filter on</param>
    ''' <param name="map">The map of filter values against the filter names defined
    ''' in this class.</param>
    Public Sub New(ByVal tp As DiagramType, ByVal map As IDictionary(Of String, Object))
        mProcType = tp
        SetFrom(map)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The session number set in this filter; 0 if unset.
    ''' </summary>
    Public ReadOnly Property SessionNo() As Integer
        Get
            Return mSessNo
        End Get
    End Property

    ''' <summary>
    ''' The process type to filter on; <see cref="DiagramType.Unset"/> if not set
    ''' </summary>
    Public Property ProcessType() As DiagramType
        Get
            Return mProcType
        End Get
        Set(ByVal value As DiagramType)
            mProcType = value
        End Set
    End Property

    ''' <summary>
    ''' The range that the start date should fall within. Null if not set.
    ''' </summary>
    Public ReadOnly Property StartDateRange() As clsDateRange
        Get
            Return mStartDateRange
        End Get
    End Property

    ''' <summary>
    ''' The range that the end date should fall within. Null if not set.
    ''' </summary>
    Public ReadOnly Property EndDateRange() As clsDateRange
        Get
            Return mEndDateRange
        End Get
    End Property

    ''' <summary>
    ''' The process name to filter on. Null if not set.
    ''' </summary>
    Public ReadOnly Property ProcessName() As String
        Get
            Return mProcessName
        End Get
    End Property

    ''' <summary>
    ''' The status of the session to filter on. Null if not set.
    ''' </summary>
    Public ReadOnly Property Status() As String
        Get
            Return mStatus
        End Get
    End Property

    ''' <summary>
    ''' The name of the resource which initiated the session to filter on.
    ''' Null if not set.
    ''' </summary>
    Public ReadOnly Property SourceResourceName() As String
        Get
            Return mSourceResourceName
        End Get
    End Property

    ''' <summary>
    ''' The name of the resource which ran the session to filter on. Null if not set.
    ''' </summary>
    Public ReadOnly Property TargetResourceName() As String
        Get
            Return mTargetResourceName
        End Get
    End Property

    ''' <summary>
    ''' The Windows username to filter on. Null if not set.
    ''' </summary>
    Public ReadOnly Property WindowsUser() As String
        Get
            Return mWindowsUser
        End Get
    End Property

    ''' <summary>
    ''' The Number of rows per page to display.
    ''' </summary>
    Public Property RowsPerPage() As Integer
        Get
            Return If(mRowsPerPage <= 0, -1, mRowsPerPage)
        End Get
        Set
            mRowsPerPage = Value
        End Set
    End Property

    ''' <summary>
    ''' The Current Page
    ''' </summary>
    Public Property CurrentPage() As Integer
        Get
            Return If(mCurrentPage <= 0, -1, mCurrentPage)
        End Get
        Set
            mCurrentPage = Value
        End Set
    End Property
    Public Property SortColumn() As String
        Get
            Return If(String.IsNullOrEmpty(mSortColumn), "", mSortColumn)
        End Get
        Set
            mSortColumn = Value
        End Set
    End Property

    Public Property SortDirection() As ListSortDirection
        Get
            Return If(mSortDirection = Nothing, ListSortDirection.Ascending, mSortDirection)
        End Get
        Set
            mSortDirection = Value
        End Set
    End Property


#End Region

#Region " Methods "

    ''' <summary>
    ''' Sets the filter values in this object to those provided by the given map.
    ''' </summary>
    ''' <param name="map">The dictionary of filter values keyed against the filter
    ''' names as defined in the <see cref="FilterNames"/> class.</param>
    Public Sub SetFrom(ByVal map As IDictionary(Of String, Object))
        Clear()
        ExtractValue(FilterNames.SessionNo, map, mSessNo)
        ExtractValue(FilterNames.StartDate, map, mStartDateRange)
        ExtractValue(FilterNames.EndDate, map, mEndDateRange)
        ExtractValue(FilterNames.Process, map, mProcessName)
        ExtractValue(FilterNames.Status, map, mStatus)
        ExtractValue(FilterNames.SourceLocn, map, mSourceResourceName)
        ExtractValue(FilterNames.TargetLocn, map, mTargetResourceName)
        ExtractValue(FilterNames.WindowsUser, map, mWindowsUser)
    End Sub

    ''' <summary>
    ''' Clears the filter values in this object, all except the process type - ie.
    ''' after calling this method, the process type is still constrained by whatever
    ''' value it held before the method was called.
    ''' </summary>
    Public Sub Clear()
        mSessNo = 0
        mStartDateRange = Nothing
        mEndDateRange = Nothing
        mProcessName = Nothing
        mStatus = Nothing
        mSourceResourceName = Nothing
        mTargetResourceName = Nothing
        mWindowsUser = Nothing
    End Sub

    ''' <summary>
    ''' Extracts the value from the dictionary keyed against the specified name,
    ''' doing nothing if the dictionary doesn't contain the specified name, or the
    ''' value in the dictionary is of an unexpected type.
    ''' </summary>
    ''' <typeparam name="T">The type of value expected in the map</typeparam>
    ''' <param name="name">The name of the map entry to look up</param>
    ''' <param name="map">The dictionary in which the key/value pair is potentially
    ''' held.</param>
    ''' <param name="into">The output location into which the value is extracted.
    ''' </param>
    Private Sub ExtractValue(Of T)(
     ByVal name As String, ByVal map As IDictionary(Of String, Object), ByRef into As T)
        Try
            If map.ContainsKey(name) Then into = DirectCast(map(name), T)
        Catch
        End Try
    End Sub

#End Region

End Class
