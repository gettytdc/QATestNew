Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Delegate used to group member drop events
''' </summary>
Public Delegate Sub GroupMemberDropEventHandler(
    sender As Object, e As GroupMemberDropEventArgs)

''' <summary>
''' Event Args used to define a drop of group members onto a target group member.
''' </summary>
Public Class GroupMemberDropEventArgs : Inherits GroupMemberEventArgs

    ' The contents dropped onto the group member
    Private mContents As ICollection(Of IGroupMember)

    ''' <summary>
    ''' Creates new event args describing a number of processes being run on a single
    ''' resource.
    ''' </summary>
    ''' <param name="target">The group member onto which the contents of the drag
    ''' operation were dropped.</param>
    ''' <param name="contents">The contents of the drag operation</param>
    Public Sub New(target As IGroupMember, contents As ICollection(Of IGroupMember))
        MyBase.New(target)
        mContents = New List(Of IGroupMember)(contents)
    End Sub

    ''' <summary>
    ''' Creates new event args describing a process being run on a number of
    ''' resources.
    ''' </summary>
    ''' <param name="target">The group member onto which the contents of the drag
    ''' operation were dropped.</param>
    ''' <param name="contents">The contents of the drag operation</param>
    Public Sub New(ByVal target As IGroupMember, contents As IGroupMember)
        Me.New(target, GetSingleton.ICollection(contents))
    End Sub

    ''' <summary>
    ''' Gets the contents of the drag operation which formed this event.
    ''' </summary>
    Public ReadOnly Property Contents As ICollection(Of IGroupMember)
        Get
            Return GetReadOnly.ICollection(mContents)
        End Get
    End Property

End Class
