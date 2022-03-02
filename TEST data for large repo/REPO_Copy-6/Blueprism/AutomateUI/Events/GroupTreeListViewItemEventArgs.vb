Imports AutomateControls.TreeList

''' <summary>
''' Handler for events which deal with group tree list view items
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event</param>
Public Delegate Sub GroupTreeListViewItemEventHandler(
    sender As Object, e As GroupTreeListViewItemEventArgs)

''' <summary>
''' Event args detailing an event which deals with group tree list view items.
''' </summary>
Public Class GroupTreeListViewItemEventArgs : Inherits EventArgs

    ' The item that the event refers to 
    Private mItem As TreeListViewItem

    ''' <summary>
    ''' Creates a new event args object based around the given item.
    ''' </summary>
    ''' <param name="it">The item to which the event refers</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="it"/> is null.
    ''' </exception>
    Public Sub New(it As TreeListViewItem)
        If it Is Nothing Then Throw New ArgumentNullException(NameOf(it))
        mItem = it
    End Sub

    ''' <summary>
    ''' The list view item to which the raised event pertains.
    ''' </summary>
    Public ReadOnly Property Item As TreeListViewItem
        Get
            Return mItem
        End Get
    End Property

End Class
