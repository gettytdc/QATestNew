Imports System.Runtime.Serialization
''' Project  : BPCoreLib
''' Class    : clsTimeRange
''' <summary>
''' Create a range between 2 times.
''' </summary>
<Serializable, DataContract(Namespace:="bp")>
Public Class clsTimeRange

    ''' <summary>
    ''' The start time (inclusive) - ie. the first time to be included in this
    ''' range.
    ''' </summary>
    <DataMember>
    Private mStart As TimeSpan

    ''' <summary>
    ''' The end time (inclusive) - ie. the last time to be included in this range.
    ''' </summary>
    <DataMember>
    Private mEnd As TimeSpan

    ''' <summary>
    ''' Creates a time range from the start time up to and including the end time
    ''' </summary>
    ''' <param name="startTime">The first time which is to be valid in this range
    ''' </param>
    ''' <param name="endTime">The last time which is to be valid in this range.
    ''' </param>
    ''' <exception cref="ArgumentException">If the start time is after the end
    ''' time.</exception>
    Public Sub New(ByVal startTime As TimeSpan, ByVal endTime As TimeSpan)

        ' start must be <= end - otherwise world goes topsy-turvy.
        If startTime > endTime Then

            Throw New ArgumentException(String.Format( _
             "Start time must be before End time. Start: {0}; End: {1}", _
             startTime, endTime))

        End If

        mStart = startTime
        mEnd = endTime
    End Sub

    ''' <summary>
    ''' The (inclusive) start time of this time range.
    ''' </summary>
    Public ReadOnly Property StartTime() As TimeSpan
        Get
            Return mStart
        End Get
    End Property

    ''' <summary>
    ''' The (inclusive) end time of this time range.
    ''' </summary>
    Public ReadOnly Property EndTime() As TimeSpan
        Get
            Return mEnd
        End Get
    End Property

    ''' <summary>
    ''' Checks if this range contains the given time. True if the given time is
    ''' at or after the start time, and at or before the end time.
    ''' </summary>
    ''' <param name="time">The time to check is within this time range.</param>
    ''' <returns>true if the given time falls within the range specified in this
    ''' object; false otherwise.</returns>
    Public Function Contains(ByVal time As TimeSpan) As Boolean
        Return (time >= mStart AndAlso time <= mEnd)
    End Function

End Class
