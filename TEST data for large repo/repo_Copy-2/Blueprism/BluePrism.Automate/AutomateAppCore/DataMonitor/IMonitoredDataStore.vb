Namespace DataMonitor

    ''' <summary>
    ''' A store which provides access to monitored data held in some persisted area,
    ''' typically a database.
    ''' </summary>
    Public Interface IMonitoredDataStore

        ''' <summary>
        ''' Gets the current state of all monitored data from the store
        ''' </summary>
        ''' <returns>The current version numbers for all monitored data, mapped against
        ''' the name of the data</returns>
        Function GetMonitoredData() As IDictionary(Of String, Long)

        ''' <summary>
        ''' Checks if the named data has been updated from the current version number,
        ''' returning true and setting the new version number if it has.
        ''' </summary>
        ''' <param name="name">The name of the data to check</param>
        ''' <param name="ver">On entry, the current version number held by the calling
        ''' context; on exit, the current version number set in the store.</param>
        ''' <returns>True if the version number in the store was different to the one
        ''' given in the call to the method; False if they were the same value.</returns>
        Function HasDataUpdated(name As String, ByRef ver As Long) As Boolean

    End Interface

End Namespace