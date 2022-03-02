Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' More specific log entry holding the session log entries pertinent to the task
''' referred to by the entry.
''' </summary>
Public Class TaskCompoundLogEntry
    Inherits CompoundLogEntry

    ' The sessions held in this task log.
    Private mSessions As New clsGeneratorDictionary(Of Integer, SessionCompoundLogEntry)

    ' List of messages indicating that a session failed to start
    Private mSessionCreateFailMessages As New List(Of KeyValuePair(Of Date, String))

#Region "Properties"

    ''' <summary>
    ''' The sessions held in this task log entry, keyed on session log ID.
    ''' Note that this reference needs to be the concrete 'generator dictionary'
    ''' class since there's no way to override behaviour in its parent dictionary
    ''' classes, meaning it has to resort to 'shadowing', meaning it acts differently
    ''' depending on what type it's cast as at any given time. Horrible, but true.
    ''' </summary>
    Public ReadOnly Property Sessions() As  _
     clsGeneratorDictionary(Of Integer, SessionCompoundLogEntry)
        Get
            Return mSessions
        End Get
    End Property

    ''' <summary>
    ''' List of messages indicating why any sessions within this task have failed
    ''' </summary>
    Public ReadOnly Property SessionCreationFailedMessages() As IList(Of KeyValuePair(Of Date, String))
        Get
            Return mSessionCreateFailMessages
        End Get
    End Property

    ''' <summary>
    ''' The sessions held in this task log entry, sorted into date order.
    ''' Note that changes made to the returned set will <em>not</em> be reflected in
    ''' this log entry.
    ''' </summary>
    Public ReadOnly Property SortedSessions() As IBPSet(Of SessionCompoundLogEntry)
        Get
            Return New clsSortedSet(Of SessionCompoundLogEntry)(mSessions.Values)
        End Get
    End Property

#End Region

End Class
