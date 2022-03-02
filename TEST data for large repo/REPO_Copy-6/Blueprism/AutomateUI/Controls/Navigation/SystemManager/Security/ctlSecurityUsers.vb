Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Images
Imports BluePrism.Server.Domain.Models

Public Class CtlSecurityUsers : Implements IHelp, IChild, IPermission, IMenuButtonHandler

#Region " Member vars "

    ''' <summary>
    ''' Indicates whether all users (including deleted users) should be displayed
    ''' </summary>
    Private mShowAllUsers As Boolean

    ' The application form hosting this control
    Private mParent As frmApplication

    ' The toolstrip menu item which allows the user to show/hide deleted users
    Friend WithEvents mShowAllUsersMenuItem As ToolStripMenuItem

    ' The user tree showing all users, including deleted (and system)
    Private mAllUserTree As IGroupTree

    ' The active user tree, filtered from mUserTree hiding system and deleted users
    Private mActiveUserTree As IGroupTree

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new Security/Users control
    ''' </summary>
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        mShowAllUsersMenuItem = New ToolStripMenuItem() With {
            .Text = My.Resources.ctlSecurityUsers_ShowAllUsers,
            .Image = AuthImages.Users_16x16
        }
        gtUsers.ExtraContextMenuItems.Add(mShowAllUsersMenuItem)

        panUser.Hide()

        ' Show the correct context menu items on the tree for native/AD system
        Dim isNativeDatabase = User.IsLoggedInto(DatabaseType.NativeAndExternal)
        gtUsers.ItemCreateEnabled = isNativeDatabase
        gtUsers.ItemDeleteEnabled = isNativeDatabase

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the application form ultimately hosting this control
    ''' </summary>
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the permissions required to view this form
    ''' </summary>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.Security.Users)
        End Get
    End Property

    ''' <summary>
    ''' The menu strip to use for the menu button; this just delegates to the detail
    ''' panel that it is currently showing.
    ''' </summary>
    Private ReadOnly Property MenuStrip As ContextMenuStrip Implements IMenuButtonHandler.MenuStrip
        Get
            If lvUsers.Visible Then
                Return lvUsers.MenuStrip
            ElseIf panUser.Visible Then
                Return panUser.MenuStrip
            Else
                Return Nothing
            End If
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets whether to display all users in this control
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
        "Show all users including deleted users")>
    Public Property ShowAllUsers As Boolean
        Get
            Return mShowAllUsers
        End Get
        Set(value As Boolean)
            If mShowAllUsers = value Then Return
            mShowAllUsers = value

            lvUsers.ShouldShowAll = value
            mShowAllUsersMenuItem.Checked = value

            gtUsers.ReplaceTree(
                If(value, mActiveUserTree, mAllUserTree),
                If(value, mAllUserTree, mActiveUserTree)
            )

            Dim gp = gtUsers.SelectedGroup
            Dim ugm = TryCast(gtUsers.SelectedMember, UserGroupMember)
            Select Case True
                Case gp IsNot Nothing : lvUsers.DisplayedGroup = gp
                Case ugm IsNot Nothing : panUser.UserMember = ugm
            End Select
            UpdateView()
        End Set
    End Property

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Make sure that 'Delete Item' is not available for already deleted members
    ''' </summary>
    ''' <remarks>FIXME: This is a bit of a hack and is entirely reliant on the menu
    ''' item never changing its name - not the end of the world if it's broken (any
    ''' attempt to delete an already deleted user ends up as a no-op), but it should
    ''' be done better.</remarks>
    Private Sub HandleContextMenuOpening(
     sender As Object, e As GroupMemberContexMenuOpeningEventArgs) _
     Handles gtUsers.ContextMenuOpening
        Dim mem = TryCast(e.Target, UserGroupMember)
        If mem Is Nothing Then Return

        For Each item As ToolStripItem In e.ContextMenu.Items
            If item.Name = "menuDeleteItem" Then
                item.Enabled = Not mem.IsDeleted AndAlso Not mem.AuthenticationServiceAccount()
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' Handles the visibility of the detail panel changing - ie. either the users
    ''' list or the user details panel is being shown/hidden. This ensures that the
    ''' correct menu strip is set into the menu button handler.
    ''' </summary>
    Private Sub HandleDetailVisibleChanged(sender As Object, e As EventArgs) _
     Handles lvUsers.VisibleChanged, panUser.VisibleChanged
        Dim sysman = GetAncestor(Of ctlSystemManager)()
        If sysman Is Nothing Then Return

        If lvUsers.Visible Then
            sysman.MenuButtonContextMenuStrip = lvUsers.MenuStrip

        ElseIf panUser.Visible Then
            sysman.MenuButtonContextMenuStrip = panUser.MenuStrip

        End If

    End Sub

    ''' <summary>
    ''' Handles the Entering of the users view, loading an updated view .
    ''' </summary>
    Private Sub HandleSecurityUsersEntered(sender As Object, e As EventArgs) _
     Handles Me.Enter
        UpdateView()
    End Sub

    ''' <summary>
    ''' Handles the loading of the user group tree, setting the tree to display into
    ''' it and adding appropriate handlers.
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        If DesignMode Then Return

        With GetGroupStore().GetTree(GroupTreeType.Users, Nothing, Nothing, True, False, False)
            mAllUserTree = .GetFilteredView(UserGroupMember.AllNonSystem)
            mActiveUserTree = .GetFilteredView(UserGroupMember.ActiveNonSystem)
        End With

        gtUsers.AddTree(If(ShowAllUsers, mAllUserTree, mActiveUserTree))
    End Sub

    ''' <summary>
    ''' Handles a 'Create Item' being requested from within the groups tree.
    ''' </summary>
    Private Sub HandleGroupMemberCreateRequested(
     sender As Object, e As CreateGroupMemberEventArgs) _
     Handles gtUsers.CreateRequested, lvUsers.CreateUserRequested
        CreateUser(AuthMode.Native)
    End Sub

    Private Sub CreateUser(authType As AuthMode)
        Dim f As New frmUserCreate(authType)
        ' if we are in a group, make sure we try to add the user to that group
        f.Group = If(gtUsers.SelectedGroup, gtUsers.SelectedMember.Owner)

        ' Make sure the user selector is updated when the form is closed
        AddHandler f.FormClosed,
            Sub()
                If f.DialogResult = DialogResult.OK Then
                    gtUsers.FlushGroupFromCache(f.Group)
                    UpdateView()
                End If
            End Sub
        ParentAppForm.StartForm(f, True)
    End Sub

    ''' <summary>
    ''' Handles a group member edit being requested by a child control
    ''' </summary>
    Private Sub HandleGroupMemberEditRequested(
     sender As Object, e As GroupMemberEventArgs) _
     Handles panUser.EditUserRequested, lvUsers.EditUserRequested, gtUsers.ItemActivated

        ' Double click event on tree is disabled for Active Directory environments
        If User.IsLoggedInto(DatabaseType.SingleSignOn) Then Return

        Dim ugm = TryCast(e.Target, UserGroupMember)
        If ugm Is Nothing Then Return

        Dim u = gSv.GetUser(ugm.IdAsGuid)
        If u Is Nothing Then
            UserMessage.Err(
                My.Resources.ctlSecurityUsers_CouldNotFindTheUser0OnTheDatabase, ugm.Name)
            Return
        End If

        Dim userIsCurrentUser = (User.Current.Id = u.Id)

        gtUsers.ClearGroupCache()

        If u.AuthType = AuthMode.External Then
            Using f As New frmExternalUserSettings() With {.User = u}
                Dim response = ParentAppForm.StartForm(f, True)
                If response = DialogResult.OK Then UpdateView()
            End Using
        ElseIf u.AuthType = AuthMode.MappedActiveDirectory Then

            Using f As New frmMappedActiveDirectoryUserSettings() With {.User = u}
                Dim response = ParentAppForm.StartForm(f, True)
                If response = DialogResult.OK Then UpdateView()
            End Using
        Else
            Using f As New frmUserSettings() With {.User = u}
                Dim response = ParentAppForm.StartForm(f, True)
                If response = DialogResult.OK Then
                    UpdateView()
                    If userIsCurrentUser AndAlso f.mUserLogin.HasUsernameChanged() Then
                        mParent.Logout()
                    End If
                End If
            End Using
        End If
    End Sub

    ''' <summary>
    ''' Refreshes the user group tree and ensures all the visual components update
    ''' their display of the data.
    ''' </summary>
    Private Sub UpdateView()
        gtUsers.ClearGroupCache()
        gtUsers.UpdateView(True)
        panUser.UserMember = TryCast(gtUsers.SelectedMember, UserGroupMember)
        lvUsers.DisplayedGroup = gtUsers.SelectedMembersGroup
    End Sub

    ''' <summary>
    ''' Instruct treeview control to refresh
    ''' </summary>
    Private Sub RefreshViewHandle() Handles gtUsers.RefreshView
        UpdateView()
    End Sub


    ''' <summary>
    ''' Refreshes the data displayed in this control and its child controls
    ''' </summary>
    Private Sub HandleRefreshRequested() _
     Handles panUser.RefreshRequested, lvUsers.RefreshRequested
        UpdateView()
    End Sub

    ''' <summary>
    ''' Handles a group member delete request coming in from the group tree.
    ''' </summary>
    Private Sub HandleDeleteRequested(sender As Object, e As GroupMemberEventArgs) _
     Handles gtUsers.DeleteRequested, panUser.DeleteUserRequested, lvUsers.DeleteUserRequested
        Dim mem = TryCast(e.Target, UserGroupMember)
        If mem IsNot Nothing Then DoDeleteUser(mem)
    End Sub

    ''' <summary>
    ''' Handles the AD user list refresh event fired from the  user list.
    ''' </summary>
    Private Sub userListView_ADUserListRefreshed(sender As Object, e As EventArgs) _
        Handles lvUsers.ADUserListRefreshed
        gtUsers.UpdateView(True)
    End Sub

    ''' <summary>
    ''' Toggles the value of mShowAllUsers and refreshes the group control and list 
    ''' view accordingly to show or hide deleted users as required.
    ''' </summary>
    Private Sub userGroupTree_ShowAllClicked(sender As Object, e As EventArgs) _
     Handles mShowAllUsersMenuItem.Click
        ShowAllUsers = Not ShowAllUsers
    End Sub

    ''' <summary>
    ''' Refreshes the group tree control after users are unlocked in the list view
    ''' and the details view.
    ''' </summary>
    Private Sub HandleUserUnlocked(sender As Object, e As GroupMemberEventArgs) _
     Handles lvUsers.UnlockUserRequested, panUser.UnlockUserRequested

        Dim mem = TryCast(e.Target, UserGroupMember)

        ' If there's nothing to unlock. Don't unlock it.
        If mem Is Nothing Then Return

        If gSv.UnlockUser(mem.IdAsGuid) Then
            mem.ResetLock()
            UserMessage.Show(String.Format(My.Resources.ctlSecurityUsers_User0WasUnlocked, mem.Name))
            gtUsers.UpdateView()
            lvUsers.UpdateView()
            panUser.UpdateView()

        Else
            UserMessage.Show(String.Format(My.Resources.ctlSecurityUsers_User0CouldNotBeUnlocked, mem.Name))

        End If

    End Sub

    ''' <summary>
    ''' Updates the display when a group is selected in the tree on the left, 
    ''' displaying the group's members in the list view on the right hand side.
    '''  </summary>
    Private Sub userGroupTree_GroupSelected(sender As Object, e As EventArgs) _
     Handles gtUsers.GroupSelected
        lvUsers.DisplayedGroup = gtUsers.SelectedGroup
        lvUsers.Show()
        panUser.Hide()
    End Sub

    ''' <summary>
    ''' Updates the display when a user is selected in the tree on the left, 
    ''' displaying the user details view on the right hand side.
    '''  </summary>
    Private Sub userGroupTree_ItemSelected(sender As Object, e As EventArgs) _
     Handles gtUsers.ItemSelected
        ' Check we have selected a user
        Dim mem = TryCast(gtUsers.SelectedMember, UserGroupMember)
        If mem Is Nothing Then Return

        panUser.UserMember = mem
        lvUsers.Hide()
        panUser.Show()

    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Gets the help file associated with this control
    ''' </summary>
    ''' <returns>The name of the web page within the help which describes this
    ''' control</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpSystemManUsers.htm"
    End Function

    ''' <summary>
    ''' Implements the deletion of the user after performing suitable checks on the
    ''' appropriateness of this action.
    ''' </summary>
    ''' <returns>Returns true if the deletion is carried out successfully, false
    ''' otherwise (eg if the user should not be deleted for whatever reason, or
    ''' if an exception occurs).</returns>
    Private Function DoDeleteUser(ByVal u As UserGroupMember) As Boolean

        If u Is Nothing Then
            UserMessage.Show(My.Resources.ctlSecurityUsers_PleaseFirstSelectAUserToBeDeleted)
            Return False
        End If

        Try
            Dim warningMessage As String = Nothing
            If u.AuthType = AuthMode.External Then
                warningMessage = My.Resources.ctlSecurityUsers_AreYouSureYouWantToDeleteExternalUser0ThisWillUnmapTheirExternalIdentity
            ElseIf u.AuthType = AuthMode.MappedActiveDirectory Then
                warningMessage = My.Resources.ctlSecurityUsers_AreYouSureYouWantToDeleteADUser0ThisWillUnmapTheirActiveDirectoryID
            Else
                warningMessage = My.Resources.ctlSecurityUsers_AreYouSureYouWantToDeleteUser0
            End If
            Dim response = UserMessage.YesNo(warningMessage, u.Name)
            If response <> MsgBoxResult.Yes Then Return False

            gSv.DeleteUser(u.IdAsGuid)
            gtUsers.FlushGroupFromCache(u.Owner)
            gtUsers.UpdateView(True)
            lvUsers.UpdateView()

            Return True

        Catch ex As Exception
            UserMessage.Err(ex, ex.Message)
            Return False
        End Try

    End Function

#End Region

End Class
