Imports BluePrism.BPCoreLib

''' Project  : AutomateAppCore
''' Class    : clsListRenameEventArgs
''' <summary>
''' Event arguments to hold details about a rename event occurring
''' on a clsScheduleList.
''' </summary>
Public Class ListRenameEventArgs : Inherits NameChangedEventArgs

    ' The list which is being renamed
    Private mList As ScheduleList

    ''' <summary>
    ''' Creates a new event args object holding details about the name changing
    ''' event of a schedule list.
    ''' </summary>
    ''' <param name="list">The list which is being renamed</param>
    ''' <param name="oldName">The old (ie. current) name of the list</param>
    ''' <param name="newName">The new name of the list</param>
    Public Sub New(ByVal list As ScheduleList, _
     ByVal oldName As String, ByVal newName As String)
        MyBase.New(oldName, newName)
        mList = list
    End Sub

    ''' <summary>
    ''' The list which is being renamed
    ''' </summary>
    Public ReadOnly Property Sender() As ScheduleList
        Get
            Return mList
        End Get
    End Property

End Class
