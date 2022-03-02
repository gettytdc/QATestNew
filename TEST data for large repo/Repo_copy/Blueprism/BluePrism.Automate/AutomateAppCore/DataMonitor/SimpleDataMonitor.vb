Imports System.Collections.Concurrent
Imports BluePrism.BPCoreLib.Collections

Namespace DataMonitor

    ''' <summary>
    ''' Base implementation of a data monitor which handles the store and internal
    ''' mapping of version numbers to names, as well as providing the event mechanics
    ''' so that subclasses need only call the OnXXX() method to raise the appropriate
    ''' events.
    ''' </summary>
    Public Class SimpleDataMonitor : Implements IDataMonitor

#Region " Events "

        ''' <summary>
        ''' Event fired when an update to monitored data has been detected
        ''' </summary>
        Public Event MonitoredDataUpdated As MonitoredDataUpdateEventHandler _
         Implements IDataMonitor.MonitoredDataUpdated

#End Region

#Region " Member Variables "

        ' Flag set when this object is disposed
        Private mDisposed As Boolean

        ' The store to check the versions from
        Private mStore As IMonitoredDataStore

        ' The map containing the current version numbers against their data names
        Private mMap As IDictionary(Of String, Long)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new basic data monitor
        ''' </summary>
        ''' <param name="dataStore">The store to use to get the version numbers which
        ''' correspond to given data names</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="dataStore"/> is
        ''' null.</exception>
        Public Sub New(dataStore As IMonitoredDataStore)
            If dataStore Is Nothing Then Throw New ArgumentNullException(NameOf(dataStore))
            mStore = dataStore
            Map = GetSynced.IDictionary(dataStore.GetMonitoredData())
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The store used to access the version information about the monitored data.
        ''' </summary>
        Protected ReadOnly Property Store As IMonitoredDataStore
            Get
                Return mStore
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this monitor has been disposed or not
        ''' </summary>
        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return mDisposed
            End Get
        End Property

        ''' <summary>
        ''' The mapping of version number against the corresponding data names
        ''' </summary>
        Protected Property Map As IDictionary(Of String, Long)
            Get
                If mMap Is Nothing Then mMap = CreateMap(Nothing)
                Return mMap
            End Get
            Private Set(value As IDictionary(Of String, Long))
                mMap = value
            End Set
        End Property

#End Region

#Region " Methods "

        ''' <summary>
        ''' Polls the store for changes to the data, raising events as necessary for
        ''' any updated data. Note that any events raised from this call are executed
        ''' within this thread - ie. this method will not return until all events have
        ''' been handled for updated data.
        ''' </summary>
        Protected Sub Poll() Implements IDataMonitor.Poll
            If IsDisposed Then Return
            For Each pair In mStore.GetMonitoredData()
                Dim currVer As Long = 0
                ' If it's not there, then it's been added since this monitor was
                ' created - we have to assume a change
                If Not Map.TryGetValue(pair.Key, currVer) Then currVer = 0
                If pair.Value <> currVer Then
                    OnMonitoredDataUpdated(New MonitoredDataUpdateEventArgs(pair.Key))
                    Map(pair.Key) = pair.Value
                End If
            Next
        End Sub

        ''' <summary>
        ''' Creates a mapping of version numbers to data names for the given names
        ''' </summary>
        ''' <param name="names">The collection of names to create the map for</param>
        ''' <returns>A mapping of version numbers against the names given, all
        ''' initialised to zero.</returns>
        Private Function CreateMap(names As IEnumerable(Of String)) _
         As IDictionary(Of String, Long)
            Dim map As New ConcurrentDictionary(Of String, Long)
            If names IsNot Nothing Then
                For Each nm As String In names
                    map(nm) = 0
                Next
            End If
            Return map
        End Function

        ''' <summary>
        ''' Raises the <see cref="MonitoredDataUpdated"/> event.
        ''' </summary>
        ''' <param name="e">The args detailing the event</param>
        Protected Overridable Sub OnMonitoredDataUpdated(e As MonitoredDataUpdateEventArgs)
            RaiseEvent MonitoredDataUpdated(Me, e)
        End Sub

        ''' <summary>
        ''' Disposes of this data monitor
        ''' </summary>
        ''' <param name="explicit">True if being called as a result of the
        ''' <see cref="Dispose"/> method being called explicitly; False if being called
        ''' as a result of object finalization.</param>
        Protected Overridable Sub Dispose(explicit As Boolean)
            If Not mDisposed AndAlso explicit Then
                mStore = Nothing
                mMap = Nothing
            End If
            mDisposed = True
        End Sub

        ''' <summary>
        ''' Disposes of this object, ensuring all resources and threads are cleaned up
        ''' accordingly.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#End Region

    End Class

End Namespace
