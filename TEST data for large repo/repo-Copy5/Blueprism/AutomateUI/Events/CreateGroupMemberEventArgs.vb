Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Event handlers for new item request events.
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event.</param>
Public Delegate Sub CreateGroupMemberEventHandler(sender As Object,
                                               e As CreateGroupMemberEventArgs)

''' <summary>
''' Event args detailing a CreateGroupMember event.
''' This is triggered by the user requesting a new item of a particular member type,
''' and typically, the member is created by the listener and set into the event.
''' </summary>
Public Class CreateGroupMemberEventArgs : Inherits EventArgs

    ' The group in which the member is to be created
    Private mGroup As IGroup

    ' The type of group member requested
    Private mType As GroupMemberType

    ''' <summary>
    ''' Creates a new eventargs object requesting a new item of the given type
    ''' </summary>
    ''' <param name="gp">The group into which the new member is to be created</param>
    ''' <param name="tp">The type requested</param>
    Public Sub New(gp As IGroup, tp As GroupMemberType)
        mType = tp
        mGroup = gp
    End Sub

    ''' <summary>
    ''' The group member type requested by the user.
    ''' </summary>
    Public ReadOnly Property Type As GroupMemberType
        Get
            Return mType
        End Get
    End Property

    ''' <summary>
    ''' Gets the group into which the new member will be created
    ''' </summary>
    Public ReadOnly Property TargetGroup As IGroup
        Get
            Return mGroup
        End Get
    End Property

    ''' <summary>
    ''' The item created as a result of this request.
    ''' </summary>
    Public Property CreatedItem As IGroupMember

End Class
