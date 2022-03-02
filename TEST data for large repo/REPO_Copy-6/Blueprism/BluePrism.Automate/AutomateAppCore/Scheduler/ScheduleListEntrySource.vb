Imports BluePrism.BPCoreLib.Diary
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Implementation of a diary entry source which uses a base schedule list
''' to load diary entries from within specified date ranges.
''' </summary>
Public Class ScheduleListEntrySource
    Implements IDiaryEntrySource

    ''' <summary>
    ''' The schedule list base to use to get the schedules to display and the type
    ''' of entries required.
    ''' </summary>
    Private mBase As ScheduleList

    ''' <summary>
    ''' Creates a new schedule list entry source using the given list as a base.
    ''' Note that the list is copied on creation of this object, so any later
    ''' changes made to the list will not be reflected in this object.
    ''' </summary>
    ''' <param name="base">The schedule list which provides the schedules and the
    ''' list type to use as a diary entry source.</param>
    Public Sub New(ByVal base As ScheduleList)
        mBase = base.Copy()
    End Sub

    ''' <summary>
    ''' Gets the entries corresponding to the base entry source which fall in between
    ''' the two dates provided.
    ''' </summary>
    ''' <param name="startDate">The start date for which entries are required.</param>
    ''' <param name="endDate">The end date for which entries are required.</param>
    ''' <returns>The collection of diary entries which occur on the base list in the
    ''' specified date range.</returns>
    Public Function GetEntries(ByVal startDate As Date, ByVal endDate As Date) _
     As ICollection(Of IDiaryEntry) Implements IDiaryEntrySource.GetEntries

        Return New clsCovariantCollection(Of IDiaryEntry, IScheduleInstance) _
         (mBase.GetEntries(startDate, endDate))

    End Function

End Class
