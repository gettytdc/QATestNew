Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Event handlers for clone item request events.
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event.</param>
Public Delegate Sub AddGroupToTreeNodeEventHandler(sender As Object,
                                                 e As AddingGroupTreeNodeEventArgs)

''' <summary>
''' Event args detailing a CloneGroupMember event.
''' This is triggered by the user requesting a clone of an existing item from a group
''' member tree, and typically, the member is created by the listener and set into
''' the event.
''' </summary>
Public Class AddingGroupTreeNodeEventArgs : Inherits EventArgs

    ' The source group member which is to be added to the tree
    Public ReadOnly Property Member As IGroupMember

    ''' <summary>
    ''' Creates a new eventargs object requesting adding the group member to the tree
    ''' </summary>
    ''' <param name="mem">The member which is to be added</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="mem"/> is null.
    ''' </exception>
    Public Sub New(mem As IGroupMember)
        If mem Is Nothing Then Throw New ArgumentNullException(NameOf(mem))
        Member = mem
    End Sub



    ''' <summary>
    ''' If set to true, the node will not be added
    ''' </summary>
    Public Property DoNotAdd As Boolean = False

End Class
