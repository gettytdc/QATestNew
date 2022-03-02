Namespace Diary

    ''' <summary>
    ''' Interface which describes a source of diary entries.
    ''' </summary>
    Public Interface IDiaryEntrySource

        ''' <summary>
        ''' Gets all the diary entries which occur between the given dates
        ''' </summary>
        ''' <param name="startDate">The inclusive start date/time at which diary 
        ''' entries should be returned.</param>
        ''' <param name="endDate">The exclusive end date/time at which diary entries 
        ''' should be returned.</param>
        ''' <returns>The non-null collection of diary entries which occur
        ''' between the specified dates.</returns>
        ''' <remarks>If the dates are reversed, such that
        ''' <paramref name="startDate"/> is after <paramref name="endDate"/>, then
        ''' this will return an empty collection.</remarks>
        Function GetEntries(ByVal startDate As Date, ByVal endDate As Date) _
         As ICollection(Of IDiaryEntry)

    End Interface

End Namespace
