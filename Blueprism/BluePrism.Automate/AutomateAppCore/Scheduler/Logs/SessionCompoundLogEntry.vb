
''' <summary>
''' More specific log entry holding data regarding a session that the entry refers to
''' </summary>
Public Class SessionCompoundLogEntry
    Inherits CompoundLogEntry

#Region "Member variables"

    ' The resource name for the session represented by this entry
    Private mResourceName As String

    ' The session number for this entry
    Private mSessionNo As Integer

    Private mSessionId As Guid

#End Region

#Region "Properties"

    ''' <summary>
    ''' The name of the resource that the session represented by this entry ran on.
    ''' </summary>
    Public Property ResourceName() As String
        Get
            Return mResourceName
        End Get
        Set(ByVal value As String)
            mResourceName = value
        End Set
    End Property

    ''' <summary>
    ''' The session number of the session represented by this entry
    ''' </summary>
    Public Property SessionNo() As Integer
        Get
            Return mSessionNo
        End Get
        Set(ByVal value As Integer)
            ' Reset the cached session ID if the session number is changing...
            If mSessionNo <> value Then mSessionId = Nothing
            mSessionNo = value
        End Set
    End Property

    ''' <summary>
    ''' The GUID session ID of the session represented by this log entry.
    ''' </summary>
    Public ReadOnly Property SessionID() As Guid
        Get
            If mSessionId = Nothing Then
                mSessionId = gSv.GetSessionID(mSessionNo)
            End If
            Return mSessionId
        End Get
    End Property

#End Region

    ''' <summary>
    ''' Override of the clsCompoundLogEntry compare method to allow session number
    ''' to be checked if all else is the same (since sessions don't have 'names' as
    ''' such, this is potentially more likely than for tasks and schedules).
    ''' </summary>
    ''' <param name="entry">The log entry to compare against.</param>
    ''' <returns>A negative number, zero or a positive number if this object is 
    ''' 'less than', 'equal to' or 'greater than' the given entry.</returns>
    ''' <remarks>If the base class considers this object and the given entry as
    ''' equal and the given entry is not a session log entry, this method assumes
    ''' that this object is 'less than' the given entry.</remarks>
    Public Overrides Function CompareTo(ByVal entry As CompoundLogEntry) As Integer
        Dim comp As Integer = MyBase.CompareTo(entry)
        If comp <> 0 Then Return comp
        Dim sessEntry As SessionCompoundLogEntry = TryCast(entry, SessionCompoundLogEntry)
        If sessEntry Is Nothing Then Return -1
        Return mSessionNo - sessEntry.mSessionNo
    End Function

End Class

