Namespace DataMonitor


    ''' <summary>
    ''' A delegate representing an event handler for dealing with monitored data updates.
    ''' </summary>
    ''' <param name="sender">The source of the event, typically the monitor object which
    ''' is regularly checking for data updates.</param>
    ''' <param name="e">The args detailing the event.</param>
    Public Delegate Sub MonitoredDataUpdateEventHandler(
        sender As Object, e As MonitoredDataUpdateEventArgs)

    ''' <summary>
    ''' Args object detailing an event indicating that monitored data has been updated.
    ''' </summary>
    Public Class MonitoredDataUpdateEventArgs : Inherits EventArgs

        ''' <summary>
        ''' Creates a new EventArgs object representing a monitored data update for data
        ''' indicated by a specified name.
        ''' </summary>
        ''' <param name="nm">The name of the data that has updated, as defined in the
        ''' <c>BPADataTracker</c> table in the database.</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="nm"/> is null.
        ''' </exception>
        ''' <exception cref="ArgumentException">If <paramref name="nm"/> is blank.
        ''' </exception>
        Public Sub New(nm As String)
            If nm Is Nothing Then Throw New ArgumentNullException(NameOf(nm))
            nm = nm.Trim()
            If nm = "" Then Throw New ArgumentException("A name must be provided", NameOf(nm))
            Name = nm
        End Sub

        ''' <summary>
        ''' The name indicating which data has been updated.
        ''' </summary>
        Public ReadOnly Property Name As String

    End Class

End Namespace