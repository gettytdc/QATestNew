
''' <summary>
''' Event args detailing a list row changed event.
''' </summary>
Friend Class ListRowChangedEventArgs : Inherits EventArgs

    ' The old row, or null if there is no old row
    Private mOld As clsListRow

    ' The new row, or null if there is no new row
    Private mNew As clsListRow

    ''' <summary>
    ''' Creates a new event args object detailing a ListRowChanged event.
    ''' </summary>
    ''' <param name="rowOld">The old row - ie. the list row which has been changed
    ''' <em>from</em>. Null if there was no old row - ie. if there was no row
    ''' selected/editing before this event.</param>
    ''' <param name="rowNew">The new row - ie. the list row which has been changed
    ''' <em>to</em>. Null if there is no new row - ie. if the selection/editing is
    ''' being switched off in the hosting listview.</param>
    Public Sub New(ByVal rowOld As clsListRow, ByVal rowNew As clsListRow)
        mOld = rowOld
        mNew = rowNew
    End Sub

    ''' <summary>
    ''' The old row - ie. the list row which has been changed
    ''' <em>from</em>. Null if there was no old row - ie. if there was no row
    ''' selected/editing before this event.
    ''' </summary>
    Public ReadOnly Property OldRow() As clsListRow
        Get
            Return mOld
        End Get
    End Property

    ''' <summary>
    ''' The new row - ie. the list row which has been changed
    ''' <em>to</em>. Null if there is no new row - ie. if the selection/editing is
    ''' being switched off in the hosting listview.
    ''' </summary>
    Public ReadOnly Property NewRow() As clsListRow
        Get
            Return mNew
        End Get
    End Property
End Class

