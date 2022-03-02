Imports System.Runtime.Serialization

''' <summary>
''' Class to encapsulate a date range
''' </summary>
<Serializable()>
<DataContract(Namespace:="bp")>
Public Class clsDateRange

    ''' <summary>
    '''  The start date in the date range.
    ''' </summary>
    <DataMember>
    Private _startTime As DateTime

    ''' <summary>
    ''' The end date in the date range.
    ''' </summary>
    <DataMember>
    Private _endTime As DateTime

    ''' <summary>
    ''' Creates a new date range with the given start and end dates.
    ''' </summary>
    ''' <param name="startTime">The start date in the range.</param>
    ''' <param name="endTime">The end date in the range.</param>
    Public Sub New(ByVal startTime As DateTime, ByVal endTime As DateTime)
        _startTime = startTime
        _endTime = endTime
    End Sub

    ''' <summary>
    ''' Creates a new date range with the given dynamic dates.
    ''' </summary>
    ''' <param name="startDaysFromToday">The number of days to add to today to
    ''' use as a start date, eg. <i>-1</i> indicates that yesterday at midnight
    ''' should act as the start date.</param>
    ''' <param name="endDaysFromToday">The number of days to add to today to
    ''' use as an end date, eg. <i>1</i> indicates that tomorrow at midnight
    ''' (ie. the next occurring midnight) should be used as the end date.
    ''' </param>
    Public Sub New(ByVal startDaysFromToday As Integer, ByVal endDaysFromToday As Integer)
        Me.New(Today.AddDays(startDaysFromToday), Today.AddDays(endDaysFromToday))
    End Sub

    ''' <summary>
    ''' The start date/time in the range.
    ''' </summary>
    Public ReadOnly Property StartTime() As DateTime
        Get
            Return _startTime
        End Get
    End Property

    ''' <summary>
    ''' The end date/time in the range.
    ''' </summary>
    Public ReadOnly Property EndTime() As DateTime
        Get
            If _endTime = Nothing Then Return Date.MaxValue
            Return _endTime
        End Get
    End Property

    ''' <summary>
    ''' Returns a string representation of this date range.
    ''' </summary>
    ''' <returns>A string representation of this date range.</returns>
    Public Overrides Function ToString() As String
        Return String.Format("{0} - {1}", _startTime, _endTime)
    End Function

    ''' <summary>
    ''' Checks if this date range is equal to the given object.
    ''' A range is considered equal to an object if that object is a DateRange
    ''' instance with the same start and end time as this range.
    ''' </summary>
    ''' <param name="o">The object to check for equality against.</param>
    ''' <returns>True if the given object is a non-null DateRange with the same
    ''' start and end times as this date range.</returns>
    Public Overrides Function Equals(ByVal o As Object) As Boolean
        Dim dr As clsDateRange = TryCast(o, clsDateRange)
        If dr Is Nothing Then Return False
        Return (_startTime = dr._startTime AndAlso _endTime = dr._endTime)
    End Function

    ''' <summary>
    ''' Gets a hash code representing this object.
    ''' </summary>
    ''' <returns>An integer hash of this object, based on the start and end dates
    ''' of the range.</returns>
    Public Overrides Function GetHashCode() As Integer
        Return (_startTime.GetHashCode() Xor _endTime.GetHashCode())
    End Function

End Class
