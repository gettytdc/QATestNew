
Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Delegate used to group member drop events
''' </summary>
Public Delegate Sub GroupMultipleMemberEventHandler(
    sender As Object, e As GroupMultipleMemberEventArgs)

''' <summary>
''' Event Args used to define an operation involving multiple group members.
''' </summary>
Public Class GroupMultipleMemberEventArgs : Inherits CancelEventArgs

    ' The members detailed in these args
    Private mMembers As ICollection(Of IGroupMember)

    ''' <summary>
    ''' Creates new event args describing a cancellable event occurring with a
    ''' particular target group member.
    ''' </summary>
    ''' <param name="mems">The group members for which the cancellable event is
    ''' occurring.</param>
    Public Sub New(mems As ICollection(Of IGroupMember))
        Me.New(mems, False)
    End Sub

    ''' <summary>
    ''' Creates new event args describing a cancellable event occurring with a
    ''' particular target group member.
    ''' </summary>
    ''' <param name="mems">The group members for which the cancellable event is
    ''' occurring.</param>
    ''' <param name="presetCancel">The value to preset the <see cref="Cancel"/>
    ''' property to within these args</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="mems"/> is null.
    ''' </exception>
    Public Sub New(
     ByVal mems As ICollection(Of IGroupMember), presetCancel As Boolean)
        MyBase.New(presetCancel)
        If mems Is Nothing Then Throw New ArgumentNullException(NameOf(Members))
        mMembers = mems
    End Sub

    ''' <summary>
    ''' The group members referred to by this event
    ''' </summary>
    Public ReadOnly Property Members As ICollection(Of IGroupMember)
        Get
            Return mMembers
        End Get
    End Property

End Class
