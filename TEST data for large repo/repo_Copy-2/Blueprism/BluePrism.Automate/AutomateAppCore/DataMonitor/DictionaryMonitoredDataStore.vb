
Namespace DataMonitor

    ''' <summary>
    ''' Data store which uses a dictionary for its backing data.
    ''' </summary>
    Public Class DictionaryMonitoredDataStore : Implements IMonitoredDataStore

        ' The versions mapped against their data names
        Private mVersions As IDictionary(Of String, Long)

        ''' <summary>
        ''' Creates a new data store using a backing dictionary.
        ''' </summary>
        Public Sub New()
            Me.New(New Dictionary(Of String, Long))
        End Sub

        ''' <summary>
        ''' Creates a new data store using a provided backing dictionary.
        ''' </summary>
        ''' <param name="dict">The dictionary to use hold the version data.</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="dict"/> is
        ''' null.</exception>
        Friend Sub New(dict As IDictionary(Of String, Long))
            If dict Is Nothing Then Throw New ArgumentNullException(NameOf(dict))
            mVersions = dict
        End Sub

        ''' <summary>
        ''' Increments the version associated with the given data name in this store.
        ''' If the name does not yet exist in this store, it is created and assigned
        ''' the version number '1'.
        ''' </summary>
        ''' <param name="name">The data name whose associated version should be
        ''' incremented.</param>
        ''' <returns>The new version number associated with the name.</returns>
        Public Function Increment(name As String) As Long
            Dim val As Long
            If Not mVersions.TryGetValue(name, val) Then
                mVersions(name) = 1L
                Return 1L
            End If
            val += 1
            mVersions(name) = val
            Return val
        End Function

        ''' <summary>
        ''' Gets the current state of all monitored data from the store
        ''' </summary>
        ''' <returns>The current version numbers for all monitored data, mapped
        ''' against the name of the data</returns>
        Public Function GetMonitoredData() As IDictionary(Of String, Long) _
         Implements IMonitoredDataStore.GetMonitoredData
            Return New Dictionary(Of String, Long)(mVersions)
        End Function

        ''' <summary>
        ''' Checks if the named data has been updated from the current version
        ''' number, returning true and setting the new version number if it has.
        ''' </summary>
        ''' <param name="name">The name of the data to check</param>
        ''' <param name="ver">On entry, the current version number held by the
        ''' calling context; on exit, the current version number set in the store.
        ''' </param>
        ''' <returns>True if the version number in the store was different to the one
        ''' given in the call to the method; False if they were the same value.
        ''' </returns>
        Public Function HasDataUpdated(name As String, ByRef ver As Long) As Boolean _
         Implements IMonitoredDataStore.HasDataUpdated
            Dim val As Long
            ' If the name does not exist in this store, by definition it has not been
            ' incremented. Or, indeed, created. But essentially not updated.
            If Not mVersions.TryGetValue(name, val) Then Return False
            ' If the version is the same as passed in, then it has not been updated
            If val = ver Then Return False
            ' Otherwise, there is a new version number
            ver = val
            Return True
        End Function

    End Class

End Namespace
