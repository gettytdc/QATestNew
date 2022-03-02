Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Images
Imports BluePrism.Core.Extensions

''' <summary>
''' Toolstrip menu item used to hold a 'Create Item' command within the group member
''' tree control. These are generated dynamically depending on the type of group
''' members supported by a tree within the control.
''' </summary>
Public Class CreateGroupMemberMenuItem : Inherits ToolStripMenuItem

    ' The member type that this menu item was created for
    Private mMemberType As GroupMemberType

    ''' <summary>
    ''' Creates a new 'Create' menuitem for the given group member type, which
    ''' delegates its click handling to the given event handler.
    ''' </summary>
    ''' <param name="tp">The group member type to create a menu item for.</param>
    ''' <param name="handler">The handler for handling <see cref="Click"/> events on
    ''' the newly minted menu item</param>
    Public Sub New(tp As GroupMemberType, handler As EventHandler)
        Dim tpName As String = tp.GetLocalizedFriendlyName()
        Name = "menuCreate" & tpName
        Text = String.Format(My.Resources.CreateGroupMemberMenuItem_Create0, tpName).ToSentenceCase()
        mMemberType = tp
        Tag = tp
        Image = ToolImages.Document_New_16x16
        If handler IsNot Nothing Then AddHandler Me.Click, handler
    End Sub

    ''' <summary>
    ''' Gets the member type that this menu item was created for
    ''' </summary>
    Public ReadOnly Property MemberType As GroupMemberType
        Get
            Return mMemberType
        End Get
    End Property

End Class
