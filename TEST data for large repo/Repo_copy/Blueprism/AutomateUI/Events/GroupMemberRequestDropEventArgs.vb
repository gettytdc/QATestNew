
Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Delegate used to group member drop events
''' </summary>
Public Delegate Sub GroupMemberRequestDropEventHandler(
    sender As Object, e As GroupMemberRequestDropEventArgs)

''' <summary>
''' Event Args used to define a requested drop of a group member onto 
''' a new target group parent.
''' </summary>
Public Class GroupMemberRequestDropEventArgs : Inherits GroupMemberEventArgs

    ''' <summary>
    ''' The member being moved
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Member As IGroupMember

    ''' <summary>
    ''' Should we let the drop go ahead?
    ''' </summary>
    ''' <returns></returns>
    Public Property Cancel As Boolean

    ''' <summary>
    ''' Is this a copy, or move
    ''' </summary>
    ''' <returns></returns>
    Public Property IsCopy As Boolean

    ''' <summary>
    ''' Creates new event args describing a number of processes being run on a single
    ''' resource.
    ''' </summary>
    ''' <param name="member">The group member whom is being dragged</param>
    ''' <param name="target">The group member onto which the contents of the drag
    ''' operation were dropped.</param>
    ''' <param name="copy">A flag to signify if this action is a copy, or move</param>
    Public Sub New(member As IGroupMember, target As IGroupMember, copy As Boolean)
        MyBase.New(target)
        Me.Member = member
        Me.IsCopy = copy
    End Sub

End Class
