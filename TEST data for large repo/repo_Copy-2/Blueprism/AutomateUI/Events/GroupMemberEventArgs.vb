Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Event handlers for group member events.
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event.</param>
Public Delegate Sub GroupMemberEventHandler(sender As Object,
                                            e As GroupMemberEventArgs)

''' <summary>
''' Event Args detailing an event targeting a specific group member
''' </summary>
Public Class GroupMemberEventArgs : Inherits EventArgs

    ' The target group member that this event relates to
    Private mTarget As IGroupMember 

    'Whether the event triggers a reload to the treeview
    Private ReadOnly mReloadFromStore As Boolean = True

    ''' <summary>
    ''' Creates a new event args object referring to the given group member
    ''' </summary>
    ''' <param name="targ">The member to which the event refers</param>
    Public Sub New(targ As IGroupMember)
        mTarget = targ
    End Sub

    Public Sub New(targ As IGroupMember, reloadFromStore As Boolean)
        mTarget = targ
        mReloadFromStore = reloadFromStore
    End Sub

    ''' <summary>
    ''' The target group member to which this event refers.
    ''' </summary>
    Public ReadOnly Property Target As IGroupMember
        Get
            Return mTarget
        End Get
    End Property

    ''' <summary>
    ''' Flag that notifies whether to refresh tree
    ''' </summary>
    Public ReadOnly Property ReloadFromStore As Boolean
        Get
            Return mReloadFromStore
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether or not an UpdateView should be called after handling
    ''' </summary>
    Public Property CancelUpdate As Boolean
End Class
