Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Event handlers for clone item request events.
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event.</param>
Public Delegate Sub CloneGroupMemberEventHandler(sender As Object,
                                                 e As CloneGroupMemberEventArgs)

''' <summary>
''' Event args detailing a CloneGroupMember event.
''' This is triggered by the user requesting a clone of an existing item from a group
''' member tree, and typically, the member is created by the listener and set into
''' the event.
''' </summary>
Public Class CloneGroupMemberEventArgs : Inherits EventArgs

    ' The source group member which is to be cloned
    Private mMember As IGroupMember

    ''' <summary>
    ''' Creates a new eventargs object requesting a clone of the given member
    ''' </summary>
    ''' <param name="mem">The member which is to be cloned</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="mem"/> is null.
    ''' </exception>
    Public Sub New(mem As IGroupMember)
        If mem Is Nothing Then Throw New ArgumentNullException(NameOf(mem))
        mMember = mem
    End Sub

    ''' <summary>
    ''' The group member from which the clone is to be created
    ''' </summary>
    Public ReadOnly Property Original As IGroupMember
        Get
            Return mMember
        End Get
    End Property

    ''' <summary>
    ''' The item created as a result of this request.
    ''' </summary>
    Public Property CreatedItem As IGroupMember

End Class
