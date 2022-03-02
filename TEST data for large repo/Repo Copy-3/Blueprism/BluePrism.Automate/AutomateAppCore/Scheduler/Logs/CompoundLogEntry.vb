


''' Project  : AutomateAppCore
''' Classes  : clsCompoundLogEntry
'''            clsTaskCompoundLogEntry
'''            clsSessionCompoundLogEntry
''' <summary>
''' Classes to provide a higher level view of a schedule log than individual log
''' entries - this provides a mechanism for combining disparate low level log entries
''' indicating individual events such as "task started", "session completed" etc, 
''' into single entries with all relevant information eg "task / start / end time".
''' This also provides a hierarchical view of the log such that entries for a
''' session appear within the task log entry representing the task that 'owns' the
''' session.
''' </summary>
''' <remarks>These classes were, in a previous life, LogEntry, TaskLogEntry and
''' SessionLogEntry; defined as nested classes in 
''' Automate/Forms/Scheduler/frmScheduleLogViewer. In order to use them in AutomateC
''' they were separated out into a different file and made public</remarks>
Public Class CompoundLogEntry
    Implements IComparable(Of CompoundLogEntry), IComparable

#Region "Member Variables"

    ' The name of the element represented by this entry
    Private mName As String

    ' The termination reason (if any) of this entry
    Private mTerminationReason As String

    ' The start date of this entry
    Private mStartDate As DateTime

    ' The end date of this entry
    Private mEndDate As DateTime

#End Region

#Region "Properties"

    ''' <summary>
    ''' The name of the element represented by this entry
    ''' </summary>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    ''' <summary>
    ''' The termination reason for this entry, or an empty string if it has no
    ''' termination reason.
    ''' </summary>
    Public Property TerminationReason() As String
        Get
            If mTerminationReason Is Nothing Then Return ""
            Return mTerminationReason
        End Get
        Set(ByVal value As String)
            mTerminationReason = value
        End Set
    End Property

    ''' <summary>
    ''' Start date for this entry, or Date.MinValue if it is unset.
    ''' </summary>
    Public Property StartDate() As Date
        Get
            Return mStartDate
        End Get
        Set(ByVal value As Date)
            mStartDate = value
        End Set
    End Property


    ''' <summary>
    ''' End date for this entry. Date.MinValue if it is unset.
    ''' </summary>
    Public Property EndDate() As DateTime
        Get
            Return mEndDate
        End Get
        Set(ByVal value As DateTime)
            mEndDate = value
        End Set
    End Property

#End Region

#Region "IComparable implementation"

    ''' <summary>
    ''' Compares this log entry to the given entry.
    ''' This compares first on start date. If they are equal, it compares on
    ''' end date. If they are equal, it compares on name.
    ''' </summary>
    ''' <param name="other">The other log entry to compare this to.</param>
    ''' <returns>A negative integer, zero or a positive integer if this log entry
    ''' is 'less than', 'equal to', or 'greater than' the other value, respectively.
    ''' </returns>
    Public Overridable Function CompareTo(ByVal other As CompoundLogEntry) As Integer _
     Implements IComparable(Of CompoundLogEntry).CompareTo

        ' Compare first by start date, then by end date, then by name.
        Dim comp As Integer = mStartDate.CompareTo(other.mStartDate)
        If comp <> 0 Then Return comp

        comp = mEndDate.CompareTo(other.mEndDate)
        If comp <> 0 Then Return comp

        If mName Is Nothing Then
            If other.mName Is Nothing Then Return 0
            Return -1
        ElseIf other.mName Is Nothing Then
            Return 1
        End If

        Return mName.CompareTo(other.mName)

    End Function

    ''' <summary>
    ''' Compares this log entry to the given entry.
    ''' This compares first on start date. If they are equal, it compares on
    ''' end date. If they are equal, it compares on name.
    ''' </summary>
    ''' <param name="obj">The other log entry to compare this to.</param>
    ''' <returns>A negative integer, zero or a positive integer if this log entry
    ''' is 'less than', 'equal to', or 'greater than' the other value, respectively.
    ''' </returns>
    ''' <exception cref="InvalidCastException">If the given object is not an
    ''' instance of clsCompoundLogEntry or a subclass.</exception>
    Private Function NonGenericCompareTo(ByVal obj As Object) As Integer _
     Implements IComparable.CompareTo
        Return CompareTo(DirectCast(obj, CompoundLogEntry))
    End Function

#End Region

End Class
