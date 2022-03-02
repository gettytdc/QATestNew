Imports BluePrism.BPCoreLib.Diary

''' <summary>
''' Diary entry source which uses 2 disparate entry sources for different
''' date ranges, and a pivot date to decide which source to use.
''' </summary>
Public Class CompoundScheduleListEntrySource
    Implements IDiaryEntrySource

    ' The source to use for entries before the pivot date.
    Private mBeforeSource As IDiaryEntrySource

    ' The source to use for entries after the pivot date.
    Private mAfterSource As IDiaryEntrySource

    ' The pivot date to use - null represents the current date/time.
    Private mPivot As Nullable(Of Date)

    ''' <summary>
    ''' Creates a new compound list entry source which uses the given entry
    ''' sources and a pivot date of the current date/time - ie. the date/time
    ''' at which the <see cref="GetEntries"/> method is called.
    ''' </summary>
    ''' <param name="beforeSrc">The source to use for diary entries before the
    ''' current date / time.</param>
    ''' <param name="afterSrc">The source to use for diary entries after the
    ''' current date / time.</param>
    Public Sub New( _
     ByVal beforeSrc As IDiaryEntrySource, ByVal afterSrc As IDiaryEntrySource)
        Me.New(beforeSrc, afterSrc, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new compound list entry source which uses the given entry sources
    ''' and a the provided pivot date to determine which source to use for specified
    ''' dates.
    ''' </summary>
    ''' <param name="beforeSrc">The source to use for diary entries before the
    ''' pivot date / time.</param>
    ''' <param name="afterSrc">The source to use for diary entries after the
    ''' pivot date / time.</param>
    ''' <param name="pivotTime">The date/time before which the 
    ''' <paramref name="beforeSrc"/> source should be used to retrieve entries, and
    ''' after which the <paramref name="afterSrc"/> should be used. If the given 
    ''' nullable argument has no value (ie. is null) then the point in time at which
    ''' <see cref="GetEntries"/> is called is used as the pivot.</param>
    Public Sub New( _
     ByVal beforeSrc As IDiaryEntrySource, ByVal afterSrc As IDiaryEntrySource, _
     ByVal pivotTime As Nullable(Of Date))

        mBeforeSource = beforeSrc
        mAfterSource = afterSrc
        mPivot = pivotTime

    End Sub

    ''' <summary>
    ''' Gets the entries from the compounded sources held within this object, which
    ''' correspond to the given date range.
    ''' </summary>
    ''' <param name="startDate">The date from which entries are required.</param>
    ''' <param name="endDate">The date up to which entries are required.</param>
    ''' <returns>The collection of diary entries corresponding to the given date
    ''' range.</returns>
    Public Function GetEntries(ByVal startDate As Date, ByVal endDate As Date) _
     As ICollection(Of IDiaryEntry) Implements IDiaryEntrySource.GetEntries

        Dim pivotDate As Date
        If mPivot.HasValue Then pivotDate = mPivot.Value Else pivotDate = Now

        ' Deal with the simple cases first - date range is fully before or after pivot date.
        If startDate > pivotDate Then
            Return mAfterSource.GetEntries(startDate, endDate)
        ElseIf endDate < pivotDate Then
            Return mBeforeSource.GetEntries(startDate, endDate)
        End If

        ' More awkward one - pivot date falls within range.
        ' We need to get [startDate - pivotDate] from before and [pivotDate - endDate] from after.
        Dim list As New List(Of IDiaryEntry)
        list.AddRange(mBeforeSource.GetEntries(startDate, pivotDate))
        list.AddRange(mAfterSource.GetEntries(pivotDate, endDate))
        Return list

    End Function
End Class
