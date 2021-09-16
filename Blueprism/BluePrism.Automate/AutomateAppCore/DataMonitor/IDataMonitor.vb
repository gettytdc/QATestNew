Namespace DataMonitor

    ''' <summary>
    ''' Interface representing a data monitor - ie. an object which monitors some
    ''' data and raises events when that data is updated. The primary use case for
    ''' such an object is checking a table on the database for increments indicating
    ''' that named data has been updated and will need to be refreshed.
    ''' </summary>
    Public Interface IDataMonitor : Inherits IDisposable

        ''' <summary>
        ''' Event fired when an update to monitored data has been detected
        ''' </summary>
        Event MonitoredDataUpdated As MonitoredDataUpdateEventHandler

        ''' <summary>
        ''' Polls for changes to the monitored data, raising the
        ''' <see cref="MonitoredDataUpdated"/> as appropriate.
        ''' </summary>
        ''' <remarks>This has no effect if the monitor has been disposed of. Note
        ''' that this method does not throw an exception in this case either.
        ''' </remarks>
        Sub Poll()

    End Interface

End Namespace