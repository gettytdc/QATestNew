''' <summary>
''' Delegate used to describe a NameChanged event using the
''' <see cref="NameChangedEventArgs"/>
''' </summary>
''' <param name="sender">The object which is firing the name changed event.</param>
''' <param name="e">The args detailing the name change event.</param>
Public Delegate Sub NameChangedEventHandler( _
 ByVal sender As Object, ByVal e As NameChangedEventArgs)

''' <summary>
''' Event args detailing a name changed event
''' </summary>
Public Class NameChangedEventArgs : Inherits BaseNameChangeEventArgs

    ''' <summary>
    ''' Creates a new NameChanged event args object.
    ''' </summary>
    ''' <param name="oldName">The name being changed from</param>
    ''' <param name="newName">The name being changed to</param>
    Public Sub New(ByVal oldName As String, ByVal newName As String)
        MyBase.New(oldName, newName)
    End Sub

End Class
