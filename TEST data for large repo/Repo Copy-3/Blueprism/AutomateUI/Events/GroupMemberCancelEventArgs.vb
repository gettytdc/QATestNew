
Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Delegate used to group member drop events
''' </summary>
Public Delegate Sub GroupMemberCancelEventHandler(
    sender As Object, e As GroupMemberCancelEventArgs)

''' <summary>
''' Event Args used to define a drop of group members onto a target group member.
''' </summary>
Public Class GroupMemberCancelEventArgs : Inherits GroupMemberEventArgs

    ''' <summary>
    ''' Gets or sets whether this operation should be cancelled
    ''' </summary>
    Public Overridable Property Cancel As Boolean

    ''' <summary>
    ''' Creates new event args describing a cancellable event occurring with a
    ''' particular target group member.
    ''' </summary>
    ''' <param name="targ">The group member for which the cancellable event is
    ''' occurring.</param>
    Public Sub New(targ As IGroupMember)
        Me.New(targ, False)
    End Sub

    ''' <summary>
    ''' Creates new event args describing a cancellable event occurring with a
    ''' particular target group member.
    ''' </summary>
    ''' <param name="targ">The group member for which the cancellable event is
    ''' occurring.</param>
    ''' <param name="presetCancel">The value to preset the <see cref="Cancel"/>
    ''' property to within these args</param>
    Public Sub New(ByVal targ As IGroupMember, presetCancel As Boolean)
        MyBase.New(targ)
        Cancel = presetCancel
    End Sub

End Class
