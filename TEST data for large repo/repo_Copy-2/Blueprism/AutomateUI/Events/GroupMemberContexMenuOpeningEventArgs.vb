Imports BluePrism.AutomateAppCore.Groups

Public Delegate Sub GroupMemberContexMenuOpeningEventHandler(sender As Object,
                                            e As GroupMemberContexMenuOpeningEventArgs)

''' <summary>
''' Event args detailing with a context menu opening on a group member
''' </summary>
Public Class GroupMemberContexMenuOpeningEventArgs : Inherits GroupMemberCancelEventArgs

    ' The context menu strip that is being opened
    Private mContextMenu As ContextMenuStrip

    ''' <summary>
    ''' Creates a new event args with the given properties
    ''' </summary>
    ''' <param name="targ">The target group member that the context menu is opening
    ''' with respect to</param>
    ''' <param name="menu">The context menu that is being opened</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="menu"/> is null.
    ''' </exception>
    Public Sub New(targ As IGroupMember, menu As ContextMenuStrip)
        Me.New(targ, False, menu)
    End Sub

    ''' <summary>
    ''' Creates a new event args with the given properties
    ''' </summary>
    ''' <param name="targ">The target group member that the context menu is opening
    ''' with respect to</param>
    ''' <param name="presetCancel">True to pre-set the <see cref="Cancel"/> property
    ''' to true in this args object; False to leave it at its default of False.
    ''' </param>
    ''' <param name="menu">The context menu that is being opened</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="menu"/> is null.
    ''' </exception>
    Public Sub New(targ As IGroupMember, presetCancel As Boolean, menu As ContextMenuStrip)
        MyBase.New(targ, presetCancel)
        If menu Is Nothing Then Throw New ArgumentNullException(NameOf(menu))
        mContextMenu = menu
    End Sub

    ''' <summary>
    ''' The context menu that is being opened.
    ''' </summary>
    Public ReadOnly Property ContextMenu As ContextMenuStrip
        Get
            Return mContextMenu
        End Get
    End Property

End Class
